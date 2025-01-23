using HarmonyLib;
using Server;
using UnhollowerRuntimeLib;

namespace Lethe.Patches;

public class BattleLog : Il2CppSystem.Object
{

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

}