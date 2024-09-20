using System;
using BepInEx;
using HarmonyLib;
using Il2CppSystem.IO;
using Server;
using ServerConfig;
using SimpleJSON;
using UnhollowerRuntimeLib;

namespace CustomEncounter.Patches;

public class Login : Il2CppSystem.Object
{
    
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Login>();
        harmony.PatchAll(typeof(Login));
    }

    [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
    [HarmonyPostfix]
    private static void SetLoginInfo(LoginSceneManager __instance)
    {
        __instance.tmp_loginAccount.text = "CustomEncounter v" + CustomEncounterMod.VERSION;
    }
   
    [HarmonyPatch(typeof(StaticDataManager), nameof(StaticDataManager.LoadStaticDataFromJsonFile))]
    [HarmonyPrefix]
    private static void LoadStaticDataFromJsonFile(StaticDataManager __instance, string dataClass,
        ref Il2CppSystem.Collections.Generic.List<JSONNode> nodeList)
    {
        CustomEncounterHook.LOG.LogInfo($"Dumping {dataClass}");
        var root = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "limbus_data", dataClass));
        var i = 0;
        foreach (var jsonNode in nodeList)
        {
            File.WriteAllText(Path.Combine(root.FullName, $"{i}.json"), jsonNode.ToString(2));
            i++;
        }


        var customDataList = new JSONArray();

        root = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_limbus_data", dataClass));
        foreach (var file in Directory.GetFiles(root.FullName, "*.json"))
            try
            {
                CustomEncounterHook.LOG.LogInfo($"Loading {file}");
                var node = JSONNode.Parse(File.ReadAllText(file));
                customDataList.Add(node);
                nodeList.Add(node);
            }
            catch (Exception ex)
            {
                CustomEncounterHook.LOG.LogError($"Error parsing {file}: {ex.GetType()} {ex.Message}");
            }

        try
        {
            var url = Singleton<ServerSelector>.Instance.GetServerURL() + "/Custom/Upload/" + dataClass;
            var body = customDataList.ToString(2);
            var schema = new HttpApiSchema(url, body, new Action<string>(_ => { }), "", false);
            HttpApiRequester.Instance.SendRequest(schema, true);
        }
        catch (Exception ex)
        {
            CustomEncounterHook.LOG.LogError($"Error uploading {dataClass}: {ex.GetType()} {ex.Message}");
        }
    }

}