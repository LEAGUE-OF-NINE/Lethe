using HarmonyLib;
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
    private static void StartBehaviourAction()
    {
        // stub, to catch errors
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

}