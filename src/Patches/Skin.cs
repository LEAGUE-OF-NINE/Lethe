using System;
using Addressable;
using HarmonyLib;
using Il2CppSystem.IO;
using SD;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using UnhollowerRuntimeLib;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using BepInEx.Logging;

namespace CustomEncounter.Patches;

public class Skin : MonoBehaviour
{
    private static Il2CppSystem.Collections.Generic.List<AssetBundle> loadedAssets = new();
    private static ManualLogSource Log => CustomEncounterHook.LOG;

    public static void Setup(Harmony harmony)
    {
        harmony.PatchAll(typeof(Skin));
        ClassInjector.RegisterTypeInIl2Cpp<Skin>();
        var original = typeof(AddressableManager)
            .GetMethod(nameof(AddressableManager.LoadAssetSync))?
            .MakeGenericMethod(typeof(UnityEngine.GameObject));
        if (original == null)
        {
            throw new Exception("Failed to find AddressableManager.LoadAssetSync");
        }

        harmony.Patch(
            original,
            prefix: new HarmonyMethod(typeof(Skin).GetMethod(nameof(PatchLoadAssetSync)))
        );
    }

    //load bundles
    //unload at end of stage/main scene
    [HarmonyPatch(typeof(GlobalGameManager), nameof(GlobalGameManager.LoadScene))]
    [HarmonyPrefix]
    private static void LoadScene(SCENE_STATE state, DelegateEvent onLoadScene)
    {
        Log.LogInfo($"SCENE: {state}");
        if (state == SCENE_STATE.Battle)
        {
            foreach (var bundlePath in Directory.GetFiles(CustomEncounterHook.CustomAppearanceDir.FullName, "*.bundle"))
            {
                Log.LogInfo($"{bundlePath}");
                var bundle = AssetBundle.LoadFromFile(bundlePath, 0);
                loadedAssets.Add(bundle);
                Log.LogWarning(@$"loaded bundle {bundle.name}!");

            }
        }
        else if (state == SCENE_STATE.Main)
        {
            foreach (var bundle in loadedAssets)
            {
                bundle.Unload(false);

            }
            loadedAssets.Clear();
        }
    }

    //create skin
    [HarmonyPatch(typeof(SDCharacterSkinUtil), nameof(SDCharacterSkinUtil.CreateSkin))]
    [HarmonyPatch(new Type[] { typeof(BattleUnitView), typeof(BattleUnitModel), typeof(Transform), typeof(DelegateEvent) })]
    [HarmonyPrefix]
    private static bool CreateSkin(BattleUnitView view, BattleUnitModel unit, Transform parent, DelegateEvent handler, ref CharacterAppearance __result)
    {

        CharacterAppearance characterAppearance;
        var skinType = SDCharacterSkinUtil._LABEL_ABNORMALITY;

        var appearanceID = unit.GetAppearanceID();
        //appearanceID = "10306_Donquixote_MiddleFingerAppearance";
        Log.LogInfo($"GETTING [{appearanceID}] for {unit.GetOriginUnitID().ToString()}!");

        //shit but whatever
        //load abnormality skin
        var getSkin = AddressableManager.Instance.LoadAssetSync<GameObject>(skinType, appearanceID, parent, null);

        //load enemy skin
        if (getSkin.Item1 == null && skinType == SDCharacterSkinUtil._LABEL_ABNORMALITY)
        {
            skinType = SDCharacterSkinUtil._LABEL_ENEMY;
            getSkin = AddressableManager.Instance.LoadAssetSync<GameObject>(skinType, appearanceID, parent, null);
        }

        //load personality skin
        if (getSkin.Item1 == null && skinType == SDCharacterSkinUtil._LABEL_ENEMY)
        {
            skinType = SDCharacterSkinUtil._LABEL_PERSONALITY;
            getSkin = AddressableManager.Instance.LoadAssetSync<GameObject>(skinType, appearanceID, parent, null);
        }

        characterAppearance = getSkin.Item1.GetComponent<CharacterAppearance>();
        if (characterAppearance != null)
        {
            //view._appearances.Insert(0, characterAppearance);
            //view._curAppearance = characterAppearance;
            characterAppearance.Initialize(view);
            characterAppearance.charInfo.appearanceID = appearanceID;
            __result = characterAppearance;

            Log.LogInfo($"GOT SKIN {characterAppearance.name}");
            return false;
        }
        else
        {
            Log.LogError("APPERANCE NULL");
            return true;
        }

    }

    public static bool PatchLoadAssetSync(string label, string resourceId, Transform parent, string releaseKey, ref Il2CppSystem.ValueTuple<UnityEngine.GameObject, DelegateEvent> __result)
    {
        //Log.LogInfo($"LOADASSETSYNC FIRED, {label}, {resourceId}, {parent}, {releaseKey}");
        string appearanceID;

        if (resourceId.StartsWith("!custom_"))
        {
            appearanceID = resourceId.Substring("!custom_".Length);
            foreach (var bundle in loadedAssets)
            {
                foreach (var asset in bundle.AllAssetNames())
                {
                    if (asset.Contains(appearanceID))
                    {
                        var loadAsset = bundle.LoadAsset(asset);
                        var clone = Instantiate(loadAsset, parent.position, parent.rotation, parent).Cast<GameObject>();
                        Log.LogInfo($"Found custom appearance {appearanceID}");

                        //new result
                        __result = new();
                        __result.Item1 = clone;

                        return false;
                    }
                }
            }
        }

        return true;
    }

}