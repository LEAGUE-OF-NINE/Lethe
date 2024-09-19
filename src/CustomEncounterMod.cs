using BepInEx;
using HarmonyLib;
using System;
using System.Linq;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using Il2CppSystem.IO;
using Il2CppSystem.Security.Cryptography;
using SD;
using ServerConfig;
using UnhollowerBaseLib;
using UnityEngine;
using Convert = Il2CppSystem.Convert;

namespace CustomEncounter
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CustomEncounterMod : BasePlugin
    {
        public const string GUID = "carra.CustomEncounter";
        public const string NAME = "CustomEncounter";
        public const string VERSION = "0.0.6";
        public const string AUTHOR = "Carra";
        public static Action<string, Action> LogFatalError { get; set; }
        public static Action<string> LogError { get; set; }
        public static Action<string> LogWarning { get; set; }
      
        public static string EncounterConfig = Path.Combine(Paths.ConfigPath, "encounter.json");

        public static ConfigEntry<string> ConfigServer;
        public static byte[] Identity = new byte[0];

        public override void Load()
        {
            LogError = (string log) =>
            {
                Log.LogError(log);
                Debug.LogError(log);
            };
            LogWarning = (string log) =>
            {
                Log.LogWarning(log);
                Debug.LogWarning(log);
            };
            LogFatalError = (string log, Action action) =>
            {
                LogError(log);
                Debug.LogError(log);
            };

            ConfigServer = Config.Bind("General", "ServerURL", "https://limbuspriv.windtfw.com", "The URL of the private server (e.g. http://127.0.0.1:8080), leave blank to use defaults.");
            if (string.IsNullOrEmpty(ConfigServer.Value))
            {
                LogWarning("ServerURL is empty, using the default server!!!");
            }
            else
            {
                Log.LogInfo("Using private server: " + ConfigServer.Value);
            }
            
            // get APPDATA DirectoryInfo
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var keyFile = Path.Combine(appdata, "LimbusPrivateServer.key");
            if (!File.Exists(keyFile))
            {
                // Fill file with random 32 bytes cryptographic
                var csp = new RNGCryptoServiceProvider();
                var fileStream = File.Create(keyFile);
                var buf = new byte[32];
                csp.GetBytes(buf);
                fileStream.Write(new Il2CppStructArray<byte>(buf));
                fileStream.Close();
            }

            Identity = File.ReadAllBytes(keyFile);
            
            try
            {
                // Setup harmony hooks
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
                            prefix: new HarmonyMethod(AccessTools.Method(typeof(CustomEncounterHook),
                                isDirect
                                    ? nameof(CustomEncounterHook.CreateSkin)
                                    : nameof(CustomEncounterHook.CreateSkinForModel))));
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
                LogFatalError("Unknown fatal error!!!\n", () => { });
                LogError(e.ToString());
            }
        }
    }
}
