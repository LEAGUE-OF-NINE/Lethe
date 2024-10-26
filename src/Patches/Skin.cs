using System;
using Addressable;
using HarmonyLib;
using Il2CppSystem.IO;
using SD;
using UnhollowerRuntimeLib;
using UnityEngine;
using BepInEx.Logging;

namespace CustomEncounter.Patches;

public class Skin : MonoBehaviour
{
    private static Il2CppSystem.Collections.Generic.List<AssetBundle> loadedAssets = new();
    private static ManualLogSource Log => CustomEncounterHook.LOG;
    private static Harmony AbnoPatcher = new("AbnoPatcher");

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
        switch (state)
        {
            case SCENE_STATE.Battle:
            {
                foreach (var bundlePath in Directory.GetFiles(CustomEncounterHook.CustomAppearanceDir.FullName, "*.bundle"))
                {
                    Log.LogInfo($"{bundlePath}");
                    var bundle = AssetBundle.LoadFromFile(bundlePath, 0);
                    loadedAssets.Add(bundle);
                    Log.LogWarning(@$"loaded bundle {bundle.name}!");

                }
                break;
            }
        }
    }

    [HarmonyPatch(typeof(StageController), nameof(StageController.EndStage))]
    [HarmonyPostfix]
    private static void UnloadBundles()
    {
        foreach (var bundle in loadedAssets)
        {
            bundle.Unload(false);

        }
        loadedAssets.Clear();
    }
    
    //create skin
    [HarmonyPatch(typeof(SDCharacterSkinUtil), nameof(SDCharacterSkinUtil.CreateSkin))]
    [HarmonyPatch(new []{ typeof(BattleUnitView), typeof(BattleUnitModel), typeof(Transform), typeof(DelegateEvent) })]
    [HarmonyPrefix]
    private static bool CreateSkin(BattleUnitView view, BattleUnitModel unit, Transform parent, DelegateEvent handler, ref CharacterAppearance __result)
    {
        __result = CreateSkinForModel(view, view._unitModel.GetAppearanceID(), parent);
        return false;
    }

    [HarmonyPatch(typeof(SDCharacterSkinUtil), nameof(SDCharacterSkinUtil.CreateSkin))]
    [HarmonyPatch(new Type[] { typeof(BattleUnitView), typeof(string), typeof(Transform), typeof(DelegateEvent) })]
    [HarmonyPrefix]
    private static bool CreateSkin2(BattleUnitView view, string appearanceID, Transform parent, DelegateEvent handle, ref CharacterAppearance __result)
    {
        __result = CreateSkinForModel(view, appearanceID, parent);
        return false;
    }

    private static CharacterAppearance CreateSkinForModel(BattleUnitView view, string appearanceID, Transform parent)
    {
        var skinTypes = new[] { SDCharacterSkinUtil._LABEL_ABNORMALITY, SDCharacterSkinUtil._LABEL_ENEMY, SDCharacterSkinUtil._LABEL_PERSONALITY };
        Il2CppSystem.ValueTuple<GameObject, DelegateEvent> getSkin = null;
        var unit = view._unitModel;
        Log.LogInfo($"GETTING [{appearanceID}] for {unit.GetOriginUnitID().ToString()}!");

        foreach (var skinType in skinTypes)
        {
            getSkin = AddressableManager.Instance.LoadAssetSync<GameObject>(skinType, appearanceID, parent, null);
            if (getSkin?.Item1 != null) break;
        }

        var characterAppearance = getSkin?.Item1?.GetComponent<CharacterAppearance>();

        if (characterAppearance == null)
        {
            Log.LogError("APPEARANCE NULL");
            return null;
        }

        //view._appearances.Insert(0, characterAppearance);
        //view._curAppearance = characterAppearance;
        characterAppearance.name = appearanceID;
        characterAppearance.Initialize(view);
        characterAppearance.charInfo.appearanceID = appearanceID;
        if (unit._faction == UNIT_FACTION.PLAYER)
        {
            Fixes.FixAbnoAppearanceCrash(characterAppearance.GetIl2CppType().FullName, AbnoPatcher);
        }
        Log.LogInfo($"GOT SKIN {characterAppearance.name}");
        return characterAppearance;
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
                    if (!asset.Contains(appearanceID)) continue;
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

        return true;
    }

}
