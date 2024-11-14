using System;
using System.Collections.Generic;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using BepInEx;

namespace CustomEncounter.SkillAbility
{
	internal class RollAllHeadsAbilityScript : MonoBehaviour
	{
		public static void Setup(Harmony harmony)
		{
			ClassInjector.RegisterTypeInIl2Cpp<RollAllHeadsAbilityScript>();
			harmony.PatchAll(typeof(RollAllHeadsAbilityScript));
		}

		[HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnStartCoin))]
		[HarmonyPostfix]
		private static void OnStartCoin(BattleActionModel action, CoinModel coin, BATTLE_EVENT_TIMING timing)
		{
			foreach (var ability in action._skill.GetSkillAbilityScript())
			{
				var scriptName = ability.scriptName;
				// checks if our script contains the right Script Name
				if (scriptName.Contains("RollAllHeadsAbilityScript"))
				{
					CustomEncounterHook.LOG.LogInfo("Registered [RollAllHeadsAbilityScript]");
				};
			}
		}
    }
}
