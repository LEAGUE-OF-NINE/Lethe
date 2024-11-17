using System;
using System.Collections.Generic;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using BepInEx;

namespace CustomEncounter.SkillAbility
{
	internal class LowerBuffIfGreaterByStack : MonoBehaviour
	{
		public static void Setup(Harmony harmony)
		{
			ClassInjector.RegisterTypeInIl2Cpp<LowerBuffIfGreaterByStack>();
			harmony.PatchAll(typeof(LowerBuffIfGreaterByStack));
		}

		[HarmonyPatch(typeof(BattleUnitModel), nameof(BattleActionModel.OnStartTurn_AfterLog))]
		[HarmonyPostfix]
		private static void OnStartTurn_AfterLog(BattleActionModel action, BattleUnitModel __instance, BATTLE_EVENT_TIMING timing)
		{
			foreach (var ability in action._skill.GetSkillAbilityScript())
			{
				var scriptName = ability.scriptName;
				// checks if our script contains the right Script Name
				if (scriptName.Contains("LowerBuffIfGreaterByStack"))
				{
					CustomEncounterHook.LOG.LogInfo("Registered [LowerBuffIfGreaterByStack]");
					// if buffdata is empty, skip this.
					if (ability.buffData == null) continue;

					// parses keyword data
					var keyword = ability.buffData.buffKeyword;
					var keyword_status = BUFF_UNIQUE_KEYWORD.Parse<BUFF_UNIQUE_KEYWORD>(keyword);

					// grabs how much potency was input
					var potency_check = ability.buffData.stack;
					// grabs how much potency we currenty have
					var current_potency = __instance.GetActivatedBuffStack(keyword_status);

					// grabs our current potency and lowers it by input potency. does the same for count
					var reduction_amount_stack = current_potency - potency_check;

					// if our current potency is higher than our input potency, continue 
					if (current_potency > potency_check)
                    {
						// lowers our status effect. battle_event_timing doesn't really matter
						__instance.LoseBuffStack(keyword_status, reduction_amount_stack, BATTLE_EVENT_TIMING.ON_START_TURN);
                    }
				};
			}
		}
    }
}
