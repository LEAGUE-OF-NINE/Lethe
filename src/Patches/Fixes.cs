using HarmonyLib;
using MainUI;
using UnhollowerRuntimeLib;

namespace CustomEncounter.Patches;

public class Fixes : Il2CppSystem.Object
{
    
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Fixes>();
        harmony.PatchAll(typeof(Fixes));
    }
    
    [HarmonyPatch(typeof(AbnormalityAppearance_Cromer1p), nameof(AbnormalityAppearance_Cromer1p.OnEndBehaviour))]
    [HarmonyPrefix]
    private static void KromerOnEndBehavior(AbnormalityAppearance_Cromer1p __instance)
    {
        // stub, to catch errors
    }

    [HarmonyPatch(typeof(DamageStatistics), nameof(DamageStatistics.SetResult))]
    [HarmonyPrefix]
    private static void DamageStatisticsSetResult(DamageStatistics __instance)
    {
        // stub, to catch errors
    }

    [HarmonyPatch(typeof(BuffAbility_EgoAwakenDongrangTree),
        nameof(BuffAbility_EgoAwakenDongrangTree.OnRoundStart_After_Event))]
    [HarmonyPrefix]
    private static void BuffAbility_EgoAwakenDongrangTreeOnRoundStart_After_Event(
        BuffAbility_EgoAwakenDongrangTree __instance)
    {
        // stub, to catch errors
    }
   
    [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.StartBehaviourAction))]
    [HarmonyPrefix]
    private static void StartBehaviourAction(BattleUnitView __instance, BattleActionLog actionLog)
    {
        CustomEncounterHook.LOG.LogInfo("behaviourActionViewer == null: " + (__instance._behaviourActionViewer == null));
        CustomEncounterHook.LOG.LogInfo("curAppearance == null: " + (__instance._curAppearance == null));
        CustomEncounterHook.LOG.LogInfo("appearance == null: " + (__instance.Appearance == null));
        CustomEncounterHook.LOG.LogInfo("actionLog.GetSystemLog() == null: " + (actionLog.GetSystemLog() == null));
    }
   
    [HarmonyPatch(typeof(StageController), nameof(StageController.InitStage))]
    [HarmonyPrefix]
    private static void PreInitStage(StageController __instance, bool isCleared, bool isSandbox)
    {
        CustomEncounterHook.LOG.LogInfo("Pre-InitStage " + isCleared + " " + isSandbox);
    }

    [HarmonyPatch(typeof(StageController), nameof(StageController.InitStage))]
    [HarmonyPostfix]
    private static void PostInitStage(StageController __instance, bool isCleared, bool isSandbox)
    {
        CustomEncounterHook.LOG.LogInfo("Post-InitStage " + isCleared + " " + isSandbox);
    }
   
    [HarmonyPatch(typeof(SeasonInfoStaticDataList), nameof(SeasonInfoStaticDataList.GetNowSeasonInfo))]
    [HarmonyPostfix]
    private static void PostGetNowSeasonInfo(SeasonInfoStaticDataList __instance, ref SeasonInfoStaticData __result)
    {
        if (__result != null) return;
        __result = __instance.list.ToArray()[0];
    }

    [HarmonyPatch(typeof(AbnormalityAppearance_8166), nameof(AbnormalityAppearance_8166.OnRoundStart_AfterChoice))]
    [HarmonyPrefix]
    private static void lol() { }
}