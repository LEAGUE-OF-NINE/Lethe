using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MainUI;
using UnhollowerRuntimeLib;

namespace CustomEncounter.Patches;

public class Fixes : Il2CppSystem.Object
{

    private static Dictionary<string, Type> _abnoTypes = new();
    
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Fixes>();
        harmony.PatchAll(typeof(Fixes));

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in asm.GetTypes().Where(it => !(it.IsSealed && it.IsAbstract) && it.IsSubclassOf(typeof(AbnormalityAppearance))))
            {
                if (type.FullName == null) continue;
                _abnoTypes.Add(type.FullName, type);
                CustomEncounterHook.LOG.LogInfo($"Found abno type {type.FullName}");
            }
        }
    }

    public static void FixAbnoAppearanceCrash(string typeName, Harmony harmony)
    {
        if (!_abnoTypes.TryGetValue(typeName, out var type))
        {
            CustomEncounterHook.LOG.LogInfo($"Appearance {typeName} does not need patching");
            return;
        }
        
        CustomEncounterHook.LOG.LogInfo($"Patching abno type {typeName} to prevent softlock");
        _abnoTypes.Remove(typeName);
        
        var stub = new HarmonyMethod(typeof(Fixes), nameof(Stub));
        foreach (var methodInfo in type.GetMethods().Where(it => it.IsDeclaredMember() && !it.Name.StartsWith("get_") && !it.Name.StartsWith("set_")))
        {
            try
            {
                CustomEncounterHook.LOG.LogInfo($"Patching {type.FullName}#{methodInfo.Name}");
                harmony.Patch(methodInfo, prefix: stub);
            }
            catch (Exception e)
            {
                CustomEncounterHook.LOG.LogInfo($"Failed to patch {type.FullName}#{methodInfo.Name} {e}");
            }
        }
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
    
    private static void Stub() {}
    
}