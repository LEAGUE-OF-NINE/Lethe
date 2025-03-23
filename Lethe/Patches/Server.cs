using System.Collections.Generic;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Text;
using ServerConfig;
using Steamworks;

namespace Lethe.Patches;

public class Server : Il2CppSystem.Object
{
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Server>();
        harmony.PatchAll(typeof(Server));
    }

    [HarmonyPatch(typeof(SteamUser), nameof(SteamUser.GetAuthSessionTicket))]
    [HarmonyPrefix]
    private static bool SteamUser_GetAuthSessionTicket(ref AuthTicket __result)
    {
        __result = new AuthTicket
        {
            Data = Encoding.ASCII.GetBytes(LetheHooks.AccountJwt())
        };

        return false;
    }

    [HarmonyPatch(typeof(UserDataManager), nameof(UserDataManager.UpdateData))]
    [HarmonyPostfix]
    private static void UpdateData(UserDataManager __instance)
    {
        var unlockedPersonalities = __instance._personalities._personalityList._list;
        var unlockedPersonalityIds = new HashSet<int>();
        foreach (var unlockedPersonality in unlockedPersonalities)
        {
            unlockedPersonalityIds.Add(unlockedPersonality.ID);
        }

        var unlockedEgos = __instance._egos._egoList._list;
        var unlockedEgosIds = new HashSet<int>();
        foreach (var ego in unlockedEgos)
        {
            unlockedEgosIds.Add(ego.ID);
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

        foreach (var egoStaticData in Singleton<StaticDataManager>.Instance.EgoList.list)
        {
            if (unlockedEgosIds.Contains(egoStaticData.ID)) continue;
            var ego = new Ego(egoStaticData.ID, 4, EGO_OWNED_TYPES.USER)
            {
                _gacksung = 4,
            };
            unlockedEgos.Add(ego);
        }
    }
}