using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using System.Linq;
using Il2CppSystem.IO;
using SD;
using UnityEngine;

namespace CustomEncounter
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CustomEncounterMod : BasePlugin
    {
        public const string GUID = "carra.CustomEncounter";
        public const string NAME = "CustomEncounter";
        public const string VERSION = "0.0.5";
        public const string AUTHOR = "Carra";
        public static Action<string, Action> LogFatalError { get; set; }
        public static Action<string> LogError { get; set; }
        public static Action<string> LogWarning { get; set; }
       //  public static UIBase UiBase { get; private set; }
      
        public static string EncounterConfig = Path.Combine(Paths.ConfigPath, "encounter.json");

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
                foreach (var declaredMethod in AccessTools.GetDeclaredMethods(typeof(SDCharacterSkinUtil)))
                {
                    if (declaredMethod.Name == nameof(SDCharacterSkinUtil.CreateSkin))
                    {
                        Log.LogInfo($"Patching {declaredMethod.Name}");
                        var isDirect = declaredMethod.GetParameters().ToArray().Any(x => x.Name == "appearanceID");
                        harmony.Patch(declaredMethod,
                            prefix: new HarmonyMethod(AccessTools.Method(typeof(CustomEncounterHook), isDirect ? nameof(CustomEncounterHook.CreateSkin) : nameof(CustomEncounterHook.CreateSkinForModel))));
                    }
                }
                
                EncounterHelper.Log = Log;
                if (!File.Exists(EncounterConfig))
                {
                    File.Create(EncounterConfig).Close();
                }
            }
            catch (Exception e)
            {
                LogFatalError("Unknown fatal error!!!\n", () => {});
                LogError(e.ToString());
            }
        }
    }
}
