using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using Lethe.SkillAbility;
using HarmonyLib;
using Il2CppDumper;
using Il2CppSystem.IO;
using Il2CppSystem.Security.Cryptography;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using Random = System.Random;

namespace Lethe;

[BepInPlugin(GUID, NAME, VERSION)]
public class LetheMain : BasePlugin
{
    public const string GUID = $"{AUTHOR}.{NAME}";
    public const string NAME = "Lethe";
    public const string VERSION = "1.0.0";
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

        ConfigServer = Config.Bind("General", "ServerRoot", "https://api.lethelc.site/",
            "The URL of the private server (e.g. http://127.0.0.1:8080), leave blank to use defaults.");
        if (string.IsNullOrEmpty(ConfigServer.Value))
            LogWarning("ServerRoot is empty, using the default server!!!");
        else
            Log.LogInfo("Using private server: " + ConfigServer.Value);

        var rng = new Random();
        var port = rng.Next(30000, 65500);
        Application.OpenURL(ConfigServer.Value + $"/auth/login?port={port}");

        try
        {
            //temp fix
            var toggleCRC = Config.Bind("LaunchSettings",
                                        "CRCtoggle",
                                        false,
                                        "Toggle CRC checks from the game. It's recommended that it's disabled when downloading updates.");


            LetheHooks.Setup(Log, port);
            Harmony harmony = new(NAME);

            // Register abilities
            ClassInjector.RegisterTypeInIl2Cpp<SkillAbilityEvadeThenUseSkill>();

            // Setup harmony hooks
            NewEvadeThenUseSkill.Setup(harmony);
            ChangeCounterOnConditional.Setup(harmony);
            ChangeSkillOnConditional.Setup(harmony);
            CustomFurioso.Setup(harmony);
            Patches.CustomAssistant.Setup(harmony);
            Patches.Data.Setup(harmony);
            Patches.Fixes.Setup(harmony);
            Patches.Login.Setup(harmony);
            Patches.Server.Setup(harmony);
            Patches.Skin.Setup(harmony);
            Patches.Texture.Setup(harmony);
            Passives.RollAllHeads.Setup(harmony);
            ChangeAppearance.Setup(harmony);
            ChangeSkillMotion.Setup(harmony);
            if (toggleCRC.Value) Patches.TextAsset.Setup(harmony);
            Patches.Skills.Setup(harmony);

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