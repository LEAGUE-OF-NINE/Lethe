using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using Addressable;
using SD;
using System.IO;
using UnhollowerRuntimeLib;
using UnityEngine.ProBuilder;


namespace LimbusSandbox.Patches
{
    internal class CustomAppearance : MonoBehaviour
    {
        private static Il2CppSystem.Collections.Generic.List<AssetBundle> loadedAssets = new();
        private static Harmony AbnoPatcher = new("AbnoPatcher");
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<CustomAppearance>();
            harmony.PatchAll(typeof(CustomAppearance));

            var prefix = typeof(CustomAppearance).GetMethod("PatchLoadAssetSync");
            var original = typeof(AddressableManager).GetMethod("LoadAssetSync").MakeGenericMethod(typeof(GameObject));
            harmony.Patch(original, new HarmonyMethod(prefix));
        }

        //load skin
        public static bool PatchLoadAssetSync(string label, string resourceId, Transform parent, string releaseKey, ref Il2CppSystem.ValueTuple<GameObject, DelegateEvent> __result)
        {     
            //Plugin.Log.LogWarning($"{__result} item1:[{__result.Item1}]  item2:[{__result.Item2}]");

            if (resourceId.StartsWith("!custom_"))
            {
                Plugin.Log.LogWarning($"FINDING {resourceId}");
                var appearanceID = resourceId.Substring("!custom_".Length);             
                foreach (var bundle in loadedAssets)
                {
                    foreach (var asset in bundle.AllAssetNames())
                    {
                        if (!asset.Contains(appearanceID)) continue;
                        var loadAsset = bundle.LoadAsset(asset);
                        var clone = Instantiate(loadAsset, parent.position, parent.rotation, parent).Cast<GameObject>();
                        Plugin.Log.LogInfo($"Found custom appearance {appearanceID}");

                        //new result
                        __result = new();
                        __result.Item1 = clone;

                        return false;
                    }
                }
            }

            return true;
        }

        //load bundles
        //unload at end of stage/main scene
        [HarmonyPatch(typeof(GlobalGameManager), nameof(GlobalGameManager.LoadScene))]
        [HarmonyPrefix]
        private static void LoadScene(SCENE_STATE state, DelegateEvent onLoadScene)
        {
            Plugin.Log.LogInfo($"SCENE: {state}");

            switch (state)
            {
                case SCENE_STATE.Battle:
                    {
                        foreach (var bundlePath in Directory.GetFiles(Plugin.customAppearancePath, "*.bundle"))
                        {

                            Plugin.Log.LogInfo($"{bundlePath}");
                            var bundle = AssetBundle.LoadFromFile(bundlePath, 0);
                            loadedAssets.Add(bundle);
                            Plugin.Log.LogWarning(@$"loaded bundle {bundle.name}!");
                        }
                        
                        break;
                    }
                case not SCENE_STATE.Battle:
                    {
                        foreach (var bundle in loadedAssets)
                        {
                            Plugin.Log.LogWarning($"unloading {bundle.name}");
                            bundle.Unload(false);
                        }
                        loadedAssets.Clear();                       
                        break;
                    }

            }
        }
        [HarmonyPatch(typeof(SDCharacterSkinUtil), nameof(SDCharacterSkinUtil.CreateSkin))]
        [HarmonyPatch(new[] {typeof(BattleUnitView), typeof(BattleUnitModel), typeof(Transform), typeof(DelegateEvent)})]
        [HarmonyPrefix]
        private static bool CreateSkin2(ref CharacterAppearance __result, BattleUnitView view, BattleUnitModel unit, Transform parent, DelegateEvent handler)
        {
            __result = CreateSkinForModel(view, unit.GetAppearanceID(), parent);
            return false;
        }
        [HarmonyPatch(typeof(SDCharacterSkinUtil), nameof(SDCharacterSkinUtil.CreateSkin))]
        [HarmonyPatch(new[] { typeof(BattleUnitView), typeof(string), typeof(Transform), typeof(DelegateEvent) })]
        [HarmonyPrefix]
        private static bool CreateSkin(ref CharacterAppearance __result, BattleUnitView view, string appearanceID, Transform parent, DelegateEvent handle)
        {
            __result = CreateSkinForModel(view, appearanceID, parent);
            return false;
        }

        private static CharacterAppearance CreateSkinForModel(BattleUnitView view, string appearanceID, Transform parent)
        {
            var skinTypes = new[] { SDCharacterSkinUtil._LABEL_ABNORMALITY, SDCharacterSkinUtil._LABEL_ENEMY, SDCharacterSkinUtil._LABEL_PERSONALITY };
            Il2CppSystem.ValueTuple<GameObject, DelegateEvent> getSkin = null;
            var unit = view._unitModel;
            Plugin.Log.LogInfo($"GETTING [{appearanceID}] for {unit.GetOriginUnitID().ToString()}!");

            foreach (var skinType in skinTypes)
            {
                getSkin = AddressableManager.Instance.LoadAssetSync<GameObject>(skinType, appearanceID, parent, null);
                if (getSkin?.Item1 != null) break;
            }

            var characterAppearance = getSkin?.Item1?.GetComponent<CharacterAppearance>();

            if (characterAppearance == null)
            {
                Plugin.Log.LogError("APPEARANCE IS NULL, USING BASE FAUST AS BACKUP!");
                return CreateSkinForModel(view, "10201_Faust_BaseAppearance", parent);
            }

            //view._appearances.Insert(0, characterAppearance);
            //view._curAppearance = characterAppearance;
            characterAppearance.name = appearanceID;
            characterAppearance.Initialize(view);
            characterAppearance.charInfo.appearanceID = appearanceID;
            if (unit._faction == UNIT_FACTION.PLAYER)
            {
                Fixes.AppearancePatch.FixAbnoAppearanceCrash(characterAppearance.GetIl2CppType().FullName, AbnoPatcher);
            }
            Plugin.Log.LogInfo($"GOT SKIN {characterAppearance.name}");
            return characterAppearance;
        }

    }


}
