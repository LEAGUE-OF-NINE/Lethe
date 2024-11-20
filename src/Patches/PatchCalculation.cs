using HarmonyLib;
using UnhollowerRuntimeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAssetPatch;
using UnityEngine;

namespace LimbusSandbox.Patches
{
    internal class PatchCalculation : MonoBehaviour
    {
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<PatchCalculation>();
            harmony.PatchAll(typeof(PatchCalculation));
        }

        [HarmonyPatch(typeof(TextAssetPatchInfoManager), nameof(TextAssetPatchInfoManager.CalculateMissingFiles))]
        [HarmonyPrefix]
        private static void CalculateMissingFiles(TextAssetPatch.PatchInfo remotePatchInfo)
        {
            _PatchCalculation(remotePatchInfo);
        }

        [HarmonyPatch(typeof(TextAssetPatchInfoManager), nameof(TextAssetPatchInfoManager.CalculateRemovedFiles))]
        [HarmonyPrefix]
        private static void CalculateRemovedFiles(TextAssetPatch.PatchInfo remotePatchInfo, TextAssetPatch.PatchInfo localPatchInfo)
        {
            _PatchCalculation(remotePatchInfo);
        }

        [HarmonyPatch(typeof(TextAssetPatchInfoManager), nameof(TextAssetPatchInfoManager.CalculateUpdatedFiles))]
        [HarmonyPrefix]
        private static void CalculateUpdatedFiles(TextAssetPatch.PatchInfo remotePatchInfo, TextAssetPatch.PatchInfo localPatchInfo)
        {
            _PatchCalculation(remotePatchInfo);
        }

        [HarmonyPatch(typeof(TextAssetPatchInfoManager), nameof(TextAssetPatchInfoManager.CreatePatchFileInfo))]
        [HarmonyPrefix]
        private static bool PatchCreatePatchFileInfo(string filePath, ref PatchFileInfo __result)
        {
            __result = new()
            {
                _Crc_k__BackingField = 0,
                _Hash_k__BackingField = "",
                _Size_k__BackingField = new FileInfo(filePath).Length
            };
            
            return false;
        }

        private static void _PatchCalculation(TextAssetPatch.PatchInfo remotePatchInfo)
        {
            foreach (var keyValuePair in remotePatchInfo.Files)
            {
                keyValuePair.value._Crc_k__BackingField = 0;
                keyValuePair.value._Hash_k__BackingField = "";
            }
        }
    }
}
