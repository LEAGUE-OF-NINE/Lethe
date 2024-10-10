using System.Collections.Generic;
using HarmonyLib;
using Il2CppSystem.Text;
using Server;
using ServerConfig;
using Steamworks;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CustomEncounter.Patches;

public class Server : Il2CppSystem.Object
{
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Server>();
        harmony.PatchAll(typeof(Server));
    }

    [HarmonyPatch(typeof(ServerSelector), nameof(ServerSelector.GetServerURL))]
    [HarmonyPostfix]
    private static void ServerSelector_GetServerURL(ServerSelector __instance, ref string __result)
    {
        var serverURL = CustomEncounterMod.ConfigServer.Value;
        if (!string.IsNullOrEmpty(serverURL)) __result = serverURL;
    }

    [HarmonyPatch(typeof(SteamUser), nameof(SteamUser.GetAuthSessionTicket))]
    [HarmonyPrefix]
    private static bool SteamUser_GetAuthSessionTicket(ref AuthTicket __result)
    {
        __result = new AuthTicket
        {
            Data = Encoding.ASCII.GetBytes(CustomEncounterHook.AccountJwt())
        };

        return false;
    }

    [HarmonyPatch(typeof(HttpApiRequester), nameof(HttpApiRequester.OnResponseWithErrorCode))]
    [HarmonyPrefix]
    private static void OnResponseWithErrorCode(HttpApiRequester __instance)
    {
        __instance._errorCount = 0;
    }

    [HarmonyPatch(typeof(UserDataManager), nameof(UserDataManager.UpdateData))]
    [HarmonyPostfix]
    private static void UpdateData(UserDataManager __instance, UpdatedFormat updated)
    {
        var unlockedPersonalities = __instance._personalities._personalityList._list;
        var unlockedPersonalityIds = new HashSet<int>();
        foreach (var unlockedPersonality in unlockedPersonalities)
        {
            unlockedPersonalityIds.Add(unlockedPersonality.ID);
        }

        foreach (var personalityStaticData in Singleton<StaticDataManager>.Instance.PersonalityStaticDataList.list)
        {
            if (unlockedPersonalityIds.Contains(personalityStaticData.ID)) continue;
            var personality = new Personality(personalityStaticData.ID)
            {
                _gacksung = 4,
                _level = 50,
                _acquireTime = new DateUtil()
            };
            unlockedPersonalities.Add(personality);
        }
    }
}