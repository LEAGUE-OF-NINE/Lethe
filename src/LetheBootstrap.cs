using DG.Tweening.Plugins.Core;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.ProBuilder;
using BepInEx.IL2CPP.Utils;

namespace LetheV2
{
    internal class LetheBootstrap : MonoBehaviour
    {
        public static LetheBootstrap Instance;

        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<LetheBootstrap>();
            harmony.PatchAll(typeof(LetheBootstrap));

            GameObject obj = new("letheV2hello");
            DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.HideAndDontSave;

            var hook = obj.AddComponent<LetheBootstrap>();
            Instance = hook;
        }
    }
}
