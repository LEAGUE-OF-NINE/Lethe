﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using LimbusSandbox;
using UnhollowerRuntimeLib;
using UnityEngine;
using SD;

namespace LimbusSandbox.SkillAbility
{
    internal class ChangeSkillMotion : MonoBehaviour
    {
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<ChangeSkillMotion>();
            harmony.PatchAll(typeof(ChangeSkillMotion));
        }

        [HarmonyPatch(typeof(CharacterAppearance), nameof(CharacterAppearance.ChangeMotion))]
        [HarmonyPrefix]
        private static void ChangeMotion(CharacterAppearance __instance, ref MOTION_DETAIL motiondetail, ref int index)
        {
            BattleLog log = null;
            var actionlog = __instance._battleUnitView.CurrentActionLog;
            if (actionlog != null)
            {
                //CustomEncounterHook.Log.LogWarning($"{__instance._instanceID}, {actionlog}");
                log = actionlog._systemLog;
            }
            else return;
            //my hopes are held by glue
            var motionstr = motiondetail.ToString();
            if (!motionstr.StartsWith("S") && motionstr != "Parrying") return;
            if (__instance._battleUnitView._currentDuelViewer != null || __instance._battleUnitView.CurrentActionLog == null) return;
            foreach (var behavior in log.GetAllBehaviourLog_Start()) //get all battle log behaviour
            {
                var skillID = behavior._skillID;
                var actor = log.GetCharacterInfo(behavior._instanceID); //get actor
                Plugin.Log.LogInfo(BattleObjectManager.Instance.GetModel(behavior._instanceID)._originID);
                Plugin.Log.LogInfo(skillID);
                int coinIdx;
                if (__instance._battleUnitView.GetSkillViewer(skillID) != null)
                {
                    coinIdx = __instance._battleUnitView.GetSkillViewer(skillID).curCoinIndex;
                }
                else return;
                var gacksungLv = behavior._gaksungLevel;
                
                var skill = Singleton<StaticDataManager>.Instance._skillList.GetData(skillID);
                //Plugin.Log.LogWarning($"DETAIL: {motiondetail}, INDEX: {coinIdx}");

                if (behavior._instanceID == actor.instanceID && __instance._battleUnitView._instanceID == actor.instanceID) //man idk anymore
                {
                    //change skill motion
                    SkillCoinData coin = skill.GetCoins(gacksungLv)[^1];
                    if (coinIdx >= 0 && coinIdx < skill.GetCoins(gacksungLv).Count)
                    {
                        coin = skill.GetCoins(gacksungLv)[coinIdx];
                        Plugin.Log.LogInfo($" ok {coin}");
                    }


                    foreach (var abilitydata in coin.abilityScriptList)
                    {

                        var scriptName = abilitydata.scriptName;
                        if (scriptName != null)
                        {
                            if (scriptName.Contains("ChangeSkillMotion_"))
                            {
                                var motiontype = scriptName.Replace("ChangeSkillMotion_", "");                           
                                Enum.TryParse<MOTION_DETAIL>(motiontype, out var motion);
                                __instance._currentMotiondetail = motion;                              
                                
                                if (abilitydata.buffData != null)
                                {
                                    index = abilitydata.buffData.turn - 1;
                                }

                                motiondetail = motion;
                                Plugin.Log.LogWarning($"SWITCH TO ANIM {motion}, INDEX {index}");
                            }
                        }
                    }

                }
            }
        }


    }
}
