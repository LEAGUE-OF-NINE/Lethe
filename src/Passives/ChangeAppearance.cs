using CustomEncounter.Passives;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CustomEncounter.Passives
{
    internal class ChangeAppearance : MonoBehaviour
    {

        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<RollAllHeads>();
            harmony.PatchAll(typeof(ChangeAppearance));
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
                if (behavior._instanceID == actor.instanceID && __instance._instanceID == actor.instanceID) //man idk anymore
                {
                    foreach (var abilitydata in skill.GetAbilityScript(gacksungLv))
                    {
                        var scriptName = abilitydata.scriptName;
                        if (scriptName.Contains("ChangeAppearance_"))
                        {
                            var appearanceId = scriptName.Replace("ChangeAppearance_", "");
                            //CustomEncounterHook.Log.LogWarning($"CHANGE APPEARANCE {appearanceId}");
                            __instance.ChangeAppearance(appearanceId, true);
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
            //there is honestly a better way to do this but i wanna sleep
        }


    }
}
