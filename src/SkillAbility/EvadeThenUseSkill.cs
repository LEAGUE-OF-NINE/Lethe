using HarmonyLib;
using UnhollowerRuntimeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LimbusSandbox.SkillAbility
{
    internal class EvadeThenUseSkill : MonoBehaviour
    {
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<EvadeThenUseSkill>();
            harmony.PatchAll(typeof(EvadeThenUseSkill));
        }


        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnSucceedEvade))]
        [HarmonyPostfix]
        private static void OnEvade(BattleUnitModel __instance, BattleActionModel evadeAction, BattleActionModel attackAction, BATTLE_EVENT_TIMING timing)
        {
            foreach (var ability in evadeAction._skill.GetSkillAbilityScript())
            {
                var scriptName = ability.scriptName;
                if (!scriptName.Contains("EvadeThenUseSkill_")) continue;
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
