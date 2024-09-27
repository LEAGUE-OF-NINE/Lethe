using System.Collections.Generic;
using Cpp2IL.Core;
using CustomEncounter.SkillAbility;
using HarmonyLib;
using Il2CppSystem;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CustomEncounter.Patches;

public class Skills : Il2CppSystem.Object
{

    private static Dictionary<string, Il2CppSystem.Type> typeOverrides = new();
   
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Skills>();
        harmony.PatchAll(typeof(Skills));
        typeOverrides["SkillAbility_EvadeThenUseSkill"] = UnhollowerRuntimeLib.Il2CppType.Of<SkillAbilityEvadeThenUseSkill>();
    }

    [HarmonyPatch(typeof(BattleActionModelManager), nameof(BattleActionModelManager.GetDefenseAction))]
    [HarmonyPrefix]
    private static void PatchGetDefenseAction(BattleActionModelManager __instance, BattleActionModel attackerAction, BattleUnitModel target, DEFENSE_TYPE type)
    {
        CustomEncounterHook.LOG.LogInfo("=====================================");
        CustomEncounterHook.LOG.LogInfo($"Check defense for {target}, faction {target.Faction}, type {type}, attack from {attackerAction._model.Faction}");
        var key = target.InstanceID;
        if (!__instance._keptDefenseActionDictionary.ContainsKey(key))
        {
            __instance._keptDefenseActionDictionary[key] = new Il2CppSystem.Collections.Generic.List<BattleActionModel>();
        }
        
        var defense = __instance._keptDefenseActionDictionary[key];
        BattleActionModel newDefense = null;
        foreach (var defenseAction in defense)
        {
            CustomEncounterHook.LOG.LogInfo($"Defense {defenseAction._skill.GetID()}, is used {defenseAction._isDone}");
            if (defenseAction._isDone) continue;
            var skillRegistry = Singleton<StaticDataManager>.Instance.SkillList;
            var attackSkillData = attackerAction._skill._skillData;
            attackSkillData = defenseAction._skill._skillData;
            
            var newSkillDataList = new Il2CppSystem.Collections.Generic.List<SkillDataByLevel>();
            foreach (var skillData in skillRegistry.GetData(attackSkillData.id).skillData)
            {
                newSkillDataList.Add(skillData);
                // newSkillDataList.Add(new SkillDataByLevel
                // {
                //     iconID = attackSkillData.id.ToString(),
                //     gaksungLevel = skillData.gaksungLevel,
                //     _attributeType = skillData._attributeType,
                //     attributeType = skillData.attributeType,
                //     _atkType = skillData._atkType,
                //     atkType = skillData.atkType,
                //     _defenseType = DEFENSE_TYPE.COUNTER,
                //     _skillMotion = defenseAction._skill.GetSkillMotion(),
                //     _viewType = defenseAction._skill.GetViewType(),
                //     actionScript = attackSkillData.actionScript,
                //     targetNum = 1,
                //     defaultValue = attackSkillData.defaultSkillPower,
                //     mpUsage = attackSkillData.mpUsage,
                //     abilityScriptList = attackSkillData.abilityScriptList,
                //     coinList = attackSkillData.coinDataList,
                //     actionList = attackSkillData.actionData,
                //     skillLevelCorrection = attackSkillData.skillLevelCorrection,
                //     viewType = "BATTLE",
                //     informationID = -1,
                //     range = -1,
                //     arearange = -1,
                //     _parryingCloseType = PARRYING_CLOSE_TYPE.NEAR,
                // });
            }
            
            var newSkillData = new SkillStaticData
            {
                id = attackSkillData.id * 37 % 2147483647,
                textID = attackSkillData.id,
                skillTier = 1,
                skillData = newSkillDataList,
                requireIDList = new Il2CppSystem.Collections.Generic.List<string>(),
                _skillType = SKILL_TYPE.SKILL,
            };

            if (!skillRegistry.dict.ContainsKey(newSkillData.id))
            {
                skillRegistry.list.Add(newSkillData);
                skillRegistry.dict.Add(newSkillData.id, newSkillData);
            }
                
            newDefense = new BattleActionModel(defenseAction)
            {
                _isDone = false,
                _isReusedAction = false,
                _reusedTimes = 0,
                _actionInstanceID = defenseAction._actionInstanceID, // needed to make coin UI show up
                // by default it is Singleton<StageController>.Instance._stageModel._actionInstanceIDGiver++;
                _skill = new SkillModel(skillRegistry.GetData(1100204), defenseAction._skill.skillData.level, defenseAction._skill.skillData.gaksungLevel),
            };
        }
        
        defense.Add(newDefense);
        CustomEncounterHook.LOG.LogInfo("=====================================");
    }


    [HarmonyPatch(typeof(Il2CppSystem.Type), nameof(Il2CppSystem.Type.GetType), typeof(string))]
    [HarmonyPrefix]
    private static bool PatchGetType(string typeName, ref Il2CppSystem.Type __result)
    {
        if (!typeOverrides.TryGetValue(typeName, out __result)) return true;
        CustomEncounterHook.LOG.LogInfo($"Using custom type {typeName}");
        return false;
    }

}