using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using Utils;

namespace CustomEncounter.SkillAbility
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
            }

        }


        private static void ChangeAppearance_Internal(BattleUnitView __instance, BattleLog log, int coinIdx, bool isDuel)
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

        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitModel.OnRoundEnd))]
        [HarmonyPostfix]
        private static void OnRoundEnd(BattleUnitView __instance)
        {
            var name = __instance._appearances[0].name;
            var appearanceId = name.Replace("(Clone)", "");
            __instance.ChangeAppearance(appearanceId, true);
            //there is honestly a better way to do this
        }
    }
}
