using System.IO;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Lethe.Patches;

public class Pref
{

    private static string _configFolder;
    
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Pref>();
        harmony.PatchAll(typeof(Pref));
        _configFolder = Path.Combine(BepInEx.Paths.ConfigPath, "LimbusCompany");
        Directory.CreateDirectory(_configFolder);
    }
    
    [HarmonyPatch(typeof(PlayerPrefs), nameof(PlayerPrefs.SetString))]
    [HarmonyPrefix]
    private static bool SetString(string key, string value)
    {
        var file = Path.Combine(_configFolder, key + ".perf");
        File.WriteAllText(file, value);
        return false;
    }

    [HarmonyPatch(typeof(PlayerPrefs), nameof(PlayerPrefs.GetString), typeof(string))]
    [HarmonyPrefix]
    private static bool GetString(string key, ref string __result)
    {
        var file = Path.Combine(_configFolder, key + ".perf");
        __result = File.ReadAllText(file);
        return false;
    }

    [HarmonyPatch(typeof(PlayerPrefs), nameof(PlayerPrefs.HasKey), typeof(string))]
    [HarmonyPrefix]
    private static bool GetString(string key, ref bool __result)
    {
        var file = Path.Combine(_configFolder, key + ".perf");
        __result = File.Exists(file);
        return false;
    }

}