using System;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnhollowerRuntimeLib;

namespace CustomEncounter.Patches;

public class Skills : Il2CppSystem.Object
{
    
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Skills>();
        harmony.PatchAll(typeof(Skills));
    }

    [HarmonyPatch(typeof(BattleActionModel), nameof(BattleActionModel.CallTargetDefenseActionsByAttack))]
    [HarmonyPrefix]
    private static void CallTargetDefenseActionsByAttackPatch(
        BattleActionModel __instance,
        BattleRunLog runLog,
        ref DEFENSE_TYPE type,
        ref bool canDuel,
        OneCoinLog_Attack oneCoinLog,
        BattleUnitModel specificTarget)
    {
        CustomEncounterHook.LOG.LogInfo($"Defend attack {oneCoinLog?._oneSkillPowerData.skillId}!! Defense type {type}! Can duel {canDuel}, target {specificTarget == null} {specificTarget?._instanceID}");
        //       actionLog = (BattleLog_Defense) oneCoinLog.CreateEvadeLog(defenseAction, runLog.TakeBattleLogIdForEvadeLog(), attackerInstanceID, runLog.SystemLogInstanceID);
        // if (oneCoinLog == null) return;
        // oneCoinLog.CreateEvadeLog(__instance, runLog.TakeBattleLogIdForEvadeLog(), __instance._model._instanceID, runLog.SystemLogInstanceID);
    }
    
    private static BattleRunLog _runLog;

    [HarmonyPatch(typeof(BattleActionModelManager), nameof(BattleActionModelManager.DefenseRun))]
    [HarmonyPostfix]
    private static void PatchDefenseRun(
        BattleRunLog runLog,
        BattleActionModel defenseAction,
        int attackerInstanceID,
        BattleActionModel attackerAction,
        OneCoinLog_Attack oneCoinLog,
        bool isTemperaryTargetSlot)
    {
        // var actionLog = runLog.CreateDefenseLog(defenseAction, attackerInstanceID);
        // CustomEncounterHook.LOG.LogInfo($"Inserted defense action {actionLog}");
        // actionLog.SetStartLog(defenseAction);
        // defenseAction.Behave(runLog, actionLog);
        // defenseAction.Done();
        // defenseAction.DoneWithAction();
    }

    [HarmonyPatch(typeof(BattleActionModelManager), nameof(BattleActionModelManager.Run), typeof(BattleActionModel))]
    [HarmonyPostfix]
    private static void RunActions(BattleActionModel action, ref List<BattleActionModel> __result)
    {
        CustomEncounterHook.LOG.LogInfo($"===============================");
        CustomEncounterHook.LOG.LogInfo($"Battle log {action._model._instanceID} {action._skill.GetID()}");
        var counters = new List<BattleActionModel>();
        foreach (var subaction in __result)
        {
            CustomEncounterHook.LOG.LogInfo($"Sub-action {subaction._model._instanceID} {subaction._skill.GetID()}");
            if (action.IsCounter()) { counters.Add(action); }
        }
        CustomEncounterHook.LOG.LogInfo($"===============================");
    }

   
    [HarmonyPatch(typeof(OneCoinLog_Attack), nameof(OneCoinLog_Attack.CreateEvadeLog))]
    [HarmonyPostfix]
    private static void SetBeforeLog(
        OneCoinLog_Attack __instance, 
        BattleActionModel defenseAction,
        int battleLogID,
        int attackerInstanceID,
        int runLogInstanceID,
        ref BattleLog_Evade __result)
    {
        CustomEncounterHook.LOG.LogInfo($"Skill {__instance._oneSkillPowerData.skillId} has evade {__result}");
    }


}