using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Collections.ObjectModel;
using Il2CppSystem.IO;
using TextAssetPatch;
using UnhollowerRuntimeLib;
using FileInfo = System.IO.FileInfo;
using PatchInfo = TextAssetPatch.PatchInfo;

namespace Lethe.Patches;

public class TextAsset : Il2CppSystem.Object
{

    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<TextAsset>();
        harmony.PatchAll(typeof(TextAsset));
        foreach (var declaredMethod in AccessTools.GetDeclaredMethods(typeof(TextAssetPatchInfoManager)))
            if (declaredMethod.Name.StartsWith("Calculate"))
            {
                LetheHooks.LOG.LogInfo($"Patching {declaredMethod.Name}");
                harmony.Patch(declaredMethod, prefix: new HarmonyMethod(AccessTools.Method(typeof(TextAsset), nameof(PatchCalculation))));
            }
    }
    
    private static void PatchCalculation(PatchInfo remotePatchInfo)
    {
        foreach (var keyValuePair in remotePatchInfo.Files)
        {
            keyValuePair.value._Crc_k__BackingField = 0;
            keyValuePair.value._Hash_k__BackingField = "";
        }
    }


    [HarmonyPatch(typeof(TextAssetPatchInfoManager), nameof(TextAssetPatchInfoManager.CreatePatchFileInfo))]
    [HarmonyPrefix]
    private static bool PatchCreatePatchFileInfo(string filePath, ref PatchFileInfo __result)
    {
        // CustomEncounterHook.LOG.LogInfo($"Validating {filePath}");
        __result = new PatchFileInfo
        {
            _Crc_k__BackingField = 0,
            _Hash_k__BackingField = "",
            _Size_k__BackingField = new FileInfo(filePath).Length
        };
        return false;
    }
    
}