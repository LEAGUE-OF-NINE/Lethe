using System;
using System.Collections.Generic;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using BepInEx;

namespace CustomEncounter.SkillAbility
{
	internal class LowerBuffIfGreaterByTurn : MonoBehaviour
	{
		public static void Setup(Harmony harmony)
		{
			ClassInjector.RegisterTypeInIl2Cpp<LowerBuffIfGreaterByTurn>();
			harmony.PatchAll(typeof(LowerBuffIfGreaterByTurn));
		}

		[HarmonyPatch(typeof(BattleUnitModel), nameof(BattleActionModel.OnStartTurn_AfterLog))]
		[HarmonyPostfix]
		private static void OnStartTurn_AfterLog(BattleActionModel action, BattleUnitModel __instance, BATTLE_EVENT_TIMING timing)
		{
			foreach (var ability in action._skill.GetSkillAbilityScript())
			{
				var scriptName = ability.scriptName;
				// checks if our script contains the right Script Name
				if (scriptName.Contains("LowerBuffIfGreaterByTurn"))
				{
					CustomEncounterHook.LOG.LogInfo("Registered [LowerBuffIfGreaterByTurn]");
					// if buffdata is empty, skip this.
					if (ability.buffData == null) continue;

					// parses keyword data
					var keyword = ability.buffData.buffKeyword;
					var keyword_status = BUFF_UNIQUE_KEYWORD.Parse<BUFF_UNIQUE_KEYWORD>(keyword);

					// grabs how much count was input
					var count_check = ability.buffData.turn;
					// grabs how much count we currenty have
					var current_count = __instance.GetActivatedBuffTurn(keyword_status);

					// grabs our current count and lowers it by input count.
					var reduction_amount_count = current_count - count_check;

					// if our current count is higher than our input count, continue 
					if (current_count > count_check)
                    {
						// lowers our status effect. battle_event_timing doesn't really matter
						__instance.LoseBuffTurn(keyword_status, reduction_amount_count, BATTLE_EVENT_TIMING.ON_START_TURN);
                    }
				};
			}
		}
    }
}
