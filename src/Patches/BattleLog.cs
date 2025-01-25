using System;
using System.Collections.Generic;
using HarmonyLib;
using Server;
using UnhollowerRuntimeLib;
using UnityEngine.Networking;

namespace Lethe.Patches;

public class BattleLog : Il2CppSystem.Object
{
    private static readonly HashSet<string> BlockedDomains = new HashSet<string>
    {
        "limbuscompanyapi.com",
        "subbattlelog.limbuscompanyapi.com",
        "battlelog.limbuscompanyapi.com",
    };

    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<BattleLog>();
        harmony.PatchAll(typeof(BattleLog));
    }

    [HarmonyPatch(typeof(HttpBattleLogRequester), nameof(HttpBattleLogRequester.EnqueueRequest))]
    [HarmonyPrefix]
    public static bool PreBattleLogEnqueueRequest()
    {
        LetheHooks.LOG.LogInfo($"WARNING: LIMBUS TRIED TO REPORT TO PROJECT MOON");
        return false;
    }

    [HarmonyPatch(typeof(HttpBattleLogRequester), nameof(HttpBattleLogRequester.SendRequest))]
    [HarmonyPrefix]
    public static bool PreBattleLogSendRequest()
    {
        LetheHooks.LOG.LogInfo($"WARNING: LIMBUS TRIED TO REPORT TO PROJECT MOON");
        return false;
    }

    [HarmonyPatch(typeof(UnityWebRequest), nameof(UnityWebRequest.Post), typeof(string), typeof(string))]
    [HarmonyPrefix]
    public static bool PrePostRequest(ref string uri, string postData)
    {
        var serverURL = LetheMain.ConfigServer.Value;
        if (!string.IsNullOrEmpty(serverURL))
        {
            try
            {
                var originalUri = new Uri(uri);
                var newServerUri = new Uri(serverURL);
                var newUri = new UriBuilder(originalUri)
                {
                    Host = newServerUri.Host,
                    Scheme = newServerUri.Scheme,
                    Port = newServerUri.IsDefaultPort ? -1 : newServerUri.Port
                };

                uri = newUri.ToString();
            }
            catch (UriFormatException ex)
            {
                LetheHooks.LOG.LogError($"Error parsing URIs: {ex.Message}");
                return false;
            }
        }
        else
        {
            LetheHooks.LOG.LogError("No server URL configured. Blocking request.");
            return false;
        }

        foreach (var blocked in BlockedDomains)
        {
            if (uri.Contains(blocked))
            {
                LetheHooks.LOG.LogInfo($"Blocked POST request to: {uri}");
                return false;
            }
        }

        return true;
    }
}