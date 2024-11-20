using BepInEx.Logging;
using HarmonyLib;
using UnhollowerRuntimeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx.Configuration;
using Voice;

namespace LimbusSandbox
{
    internal class SandboxLoader : MonoBehaviour
    {
        public static SandboxLoader Instance;
        public static ManualLogSource Log;

        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<SandboxLoader>();
            harmony.PatchAll(typeof(SandboxLoader));

            GameObject obj = new("LimbusSandbox.load");
            DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.HideAndDontSave;

            var hook = obj.AddComponent<SandboxLoader>();
            Log = Plugin.Log;
            Instance = hook;
        }

        internal void Update()
        {
            Plugin.Configuration.TryGetEntry<KeyCode>(new ConfigDefinition("General", "ReloadData"), out var reloadData);
            if (Input.GetKeyDown(reloadData.Value))
            {
                StaticDataManager.Instance._isDataLoaded = false;
                GlobalGameManager.Instance.LoadUserDataAndSetScene(SCENE_STATE.Main);
            }
        }

    }
}
