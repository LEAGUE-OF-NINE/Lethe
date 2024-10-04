using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CustomEncounter.SkillAbility
{
    internal class newEvadeThenUseSkill : MonoBehaviour
    {
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<newEvadeThenUseSkill>();
            harmony.PatchAll(typeof(newEvadeThenUseSkill));
        }
        
        //evade then use skill
        [HarmonyPatch(typeof(BattleActionModel), nameof(BattleActionModel.OnTryEvade))]
        [HarmonyPostfix]
        private static void OnEvade(BattleActionModel __instance, BattleActionModel attackerAction, BATTLE_EVENT_TIMING timing)
        {
            //temporary action
            var model = __instance._model;
            var actionSlot = model._actionSlotDetail;
            var sinActionModel = actionSlot.CreateSinActionModel(true);
            actionSlot.AddSinActionModelToSlot(sinActionModel);

            //funny action stuff
            var sinModel = new UnitSinModel(1050805, model, sinActionModel, false);
            var battleActionModel = new BattleActionModel(sinModel, model, sinActionModel, -1);
            battleActionModel._targetDataDetail.ReadyOriginTargeting(battleActionModel);
            model.CutInDefenseActionForcely(battleActionModel, true);
            var enemyBattleAction = attackerAction;
            battleActionModel.ChangeMainTargetSinAction(enemyBattleAction._sinAction, enemyBattleAction, true);

        }

        //add BattleSkillViewer so it works
        //add every skill there is
        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.Start))]
        [HarmonyPostfix]
        private static void InitSkills(BattleUnitView __instance)
        {
            CustomEncounterHook.LOG.LogWarning("when the weather outside is rizzy, and the gyatt is so skibidi");
            var list = new Il2CppSystem.Collections.Generic.List<SkillModel>();
                var newSkillModel = new SkillModel(Singleton<StaticDataManager>.Instance._skillList.GetData(1050805), 45, 4);
                list.Add(newSkillModel);

            __instance.Init_Skills(list, false);
        }
    }
}
