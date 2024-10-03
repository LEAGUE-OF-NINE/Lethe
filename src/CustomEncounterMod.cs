using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using CustomEncounter.SkillAbility;
using HarmonyLib;
using Il2CppSystem.IO;
using Il2CppSystem.Security.Cryptography;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CustomEncounter;

[BepInPlugin(GUID, NAME, VERSION)]
public class CustomEncounterMod : BasePlugin
{
    public const string GUID = "carra.CustomEncounter";
    public const string NAME = "CustomEncounter";
    public const string VERSION = "0.0.6";
    public const string AUTHOR = "Carra";

    public static string EncounterConfig = Path.Combine(Paths.ConfigPath, "encounter.json");

    public static ConfigEntry<string> ConfigServer;
    public static Action<string, Action> LogFatalError { get; set; }
    public static Action<string> LogError { get; set; }
    public static Action<string> LogWarning { get; set; }

    public override void Load()
    {
        LogError = log =>
        {
            Log.LogError(log);
            Debug.LogError(log);
        };
        LogWarning = log =>
        {
            Log.LogWarning(log);
            Debug.LogWarning(log);
        };
        LogFatalError = (log, action) =>
        {
            LogError(log);
            Debug.LogError(log);
        };

        ConfigServer = Config.Bind("General", "ServerURL", "https://limbuspriv.windtfw.com",
            "The URL of the private server (e.g. http://127.0.0.1:8080), leave blank to use defaults.");
        if (string.IsNullOrEmpty(ConfigServer.Value))
            LogWarning("ServerURL is empty, using the default server!!!");
        else
            Log.LogInfo("Using private server: " + ConfigServer.Value);

        Application.OpenURL(ConfigServer.Value + "/auth/login");

        try
        {

            CustomEncounterHook.Setup(Log);
            Harmony harmony = new(NAME);
            
            // Register abilities
            ClassInjector.RegisterTypeInIl2Cpp<SkillAbilityEvadeThenUseSkill>();

            // Setup harmony hooks
            Patches.CustomAssistant.Setup(harmony);
            Patches.Data.Setup(harmony);
            Patches.Fixes.Setup(harmony);
            Patches.Login.Setup(harmony);
            Patches.Server.Setup(harmony);
            Patches.Skin.Setup(harmony);
            Patches.Texture.Setup(harmony);
            // Patches.Skills.Setup(harmony);

            EncounterHelper.Log = Log;
            if (!File.Exists(EncounterConfig)) File.Create(EncounterConfig).Close();
        }
        catch (Exception e)
        {
            LogFatalError("Unknown fatal error!!!\n", () => { });
            LogError(e.ToString());
        }
    }
}