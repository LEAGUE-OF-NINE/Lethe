using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;
using HarmonyLib;

namespace LetheV2
{
    internal class load : MonoBehaviour
    {
       
        private static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<load>();

            //load
            GameObject loader = new GameObject($"{main.NAME} {main.VERSION}");
            DontDestroyOnLoad(loader);
            loader.hideFlags |= HideFlags.HideAndDontSave;
            loader.AddComponent<load>();
            harmony.PatchAll(typeof(load));
        }
    }
}
