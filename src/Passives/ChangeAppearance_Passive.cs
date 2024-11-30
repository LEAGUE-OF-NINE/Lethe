using HarmonyLib;
using SD;
using UnhollowerRuntimeLib;
using UnityEngine;
using Utils;

namespace Lethe.Passives
{
    internal class ChangeAppearance_Passive : MonoBehaviour
    {

        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<ChangeAppearance_Passive>();
            harmony.PatchAll(typeof(ChangeAppearance_Passive));
        }

        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.StartCoinToss))]
        [HarmonyPrefix]
        private static void OnStartCoin(BattleUnitView __instance, BattleLog log)
        {
            foreach (var behavior in log.GetAllBehaviourLog_Start()) //get all battle log behaviour (so it can change during clash)
            {
                var skillID = behavior._skillID;
                var gacksungLv = behavior._gaksungLevel;
                var actor = log.GetCharacterInfo(behavior._instanceID); //get actor
                var skill = Singleton<StaticDataManager>.Instance._skillList.GetData(skillID);
                //CustomEncounterHook.Log.LogWarning($"COIN TOSS {skillID}");
                if (behavior._instanceID != actor.instanceID || __instance._instanceID != actor.instanceID) continue; //man idk anymore
                foreach (var abilitydata in skill.GetAbilityScript(gacksungLv))
                {
                    var scriptName = abilitydata.scriptName;
                    if (!scriptName.Contains("ChangeAppearance_")) continue;
                    var appearanceId = scriptName.Replace("ChangeAppearance_", "");
                    //CustomEncounterHook.Log.LogWarning($"CHANGE APPEARANCE {appearanceId}");
                    __instance.ChangeAppearance(appearanceId, true);
                }
            }
        }

        [HarmonyPatch(typeof(CharacterAppearance), nameof(CharacterAppearance.ChangeMotion))]
        [HarmonyPostfix]
        private static void ChangeMotion(MOTION_DETAIL motiondetail, CharacterAppearance __instance)
        {
            var data = __instance._battleUnitView._unitModel._unitDataModel._classInfo;
            var unitview = __instance._battleUnitView;
            if (data == null) return;
            if (data.PassiveSetInfo.PassiveIdList.Contains(1984) && unitview.CurrentActionLog == null)
            {
                switch (motiondetail)
                {
                    case MOTION_DETAIL.Damaged:
                    case MOTION_DETAIL.Damaged_2:
                    case MOTION_DETAIL.Damaged_3:
                    case MOTION_DETAIL.Evade:
                    case MOTION_DETAIL.Move:
                    case MOTION_DETAIL.Guard:
                    case MOTION_DETAIL.Idle:
                        LetheHooks.LOG.LogWarning($"SWITCH CHANGE APPEARANCE PASSIVE {motiondetail} for {unitview._unitModel.GetOriginUnitID()}");
                        unitview.ChangeAppearance(data.appearance, true);
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.OnRoundEnd))]
        [HarmonyPostfix]
        private static void OnRoundEnd(BattleUnitView __instance)
        {
            __instance.ChangeAppearance(__instance.unitModel.GetAppearanceID(), true);
        }

    }
}
