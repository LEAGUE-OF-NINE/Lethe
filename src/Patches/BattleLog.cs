using HarmonyLib;
using Server;
using UnhollowerRuntimeLib;

namespace Lethe.Patches;

public class BattleLog
{
   
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<BattleLog>();
        harmony.PatchAll(typeof(BattleLog));
    }
   
    [HarmonyPatch(typeof(HttpBattleLogRequester), nameof(HttpBattleLogRequester.EnqueueRequest))]
    [HarmonyPrefix]
    public static bool PreBattleLogEnqueueRequest(HttpLogSchema schema)
    {
        LetheHooks.LOG.LogInfo($"WARNING: LIMBUS TRIED TO REPORT TO PROJECT MOON: {schema.URL}");
        return false;
    }
   
    [HarmonyPatch(typeof(HttpBattleLogRequester), nameof(HttpBattleLogRequester.SendRequest))]
    [HarmonyPrefix]
    public static bool PreBattleLogSendRequest(HttpLogSchema schema)
    {
        LetheHooks.LOG.LogInfo($"WARNING: LIMBUS TRIED TO REPORT TO PROJECT MOON: {schema.URL}");
        return false;
    }


    
}