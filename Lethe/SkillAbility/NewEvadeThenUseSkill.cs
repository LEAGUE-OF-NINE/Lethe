using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace Lethe.SkillAbility
{
    internal class NewEvadeThenUseSkill : MonoBehaviour
    {
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<NewEvadeThenUseSkill>();
            harmony.PatchAll(typeof(NewEvadeThenUseSkill));
        }

        private static Dictionary<int, int> wahoo = new(); //unitmodel instance id, evade count
        private static Dictionary<int, List<int>> bruh = new(); //unitmodel instance id, actionmodel instance ids
        [HarmonyPatch(typeof(StageController), nameof(StageController.StartRound))]
        [HarmonyPostfix]
        private static void StartRound()
        {
            wahoo.Clear();
            bruh.Clear();
        }

        //evade then use skill
        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnSucceedEvade))]
        [HarmonyPostfix]
        private static void OnEvade(BattleUnitModel __instance, BattleActionModel evadeAction, BattleActionModel attackAction, BATTLE_EVENT_TIMING timing)
        {
            var instanceID = __instance._instanceID;
            var attackInstanceID = attackAction.ActionInstanceID;
            foreach (var ability in evadeAction._skill.GetSkillAbilityScript())
            {
                var scriptName = ability.scriptName;
                if (!scriptName.Contains("EvadeThenUseSkill_")) continue;
                wahoo.TryAdd(instanceID, 0);
                bruh.TryAdd(instanceID, new List<int> { });
                if (bruh[instanceID].Contains(attackInstanceID)) return;
                if (ability.buffData != null)
                    LetheHooks.LOG.LogInfo($"limit: {ability.buffData.limit}, current: {wahoo[instanceID]}");
                    if (wahoo[instanceID] >= ability.buffData.limit && ability.buffData.limit != 0)
                    {
                        LetheHooks.LOG.LogInfo("RETURN EVAADE");
                        return;
                    }
                bruh[instanceID].Add(attackInstanceID);
                LetheHooks.LOG.LogInfo($"EVADING {scriptName} action id {evadeAction.ActionInstanceID} enemy action id {attackInstanceID}");
                wahoo[instanceID]++;
                //temporary action
                var model = __instance;
                var actionSlot = model._actionSlotDetail;
                var sinActionModel = actionSlot.CreateSinActionModel(true);
                actionSlot.AddSinActionModelToSlot(sinActionModel);

                var skillID = Convert.ToInt32(scriptName.Replace("EvadeThenUseSkill_", ""));
                var unitView = SingletonBehavior<BattleObjectManager>.Instance.GetView(model);
                var unitDataModel = model._unitDataModel;

                //funny action stuff
                var sinModel = new UnitSinModel(skillID, model, sinActionModel, false);
                var battleActionModel = new BattleActionModel(sinModel, model, sinActionModel, -1);
                battleActionModel._targetDataDetail.ReadyOriginTargeting(battleActionModel);
                model.CutInDefenseActionForcely(battleActionModel, true);
                var enemyBattleAction = attackAction;
                battleActionModel.ChangeMainTargetSinAction(enemyBattleAction._sinAction, enemyBattleAction, true);

                //change skill and sinModel?
                battleActionModel._skill = new SkillModel(Singleton<StaticDataManager>.Instance._skillList.GetData(skillID), unitDataModel.Level, unitDataModel.SyncLevel)
                {
                    _skillData =  {
                        _defenseType = (int)DEFENSE_TYPE.COUNTER,
                        canDuel = false,
                        _targetType = (int)SKILL_TARGET_TYPE.FRONT,
                        _skillMotion = (int)MOTION_DETAIL.S3
                    }
                };
                sinModel._skillId = skillID;


                //add BattleSkillViewer
                var skillViewer = new BattleSkillViewer(unitView, skillID.ToString(), battleActionModel._skill);
                unitView._battleSkillViewers.TryAdd(skillID.ToString(), skillViewer);
                break;
            }
        }
    }
}