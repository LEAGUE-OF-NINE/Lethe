using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using UnityEngine;

namespace CustomEncounter
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CustomEncounterMod : BasePlugin
    {
        public const string GUID = "carra.CustomEncounter";
        public const string NAME = "CustomEncounter";
        public const string VERSION = "0.0.1";
        public const string AUTHOR = "Carra";
        public static Action<string, Action> LogFatalError { get; set; }
        public static Action<string> LogError { get; set; }
        public static Action<string> LogWarning { get; set; }
        public override void Load()
        {
            LogError = (string log) => { Log.LogError(log); Debug.LogError(log); };
            LogWarning = (string log) => { Log.LogWarning(log); Debug.LogWarning(log); };
            LogFatalError = (string log, Action action) => { LogError(log); Debug.LogError(log); };
            try
            {
                Harmony harmony = new(NAME);
                harmony.PatchAll(typeof(CustomEncounterHook));
            }
            catch (Exception e)
            {
                LogFatalError("Mod eas unknown fatal error!!!\n", () => {});
                LogError(e.ToString());
            }
        }
    }
}
