using System.Collections.Generic;
using CustomEncounter.SkillAbility;
using HarmonyLib;
using Il2CppSystem;
using UnhollowerRuntimeLib;

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
            if (!defenseAction._isDone)
            {
                newDefense = new BattleActionModel(defenseAction)
                {
                    _isDone = false,
                    _isReusedAction = false,
                    _reusedTimes = 0,
                    _actionInstanceID = defenseAction._actionInstanceID, // needed to make coin UI show up
                    // by default it is Singleton<StageController>.Instance._stageModel._actionInstanceIDGiver++;
                    
                    _skill = new SkillModel(attackerAction._skill)
                    {
                        _skillData =
                        {
                            canDuel = false,
                            level = defenseAction._skill._skillData.level,
                            _defenseType = (int) DEFENSE_TYPE.COUNTER,
                            _skillMotion = defenseAction._skill._skillData._skillMotion,
                            _targetType = (int) SKILL_TARGET_TYPE.FRONT,
                        }
                    }
                };
            }
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