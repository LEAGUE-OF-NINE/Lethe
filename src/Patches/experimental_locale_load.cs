using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using UnhollowerRuntimeLib;

namespace LimbusSandbox.Patches
{
    internal class experimental_locale_load : MonoBehaviour
    {
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<experimental_locale_load>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.ToString().Contains("Assembly-CSharp"))
                {
                    foreach (var _type in asm.GetTypes())
                    {
                        if (_type.IsSubclassOf(typeof(LocalizeTextData)))
                        {
                            var method = typeof(JsonDataList<>).MakeGenericType(new[] { _type }).GetMethod("InitRemote");
                            harmony.Patch(method, null, new HarmonyMethod(typeof(experimental_locale_load).GetMethod("hello")));
                            Plugin.Log.LogWarning($"Patched subclass {method}");
                        }
                    }
                }
            }
        }

        public static void hello(MethodBase __originalMethod)
        {
            Plugin.Log.LogInfo($"ksatriat titit hijau || original method: {__originalMethod.Name}");
        }
    }
}
