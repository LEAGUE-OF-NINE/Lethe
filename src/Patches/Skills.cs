using System;
using CustomEncounter.SkillAbility;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CustomEncounter.Patches;

public class Skills : Il2CppSystem.Object
{

    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Skills>();
        harmony.PatchAll(typeof(Skills));
    }


    [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.Init), new Type[]{})]
    [HarmonyPostfix]
    private static void PatchSkillInit(SkillModel __instance)
    {
        var skillAbilityScript = __instance.GetSkillAbilityScript();
        foreach (var abilityData in skillAbilityScript)
        {
            string scriptName = abilityData.ScriptName;
            if (scriptName.Contains("MeowMeowMeow"))
            {
                CustomEncounterHook.LOG.LogInfo("Crazy shit goin on!!!");
                var instance = new SkillAbilityEvadeThenUseSkill();
                instance.Init(__instance, scriptName, __instance._skillAbilityList.Count, abilityData.BuffData);
                __instance._skillAbilityList.Add(instance);
            }
        }
    }


    [HarmonyPatch(typeof(Il2CppSystem.Type), nameof(Il2CppSystem.Type.GetType), typeof(string))]
    [HarmonyPrefix]
    private static bool PatchGetType(ref string typeName, ref Il2CppSystem.Type __result)
    {
        if (!typeName.StartsWith("BattleDuelViewer_")) return true;
        if (!int.TryParse(typeName.Substring("BattleDuelViewer_".Length), out var viewerID)) return true;
        var skillData = Singleton<StaticDataManager>.Instance.SkillList.GetData(viewerID);
        if (skillData == null) return true;
        foreach (var abilityData in skillData.GetAbilityScript(4))
        {
            var scriptName = abilityData.scriptName;
            if (!scriptName.Contains("DuelViewer_")) continue;
            var appearanceId = scriptName.Replace("DuelViewer_", "");
            typeName = $"BattleDuelViewer_{appearanceId}";
            return true;
        }

        return true;
    }

    [HarmonyPatch(typeof(StageBuffManager), nameof(StageBuffManager.CheckBloodDinnerOnInitStage))]
    [HarmonyPrefix]
    private static void PatchDetectBloodfeast(StageBuffManager __instance)
    {
        var participants = Singleton<StageController>.Instance._stageModel._formationInfo.GetParticipants();
        foreach (var playerUnitData in participants)
        {
            var passiveTable = Singleton<StaticDataManager>.Instance._personalityPassiveList;
            var passive = passiveTable.GetBattlePassiveIDList(playerUnitData.personalityId, playerUnitData.PersonalityLevel);
            if (!passive.Contains(9999907)) continue;
            // constant sampled from UE
            CustomEncounterHook.LOG.LogInfo("Bloodfeast passive detected, activating");
            __instance.AddCandidateToKeyword(BUFF_UNIQUE_KEYWORD.BloodDinner, 921490743);
            __instance.AddStageBuff(BUFF_UNIQUE_KEYWORD.BloodDinner);
            return;
        }
    }


}