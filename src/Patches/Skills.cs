using HarmonyLib;
using UnhollowerRuntimeLib;

namespace Lethe.Patches;

public class Skills : Il2CppSystem.Object
{

    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Skills>();
        harmony.PatchAll(typeof(Skills));
    }


    [HarmonyPatch(typeof(Il2CppSystem.Type), nameof(Il2CppSystem.Type.GetType), typeof(string))]
    [HarmonyPrefix]
    private static void PatchGetType(ref string typeName)
    {
        if (!typeName.StartsWith("BattleDuelViewer_")) return;
        if (!int.TryParse(typeName.Substring("BattleDuelViewer_".Length), out var viewerID)) return;
        var skillData = Singleton<StaticDataManager>.Instance.SkillList.GetData(viewerID);
        if (skillData == null) return;
        foreach (var abilityData in skillData.GetAbilityScript(4))
        {
            var scriptName = abilityData.scriptName;
            if (!scriptName.Contains("DuelViewer_")) continue;
            var appearanceId = scriptName.Replace("DuelViewer_", "");
            typeName = $"BattleDuelViewer_{appearanceId}";
            return;
        }
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
            LetheHooks.LOG.LogInfo("Bloodfeast passive detected, activating");
            __instance.AddCandidateToKeyword(BUFF_UNIQUE_KEYWORD.BloodDinner, 921490743);
            __instance.AddStageBuff(BUFF_UNIQUE_KEYWORD.BloodDinner);
            return;
        }
    }


}