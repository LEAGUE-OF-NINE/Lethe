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

}