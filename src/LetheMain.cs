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
using System.Threading;

namespace Lethe;

[BepInPlugin(GUID, NAME, VERSION)]
public class LetheMain : BasePlugin
{
    public const string GUID = $"{AUTHOR}.{NAME}";
    public const string NAME = "Lethe";
    public const string VERSION = "1.0.8";
    public const string AUTHOR = "Carra";

    public static DirectoryInfo pluginPath = Directory.CreateDirectory(Path.Combine(Paths.PluginPath, $"{NAME}"));
    public static DirectoryInfo vanillaDumpPath = Directory.CreateDirectory(Path.Combine(pluginPath.FullPath, "dumpedData"));
    public static DirectoryInfo templatePath = Directory.CreateDirectory(Path.Combine(pluginPath.FullPath, "ModTemplate"));
	public static DirectoryInfo modsPath = Directory.CreateDirectory(Path.Combine(pluginPath.FullPath, "mods"));

	public static string EncounterConfig = Path.Combine(pluginPath.FullPath, "encounter.json");

    public static ConfigEntry<string> ConfigServer;
    public static ConfigEntry<KeyCode> reloadDataKey;
    public static ConfigEntry<KeyCode> dumpDataKey;
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

        try
        {
            //temp fix
            var toggleCRC = Config.Bind("LaunchSettings",
                                        "CRCtoggle",
                                        false,
                                        "Toggle CRC checks from the game. It's recommended that it's disabled when downloading updates.");

            reloadDataKey = Config.Bind
            (
                "General",
                "ReloadData",
                KeyCode.Alpha8,
                "Key bind to reload data ingame."
            );

            dumpDataKey = Config.Bind
            (
                "General",
                "DumpData",
                KeyCode.Alpha0,
                "not even dumping everything lol!!! dump locale, identities, and encounters"
            );


            LetheHooks.Setup(Log, port);
            Harmony harmony = new(NAME);

            // Setup harmony hooks
            SafeSetup(Patches.Request.Setup, harmony, "Request");
            SafeSetup(NewEvadeThenUseSkill.Setup, harmony);
            SafeSetup(ChangeCounterOnConditional.Setup, harmony);
            SafeSetup(ChangeSkillOnConditional.Setup, harmony);
            SafeSetup(CustomFurioso.Setup, harmony);
            SafeSetup(Patches.CustomAssistant.Setup, harmony, "CustomAssistant");
            SafeSetup(Patches.Data.Setup, harmony, "Data");
            SafeSetup(Patches.Fixes.Setup, harmony, "Fixes");
            SafeSetup(Patches.Login.Setup, harmony, "Login");
            SafeSetup(Patches.Server.Setup, harmony, "Server");
            SafeSetup(Patches.Skin.Setup, harmony, "Skin");
            SafeSetup(Patches.Texture.Setup, harmony, "Texture");
            SafeSetup(ChangeAppearance.Setup, harmony);
            SafeSetup(Passives.ChangeAppearance_Passive.Setup, harmony);
            SafeSetup(ChangeSkillMotion.Setup, harmony);
            if (toggleCRC.Value) SafeSetup(Patches.TextAsset.Setup, harmony, "TextAsset");
            SafeSetup(Patches.Skills.Setup, harmony, "Skills");

            //add some folder for the mod template
            Directory.CreateDirectory(Path.Combine(templatePath.FullPath, "custom_appearance"));
            Directory.CreateDirectory(Path.Combine(templatePath.FullPath, "custom_sprites"));
            Directory.CreateDirectory(Path.Combine(templatePath.FullPath, "custom_limbus_locale"));
            Directory.CreateDirectory(Path.Combine(templatePath.FullPath, "custom_assistant"));
            Directory.CreateDirectory(Path.Combine(templatePath.FullPath, "custom_appearance"));

            // visual mod
            VisualMods.Carra.Patch();
            var soundThread = new Thread(VisualMods.Sound.SoundReplace);
            soundThread.Start();
            EncounterHelper.Log = Log;
            if (!File.Exists(EncounterConfig)) File.Create(EncounterConfig).Close();
        }
        catch (Exception e)
        {
            LogFatalError("Unknown fatal error!!!\n", () => { });
            LogError(e.ToString());
        }
    }
    
    private void SafeSetup(Action<Harmony> setupAction, Harmony harmony, string patchName = null)
    {
        try
        {
            setupAction(harmony);
        }
        catch (Exception ex)
        {
            string name = patchName ?? setupAction.Method.DeclaringType?.Name ?? "Unknown";
            LogError($"Failed to setup patch {name}: {ex.Message}");
            LogError(ex.StackTrace);
        }
    }

    
}
