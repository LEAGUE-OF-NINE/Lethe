using System;
using Addressable;
using HarmonyLib;
using Il2CppSystem.IO;
using SD;
using UnityEngine;
using BepInEx.Logging;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.AddressableAssets;

namespace Lethe.Patches;

public class Skin : MonoBehaviour
{
    private static Il2CppSystem.Collections.Generic.List<AssetBundle> loadedAssets = new();
    private static ManualLogSource Log => LetheHooks.LOG;
    private static Harmony AbnoPatcher = new("AbnoPatcher");

    public static void Setup(Harmony harmony)
    {
        harmony.PatchAll(typeof(Skin));
        ClassInjector.RegisterTypeInIl2Cpp<Skin>();

        LetheHooks.LOG.LogInfo("Patching CreateSkin1....");
        var createSkin1 = typeof(SDCharacterSkinUtil).GetMethod(
        nameof(SDCharacterSkinUtil.CreateSkin),
        new Type[] { typeof(BattleUnitView), typeof(string), typeof(Transform), typeof(DelegateEvent).MakeByRefType() }
        );
        if (createSkin1 != null)
        {
            harmony.Patch(
                createSkin1,
                prefix: new HarmonyMethod(typeof(Skin).GetMethod(nameof(CreateSkin1)))
            );
            LetheHooks.LOG.LogInfo("Successfully patched CreateSkin1.");
        }
        else
        {
            LetheHooks.LOG.LogError("Failed to find CreateSkin1 method in SDCharacterSkinUtil.");
        }

        LetheHooks.LOG.LogInfo("Patching CreateSkin2....");
        var createSkin2 = typeof(SDCharacterSkinUtil).GetMethod(
            nameof(SDCharacterSkinUtil.CreateSkin),
            new Type[] { typeof(BattleUnitView), typeof(BattleUnitModel), typeof(Transform), typeof(DelegateEvent).MakeByRefType() }
        );
        if (createSkin2 != null)
        {
            harmony.Patch(
                createSkin2,
                prefix: new HarmonyMethod(typeof(Skin).GetMethod(nameof(CreateSkin2)))
            );
            LetheHooks.LOG.LogInfo("Successfully patched CreateSkin2.");
        }
        else
        {
            LetheHooks.LOG.LogError("Failed to find CreateSkin2 method in SDCharacterSkinUtil.");
        }
    }

    //load bundles
    //unload at end of stage/main scene
    [HarmonyPatch(typeof(GlobalGameManager), nameof(GlobalGameManager.LoadScene))]
    [HarmonyPrefix]
    public static void LoadScene(SCENE_STATE state, DelegateEvent onLoadScene)
    {
        Log.LogInfo($"SCENE: {state}");
        foreach (var key in AddressableManager.Instance._locationDicByKey)
        {
            Log.LogError(key);
        }
        switch (state)
        {
            case SCENE_STATE.Battle:
                {
                    foreach (var modPath in Directory.GetDirectories(LetheMain.modsPath.FullPath))
                    {
                        var expectedPath = Path.Combine(modPath, "custom_appearance");
                        if (!Directory.Exists(expectedPath)) continue;

                        foreach (var bundlePath in Directory.GetFiles(expectedPath, "*.bundle", SearchOption.AllDirectories))
                        {
                            Log.LogInfo($"{bundlePath}");
                            var bundle = AssetBundle.LoadFromFile(bundlePath, 0);
                            loadedAssets.Add(bundle);
                            Log.LogWarning(@$"loaded bundle {bundle?.name}!");
                        }
                    }
                    break;

                }
            case not SCENE_STATE.Battle:
                {
                    foreach (var bundle in loadedAssets)
                    {
                        if (bundle == null) continue;
                        LetheHooks.LOG.LogWarning($"unloading {bundle?.name}");
                        bundle?.Unload(false);
                    }
                    loadedAssets.Clear();
                    break;
                }

        }
    }

    [HarmonyPatch(typeof(StageController), nameof(StageController.EndStage))]
    [HarmonyPrefix]
    public static void UnloadBundlesOnStageEnd()
    {
        foreach (var bundle in loadedAssets)
        {
            if (bundle == null) continue;
            LetheHooks.LOG.LogWarning($"unloading {bundle?.name}");
            bundle?.Unload(false);
        }
        loadedAssets.Clear();
    }

    public static bool CreateSkin1(BattleUnitView view, string appearanceID, Transform parent, out DelegateEvent handle, ref CharacterAppearance __result)
    {
        // Custom logic to create a CharacterAppearance
        __result = CreateSkinForModel(view, appearanceID, parent);
        handle = null;
        return false;
    }

    public static bool CreateSkin2(BattleUnitView view, BattleUnitModel unit, Transform parent, out DelegateEvent handler, ref CharacterAppearance __result)
    {
        // Custom logic to create a CharacterAppearance
        __result = CreateSkinForModel(view, view._unitModel.GetAppearanceID(), parent);
        handler = null;
        return false;
    }

    public static bool PrefixAssetSync()
    {
        return true;
    }

    public static CharacterAppearance CreateSkinForModel(BattleUnitView view, string appearanceID, Transform parent)
    {
        var skinTypes = new[] { SDCharacterSkinUtil._LABEL_ABNORMALITY, SDCharacterSkinUtil._LABEL_ENEMY, SDCharacterSkinUtil._LABEL_PERSONALITY };
        Il2CppSystem.ValueTuple<GameObject, DelegateEvent> getSkin = null;
        var unit = view._unitModel;
        Log.LogInfo($"GETTING [{appearanceID}] for {unit.GetOriginUnitID().ToString()}!");

        foreach (var skinType in skinTypes)
        {
            if (appearanceID.StartsWith("!custom_"))
            {
                Log.LogInfo($"Loading custom skin: {appearanceID}");
                getSkin = loadSkin(appearanceID, parent);
            }
            else
            {
                getSkin = AddressableManager.Instance.LoadAssetSync<GameObject>(skinType, appearanceID, parent, null);
            }

            if (getSkin?.Item1 != null) break;
        }

        var characterAppearance = getSkin?.Item1?.GetComponent<CharacterAppearance>();

        if (characterAppearance == null)
        {
            Log.LogError("APPEARANCE NOT FOUND");
            return null;
        }

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

    private static Il2CppSystem.ValueTuple<UnityEngine.GameObject, DelegateEvent> loadSkin(string name, Transform parent)
    {
        string appearanceID = name.Substring("!custom_".Length);

        foreach (var bundle in loadedAssets)
        {
            foreach (var asset in bundle.AllAssetNames())
            {
                if (!asset.Contains(appearanceID)) continue;

                var loadAsset = bundle.LoadAsset(asset);
                var clone = Instantiate(loadAsset, parent.position, parent.rotation, parent).Cast<GameObject>();
                Log.LogInfo($"Found custom appearance {appearanceID}");

                // Correctly return a valid ValueTuple
                return new Il2CppSystem.ValueTuple<UnityEngine.GameObject, DelegateEvent>(clone, null);
            }
        }

        // Return a default ValueTuple instead of null
        return new Il2CppSystem.ValueTuple<UnityEngine.GameObject, DelegateEvent>(null, null);
    }

    public static bool PatchLoadAssetSync(string label, string resourceId, Transform parent, string releaseKey, ref Il2CppSystem.ValueTuple<UnityEngine.GameObject, DelegateEvent> __result)
    {
        Log.LogInfo($"LOADASSETSYNC FIRED, {label}, {resourceId}, {parent}, {releaseKey}");
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

