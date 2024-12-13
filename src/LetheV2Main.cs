using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using System.IO;
using UnityEngine;

namespace LetheV2;

[BepInPlugin(GUID, NAME, VERSION)]
public class LetheV2Main : BasePlugin
{
    public const string GUID = AUTHOR + "." + NAME;
    public const string NAME = "lethe_experimental_v2";
    public const string VERSION = "1.0.0";
    public const string AUTHOR = "what";

    //bepinex stuff
    public static new ManualLogSource Log;
    public static ConfigFile Configuration;

    public static DirectoryInfo pluginPath = Directory.CreateDirectory(Path.Combine(Paths.PluginPath, $"{NAME}"));
    public static DirectoryInfo vanillaDumpPath = Directory.CreateDirectory(Path.Combine(pluginPath.FullName, "dumpedData"));
    public static DirectoryInfo templatePath = Directory.CreateDirectory(Path.Combine(pluginPath.FullName, "ModTemplate"));
    public static DirectoryInfo modsPath = Directory.CreateDirectory(Path.Combine(pluginPath.FullName, "mods"));

    public override void Load()
    {
        Log = base.Log;
        LoadConfiguration();
    }

    public static void LoadConfiguration()
    {
        //crc toggle config
        Configuration.Bind
        (
            "LaunchSettings",
            "ToggleCRC",
            false,
            "Toggle file checks from the game. Makes the game load faster. Disable when downloading updates."
        );

        Configuration.Bind
        (
            "LaunchSettings",
            "ServerURL",
            "https://api.lethelc.site/",
            "The URL of the private server (e.g. http://127.0.0.1:8080), leave blank to use defaults."
        );

        /*Configuration.Bind
        (
           "LaunchSettings",
           "usePrivateServer",
           true,
           "risky strats"
        );*/

        Configuration.Bind
        (
            "General",
            "ReloadData",
            KeyCode.Alpha8,
            "Key bind to reload data ingame."
        );
    }
}
