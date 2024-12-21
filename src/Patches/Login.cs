using System;
using BepInEx;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.IO;
using Login;
using Server;
using ServerConfig;
using SimpleJSON;
using Steamworks;
using UnhollowerRuntimeLib;
using UnityEngine;
using Utils;

namespace Lethe.Patches;

public class Login : Il2CppSystem.Object
{

    public static readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> StaticData = new();
    
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Login>();
        harmony.PatchAll(typeof(Login));
    }

    [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.OnClickGameStartButton))]
    [HarmonyPrefix]
    private static bool OnStartGame()
    {
        LetheHooks.LOG.LogInfo("Starting game");
        if (LetheHooks.IsSignedIn())
        {
            return true;
        }
        
        GlobalGameManager.Instance.OpenGlobalPopup("Lethe is still trying to sign in from your browser.");
        return false;
    }

    [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
    [HarmonyPostfix]
    private static void PostSetLoginInfo(LoginSceneManager __instance)
    {
        SingletonBehavior<LoginInfoManager>.Instance._currentAccountType = ACCOUNT_TYPE.STEAM;
        __instance.btn_switchAccount.gameObject.SetActive(false);
        __instance.tmp_version_null.gameObject.SetActive(false);
        __instance.tmp_version_null.text = "Lethe v" + LetheMain.VERSION;
        __instance.tmp_loginAccount.gameObject.SetActive(true);
        __instance.tmp_loginAccount.text = "Lethe v" + LetheMain.VERSION;
        float num = __instance.tmp_loginAccount.preferredWidth + 10f;
        __instance.tmp_loginAccount.rectTransform.sizeDelta = __instance.tmp_loginAccount.rectTransform.sizeDelta with
        {
            x = num
        };
    }

    [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.Start))]
    [HarmonyPrefix]
    private static void PostLoginSceneStart(LoginSceneManager __instance)
    {
        SingletonBehavior<LoginInfoManager>.Instance._currentAccountType = ACCOUNT_TYPE.STEAM;
        PostSetLoginInfo(__instance);
    }
   
    [HarmonyPatch(typeof(LoginInfoManager), nameof(LoginInfoManager.ProviderLogin_Steam))]
    [HarmonyPrefix]
    private static bool PreSignInLimbusSteam()
    {
        return false;
    }

    [HarmonyPatch(typeof(SteamClient), nameof(SteamClient.Init))]
    [HarmonyPrefix]
    private static bool PreSteamClientInit()
    {
        SteamClient.AppId = 1973530;
        SteamClient.initialized = true;
        LetheHooks.LOG.LogInfo("SteamClient init intercepted");
        return false;
    }
   
    [HarmonyPatch(typeof(ISteamUser), nameof(ISteamUser.GetSteamID))]
    [HarmonyPrefix]
    private static bool GetSteamID(ref SteamId __result)
    {
        var random = new System.Random();
        var randomULong = ((ulong)random.Next() << 32) | (uint)random.Next();
        __result = new SteamId
        {
            Value = randomULong
        };
        return false;
    }
   
    [HarmonyPatch(typeof(StaticDataManager), nameof(StaticDataManager.LoadStaticDataFromJsonFile))]
    [HarmonyPrefix]
    private static void PreLoadStaticDataFromJsonFile(StaticDataManager __instance, string dataClass,
        ref Il2CppSystem.Collections.Generic.List<JSONNode> nodeList)
    {
        LetheHooks.LOG.LogInfo($"Saving {dataClass}");
        StaticData[dataClass] = new System.Collections.Generic.List<string>();
        foreach (var jsonNode in nodeList)
        {
            StaticData[dataClass].Add(jsonNode.ToString(2));
        }

        var customDataList = new JSONArray();


        var templatePath = Directory.CreateDirectory(Path.Combine(LetheMain.templatePath.FullPath, "custom_limbus_data", dataClass));
        foreach (var modPath in Directory.GetDirectories(LetheMain.modsPath.FullPath))
        {
            var expectedPath = Path.Combine(modPath, "custom_limbus_data", dataClass);
            if (!Directory.Exists(expectedPath)) continue;

            foreach(var customData in Directory.GetFiles(expectedPath, "*.json", SearchOption.AllDirectories))
            {
                LetheHooks.LOG.LogInfo($"loading file from {customData.Substring(LetheMain.modsPath.FullPath.Length)}");
                try
                {
                    var node = JSONNode.Parse(File.ReadAllText(customData));
                    customDataList.Add(node);
                    nodeList.Insert(0, node);
                }
                catch (Exception ex)
                {
                    LetheHooks.LOG.LogError($"ERROR PARSING FILE {customData}: {ex.GetType()} {ex.Message}");
                }
            }

        }


        try {
            var url = Singleton<ServerSelector>.Instance.GetServerURL() + "/custom/upload/" + dataClass;
            var auth = SingletonBehavior<LoginInfoManager>.Instance.UserAuth.ToServerUserAuthFormat();
            var body = new JSONObject();
            var subNode = new JSONObject();
            subNode.Add("list", customDataList);
            body.Add("parameters", subNode);
            body.Add("userAuth", JSONNode.Parse(JsonUtility.ToJson(auth)));
            var schema = new HttpApiSchema(url, body.ToString(2), new Action<string>(_ => { }), "", false);
            Request.EnqueueWebRequest(HttpApiRequester.Instance, schema, true);
        }
        catch (Exception ex)
        {
            LetheHooks.LOG.LogError($"Error uploading {dataClass}: {ex.GetType()} {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(SkillStaticDataList), nameof(SkillStaticDataList.Init))]
    [HarmonyPrefix]
    private static bool override_load_skill_data(Il2CppSystem.Collections.Generic.List<SkillStaticData> nodeList, SkillStaticDataList __instance)
    {
        __instance.list = new List<SkillStaticData>();
        __instance.dict.Clear();

        var nodeArray = nodeList.ToArray();
        for (int i = 0; i < nodeList.Count; i++)
        {
            List<SkillStaticData> list = JsonUtility.FromJson<SkillStaticDataList>(nodeArray[i].ToString()).GetList();
            var array = list.ToArray();
            for (int j = 0; j < list.Count; j++)
            {
                bool success = __instance.dict.TryAdd(array[j].ID, array[j]);
                if (success)
                {
                    __instance.list.Add(array[j]);
                    LetheHooks.LOG.LogWarning($"Replacing skill {array[j].ID} with custom data");
                }
            }
        }

        return false;
    }

}