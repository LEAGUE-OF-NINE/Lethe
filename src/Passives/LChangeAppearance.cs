using HarmonyLib;
using SD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace LetheV2.Passives
{
    internal class LChangeAppearance : MonoBehaviour
    {
        private const int PASSIVE_ID = 1984;
        private const string PASSIVE_NAME = "ChangeAppearance_";
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<LChangeAppearance>();
            harmony.PatchAll(typeof(LChangeAppearance));
        }

        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.Update_Cointoss))]
        [HarmonyPrefix]
        private static void OnStartCoin(BattleUnitView __instance, int index, bool isDuel)
        {
            var actionlog = __instance.CurrentActionLog;
            if (actionlog != null)
                ChangeAppearance_Internal(__instance, actionlog._systemLog, index, isDuel);
            else return;

        }

        private static void ChangeAppearance_Internal(BattleUnitView __instance, BattleLog log, int coinIdx, bool isDuel)
        {
            var actorLog = log.GetAllBehaviourLog_Start().ToArray().ToList().Find(x => x.InstanceID == __instance.InstanceID);
            if (actorLog == null) return;

            int skillID = actorLog._skillID;
            int gacksungLv = actorLog._gaksungLevel;
            var skillData = StaticDataManager.Instance.SkillList.GetData(skillID);

            var findPassive = skillData.GetAbilityScript(gacksungLv).ToArray().ToList().Find(x => x.scriptName.StartsWith(PASSIVE_NAME));
            if (findPassive == null) return;
            __instance.ChangeAppearance(findPassive.scriptName.Substring(PASSIVE_NAME.Length), true);

            var curCoin = skillData.GetCoins(gacksungLv).ToArray().Last();
            if (coinIdx >= 0 && coinIdx < skillData.GetCoins(gacksungLv).Count) {
                curCoin = skillData.GetCoins(gacksungLv).ToArray()[coinIdx];
            }

            if (curCoin != null && !isDuel) {
                var findPassiveInCoin = curCoin.abilityScriptList.ToArray().ToList().Find(x => x.scriptName.StartsWith(PASSIVE_NAME));
                if (findPassiveInCoin == null) return;
                __instance.ChangeAppearance(findPassiveInCoin.scriptName.Substring(PASSIVE_NAME.Length), true);
            }
          
        }

        //change passive when moving, evade, hurt, guard, idle
        [HarmonyPatch(typeof(CharacterAppearance), nameof(CharacterAppearance.ChangeMotion))]
        [HarmonyPostfix]
        private static void ChangeMotion(MOTION_DETAIL motiondetail, bool forcely, CharacterAppearance __instance)
        {
            var data = __instance._battleUnitView._unitModel._unitDataModel._classInfo;
            var unitview = __instance._battleUnitView;
            if (data == null) return;
            if (data.PassiveSetInfo.PassiveIdList.Contains(PASSIVE_ID) && unitview.CurrentActionLog == null)
            {
                switch (motiondetail)
                {
                    case MOTION_DETAIL.Damaged:
                    case MOTION_DETAIL.Damaged_2:
                    case MOTION_DETAIL.Damaged_3:
                    case MOTION_DETAIL.Evade:
                    case MOTION_DETAIL.Move:
                    case MOTION_DETAIL.Guard:
                    case MOTION_DETAIL.Default:
                    case MOTION_DETAIL.Idle:
                        if (motiondetail == MOTION_DETAIL.Idle && forcely == false) return;
                        LetheV2Main.Log.LogWarning($"SWITCH CHANGE APPEARANCE PASSIVE {motiondetail} for {unitview._unitModel.GetOriginUnitID()}");
                        unitview.ChangeAppearance(data.appearance, true);
                        break;
                }
            }
        }

        //if has passive
        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.End_Cointoss))]
        [HarmonyPostfix]
        private static void revert_to_original_on_skill_end(BattleUnitView __instance)
        {
            var data = __instance.unitModel._unitDataModel._classInfo;
            if (data == null) return;
            if (data.PassiveSetInfo.PassiveIdList.Contains(PASSIVE_ID))
            {
                LetheV2Main.Log.LogWarning($"SWITCH CHANGE APPEARANCE PASSIVE [watesigma] for {__instance._unitModel.GetOriginUnitID()}");
                __instance.ChangeAppearance(data.appearance, true);
            }
        }

        //revert on round end
        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.OnRoundEnd))]
        [HarmonyPostfix]
        private static void OnRoundEnd(BattleUnitView __instance)
        {
            string appearanceID = __instance.unitModel.GetAppearanceID();
            var success = int.TryParse(appearanceID, out int __what);
            if (success == true)
            {
                LetheV2Main.Log.LogWarning($"NOT A REAL APPEARANCE {appearanceID}");
                return;
            } //there are better ways to do this? but whatever
            __instance.ChangeAppearance(appearanceID, true);
        }



    }
}
