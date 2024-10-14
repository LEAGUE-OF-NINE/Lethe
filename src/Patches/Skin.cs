using System;
using Addressable;
using HarmonyLib;
using Il2CppSystem.IO;
using SD;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CustomEncounter.Patches;

public class Skin : MonoBehaviour
{
    
    public static void Setup(Harmony harmony)
    {
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
    
    public static void What() {}
    
    public static bool PatchLoadAssetSync(ref string label, ref string resourceId, Transform parent, ref Il2CppSystem.ValueTuple<UnityEngine.GameObject, DelegateEvent> __result)
    {
        CustomEncounterHook.LOG.LogInfo($"Loading {label}: {resourceId}");
        
        var prefix = "!abno_";
        if (resourceId.StartsWith(prefix))
        {
            label = SDCharacterSkinUtil._LABEL_ABNORMALITY;
            resourceId = resourceId.Substring(prefix.Length);
            CustomEncounterHook.LOG.LogInfo($"- Patched to {label}: {resourceId}");
            return true;
        }

        prefix = "!enemy_";
        if (resourceId.StartsWith(prefix))
        {
            label = SDCharacterSkinUtil._LABEL_ENEMY;
            resourceId = resourceId.Substring(prefix.Length);
            CustomEncounterHook.LOG.LogInfo($"- Patched to {label}: {resourceId}");
            return true;
        }

        prefix = "!identity_";
        if (resourceId.StartsWith(prefix))
        {
            label = SDCharacterSkinUtil._LABEL_PERSONALITY;
            resourceId = resourceId.Substring(prefix.Length);
            CustomEncounterHook.LOG.LogInfo($"- Patched to {label}: {resourceId}");
            return true;
        }

        prefix = "!custom_";
        if (resourceId.StartsWith(prefix))
        {
            var appearanceID = resourceId.Substring(prefix.Length);
            var prefabPath = "";
            if (appearanceID.Contains("/"))
            {
                var appearancePath = appearanceID.Split("/".ToCharArray(), 2);
                appearanceID = appearancePath[0];
                prefabPath = appearancePath[1];
            }

            var path = Path.Combine(CustomEncounterHook.CustomAppearanceDir.FullName, appearanceID + ".bundle");
            if (!File.Exists(path))
            {
                CustomEncounterHook.LOG.LogError("Cannot find asset bundle at: " + path);
                return true;
            }

            var bundle = AssetBundle.LoadFromFile(path);
            try
            {
                if (prefabPath == "")
                {
                    foreach (var assetName in bundle.AllAssetNames()) CustomEncounterHook.LOG.LogInfo("Found asset: " + assetName);

                    prefabPath = bundle.AllAssetNames()[0];
                }

                CustomEncounterHook.LOG.LogInfo("Using prefab: " + prefabPath);
                var asset = bundle.LoadAsset(prefabPath);
                var gameObject = Instantiate(asset, parent.position, parent.rotation, parent).Cast<GameObject>();
                __result = new();
                __result.Item1 = gameObject;
                __result.Item2 = (Action) What;

                CustomEncounterHook.LOG.LogError(appearanceID + ": appearance is null!?");
                return false;
            }
            finally
            {
                bundle.Unload(false);
            }
        }

        return true;
    }

    public static bool CreateSkin(BattleUnitView view, string appearanceID, Transform parent,
        ref CharacterAppearance __result)
    {
        var label = "";
        CustomEncounterHook.LOG.LogInfo($"Loading asset {label}: {appearanceID}");

        var prefix = "!custom_";
        if (appearanceID.StartsWith(prefix))
        {
            appearanceID = appearanceID.Substring(prefix.Length);
            var prefabPath = "";
            if (appearanceID.Contains("/"))
            {
                var appearancePath = appearanceID.Split("/".ToCharArray(), 2);
                appearanceID = appearancePath[0];
                prefabPath = appearancePath[1];
            }

            var path = Path.Combine(CustomEncounterHook.CustomAppearanceDir.FullName, appearanceID + ".bundle");
            if (!File.Exists(path))
            {
                CustomEncounterHook.LOG.LogError("Cannot find asset bundle at: " + path);
                return true;
            }

            var bundle = AssetBundle.LoadFromFile(path);
            try
            {
                if (prefabPath == "")
                {
                    foreach (var assetName in bundle.AllAssetNames()) CustomEncounterHook.LOG.LogInfo("Found asset: " + assetName);

                    prefabPath = bundle.AllAssetNames()[0];
                }

                CustomEncounterHook.LOG.LogInfo("Using prefab: " + prefabPath);
                var asset = bundle.LoadAsset(prefabPath);
                var gameObject = Instantiate(asset, parent.position, parent.rotation, parent).Cast<GameObject>();
                var appearance = gameObject.GetComponent<CharacterAppearance>();
                if (appearance != null)
                {
                    appearance.charInfo.appearanceID = appearanceID;
                    appearance.Initialize(view);
                    __result = appearance;
                    return false;
                }

                CustomEncounterHook.LOG.LogError(appearanceID + ": appearance is null!?");
            }
            finally
            {
                bundle.Unload(false);
            }
        }

        prefix = "!abno_";
        if (appearanceID.StartsWith(prefix))
        {
            label = SDCharacterSkinUtil._LABEL_ABNORMALITY;
            appearanceID = appearanceID.Substring(prefix.Length);
        }

        prefix = "!enemy_";
        if (appearanceID.StartsWith(prefix))
        {
            label = SDCharacterSkinUtil._LABEL_ENEMY;
            appearanceID = appearanceID.Substring(prefix.Length);
        }

        prefix = "!identity_";
        if (appearanceID.StartsWith(prefix))
        {
            label = SDCharacterSkinUtil._LABEL_PERSONALITY;
            appearanceID = appearanceID.Substring(prefix.Length);
        }

        if (label == "") return true;

        var res = AddressableManager.Instance.LoadAssetSync<GameObject>(label, appearanceID, parent);
        if (res == null) return true;

        var skin = res.Item1.GetComponent<CharacterAppearance>();
        if (skin == null) return true;
        skin.Initialize(view);
        skin.charInfo.appearanceID = appearanceID;
        __result = skin;
        return false;
    }

}