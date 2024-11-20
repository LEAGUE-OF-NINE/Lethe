using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace LimbusSandbox.Fixes
{
    internal class AppearancePatch : MonoBehaviour
    {
        private static Dictionary<string, Type> _abnoTypes = new();
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<AppearancePatch>();
            harmony.PatchAll(typeof(AppearancePatch));

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes().Where(it => !(it.IsSealed && it.IsAbstract) && it.IsSubclassOf(typeof(AbnormalityAppearance))))
                {
                    if (type.FullName == null) continue;
                    _abnoTypes.Add(type.FullName, type);                  
                }
            }
        }
        public static void FixAbnoAppearanceCrash(string typeName, Harmony harmony)
        {
            if (!_abnoTypes.TryGetValue(typeName, out var type))
            {
                return;
            }

            Plugin.Log.LogInfo($"Patching abno type {typeName} to prevent softlock");
            _abnoTypes.Remove(typeName);

            var stub = new HarmonyMethod(typeof(AppearancePatch), nameof(Stub));
            foreach (var methodInfo in type.GetMethods().Where(it => it.IsDeclaredMember() && !it.Name.StartsWith("get_") && !it.Name.StartsWith("set_")))
            {
                try
                {
                    Plugin.Log.LogInfo($"Patching {type.FullName}#{methodInfo.Name}");
                    harmony.Patch(methodInfo, prefix: stub);
                }
                catch (Exception e)
                {
                    Plugin.Log.LogInfo($"Failed to patch {type.FullName}#{methodInfo.Name} {e}");
                }
            }
        }

        [HarmonyPatch(typeof(DamageStatistics), nameof(DamageStatistics.SetResult))]
        [HarmonyPrefix]
        private static void DamageStatisticsSetResult(DamageStatistics __instance)
        {
            // stub, to catch errors
        }

        private static void Stub() { }
    }
}
