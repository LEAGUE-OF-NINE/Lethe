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
       //  public static UIBase UiBase { get; private set; }
        public override void Load()
        {
            LogError = (string log) => { Log.LogError(log); Debug.LogError(log); };
            LogWarning = (string log) => { Log.LogWarning(log); Debug.LogWarning(log); };
            LogFatalError = (string log, Action action) => { LogError(log); Debug.LogError(log); };
            try
            {
                CustomEncounterHook.Setup(Log);
                Harmony harmony = new(NAME);
                harmony.PatchAll(typeof(CustomEncounterHook));
                // Universe.Init(5f, OnInitialized, UniverseLog, new UniverseLibConfig()
                // {
                //     Disable_EventSystem_Override = true,
                //     Force_Unlock_Mouse = true,
                //     Unhollowed_Modules_Folder = Path.Combine(Paths.BepInExRootPath, "interop")
                // });
            }
            catch (Exception e)
            {
                LogFatalError("Unknown fatal error!!!\n", () => {});
                LogError(e.ToString());
            }
        }
        
        private void UniverseLog(string arg1, LogType arg2)
        {
            switch (arg2)
            {
                case LogType.Error:
                    LogError(arg1);
                    break;
                case LogType.Assert:
                    LogError(arg1);
                    break;
                case LogType.Warning:
                    LogWarning(arg1);
                    break;
                case LogType.Log:
                    Log.LogInfo(arg1);
                    break;
                case LogType.Exception:
                    LogError(arg1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(arg2), arg2, null);
            }
        }
        
        // private void OnInitialized()
        // {
        //     UiBase = UniversalUI.RegisterUI(GUID, () => {});
        //     UiBase.SetOnTop();
        //     EncounterPanel.IsShown = false;
        // }
    }
}
