using HarmonyLib;
using UnhollowerRuntimeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ProBuilder;
using SD;
using static Cinemachine.CinemachinePathBase;

namespace LimbusSandbox.SkillAbility
{
    internal class ChangeAppearance : MonoBehaviour
    {
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<ChangeAppearance>();
            harmony.PatchAll(typeof(ChangeAppearance));
        }


        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.Update_Cointoss))]
        [HarmonyPrefix]
        private static void OnStartCoin(BattleUnitView __instance, int index, bool isDuel)
        {
            var actionlog = __instance.CurrentActionLog;
            if (actionlog != null)
            {
                ChangeAppearance_Internal(__instance, actionlog._systemLog, index, isDuel);
            }
            else return;
                               
        }

        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.StartCoinToss))]
        [HarmonyPrefix]
        private static void StartCoinToss(BattleUnitView __instance, BattleLog log, int coinIdx, bool isDuel)
        {
            if (__instance._battleCharacterState.IsEvade == true)
            {
                ChangeAppearance_Internal(__instance, log, coinIdx, isDuel);
                Plugin.Log.LogInfo("OK");
            }
            
        }


        private static void ChangeAppearance_Internal(BattleUnitView __instance ,BattleLog log, int coinIdx, bool isDuel)
        {
            foreach (var behavior in log.GetAllBehaviourLog_Start()) //get all battle log behaviour (so it can change during clash)
            {
                var skillID = behavior._skillID;
                var gacksungLv = behavior._gaksungLevel;
                var actor = log.GetCharacterInfo(behavior._instanceID); //get actor
                var skill = StaticDataManager.Instance._skillList.GetData(skillID);


                if (behavior._instanceID == actor.instanceID && __instance._instanceID == actor.instanceID) //man idk anymore
                {
                    //change appearance on clash
                    foreach (var abilitydata in skill.GetAbilityScript(gacksungLv))
                    {

                        var scriptName = abilitydata.scriptName;
                        if (scriptName.Contains("ChangeAppearance_"))
                        {
                            var appearanceId = scriptName.Substring("ChangeAppearance_".Length);
                            __instance.ChangeAppearance(appearanceId, true);
                            break;
                        }

                    }

                    //change appearance on attack
                    //change skill motion
                    SkillCoinData coin = skill.GetCoins(gacksungLv)[^1];
                    if (coinIdx >= 0 && coinIdx < skill.GetCoins(gacksungLv).Count)
                    {
                        coin = skill.GetCoins(gacksungLv)[coinIdx];
                    }

                    if (coin != null && isDuel == false)
                    {
                        //Plugin.Log.LogWarning($"DETECT COIN USE FOR UNIT!");
                        //Plugin.Log.LogWarning($"UNIT: {__instance._unitModel._originID}, INSTANCE ID: {__instance._instanceID}, TEAM: {__instance._unitModel._faction}");
                        foreach (var abilitydata in coin.abilityScriptList)
                        {

                            var scriptName = abilitydata.scriptName;
                            if (scriptName != null)
                            {
                                if (scriptName.Contains("ChangeAppearance_"))
                                {
                                    var appearanceId = scriptName.Substring("ChangeAppearance_".Length);
                                    __instance.ChangeAppearance(appearanceId, true);
                                    break;
                                }
                            }
                        }

                    }
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
            if(data.PassiveSetInfo.PassiveIdList.Contains(1984) && unitview.CurrentActionLog == null)
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
                        Plugin.Log.LogWarning($"SWITCH CHANGE APPEARANCE PASSIVE {motiondetail} for {unitview._unitModel.GetOriginUnitID()}");
                        unitview.ChangeAppearance(data.appearance, true);
                        break;
                }
            }
        }

        public static Dictionary<int, string> appearanceRec = new();
        [HarmonyPatch(typeof(GlobalGameManager), nameof(GlobalGameManager.LoadScene))]
        [HarmonyPostfix]
        private static void funky2(SCENE_STATE state)
        {
            appearanceRec.Clear();
        }

        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.OnRoundStart))]
        [HarmonyPostfix]
        private static void ok(BattleUnitView __instance)
        {
            var instanceID = __instance._instanceID;
            var name = __instance._curAppearance.name;
            if (!appearanceRec.ContainsKey(instanceID)) appearanceRec.Add(instanceID, name);
            else
            {
                if (appearanceRec[instanceID] != name) appearanceRec[instanceID] = name;
            }
        }


        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.OnRoundEnd))]
        [HarmonyPostfix]
        private static void OnRoundEnd(BattleUnitView __instance)
        {
            var name = appearanceRec[__instance._instanceID];
            __instance.ChangeAppearance(name, true);
        }
    }
}
