using HarmonyLib;
using UnhollowerRuntimeLib;

namespace CustomEncounter.Patches;

public class Skills : Il2CppSystem.Object
{
    
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Skills>();
        harmony.PatchAll(typeof(Skills));
    }

    [HarmonyPatch(typeof(BattleLog_Defense), nameof(BattleLog_Defense.CreateOneCoinLog))]
    [HarmonyPrefix]
    private static bool CreateOneCoinLogPatch(BattleLog_Defense __instance, BattleActionModel actorAction, CoinModel coin, int skillLevel, ref OneCoinLog __result)
    {
        if (__instance is not BattleLog_Evade) return true;
        CustomEncounterHook.LOG.LogInfo("ZAZA MAN PLEASE I AM BEGGING YOU");
        OneCoinLog_Attack oneCoinLog = new OneCoinLog_Attack(actorAction, coin, skillLevel);
        __instance._actorOneCoinLogList?.Add(oneCoinLog);
        __result = oneCoinLog;
        return false;
    }


    [HarmonyPatch(typeof(BattleLog_Evade), nameof(BattleLog_Evade.SetAfterLog))]
    [HarmonyPrefix]
    private static bool SetAfterLogPatch(BattleLog_Evade __instance, BattleActionModel actorAction)
    {
        CustomEncounterHook.LOG.LogInfo("ZAZA MAN I AM BEGGING YOU");
        __instance._afterStatus = new StatusLog_Attack(actorAction);
        return false;
    }


    [HarmonyPatch(typeof(BattleLog_Evade), nameof(BattleLog_Evade.SetStartLog))]
    [HarmonyPostfix]
    private static void SetStartLog(BattleLog_Evade __instance, BattleActionModel actorAction)
    {
        CustomEncounterHook.LOG.LogInfo("EVADE START");
        __instance._startStatus = new StatusLog_Attack(actorAction);
        var targetUnitModelList = actorAction.GetTargetUnitModelList();
        __instance._subTargetInfoList_Start = new List<SubBattleLog_CharacterInfo>();
        int index1 = 0;
        for (int count = targetUnitModelList.Count; index1 < count; ++index1)
        {
            BattleUnitModel actor = targetUnitModelList[index1];
            if (!actor.InstanceID.Equals(this._mainTargetInfo_Start.GetInstanceIDPartFirst()))
                this._subTargetInfoList_Start.Add(new SubBattleLog_CharacterInfo(actor));
        }

    }

    [HarmonyPatch(typeof(BattleLog_Evade), nameof(BattleLog_Evade.SetBeforeLog))]
    [HarmonyPostfix]
    private static void SetBeforeLog(BattleLog_Evade __instance, BattleActionModel actorAction)
    {
        CustomEncounterHook.LOG.LogInfo("BEFORE START");
        __instance._beforeStatus = new StatusLog_Attack(actorAction);
    }

}