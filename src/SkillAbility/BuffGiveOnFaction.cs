using System;
using System.Collections.Generic;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using BepInEx;

namespace CustomEncounter.SkillAbility
{
	internal class BuffGiveOnFaction : MonoBehaviour
	{
		public static void Setup(Harmony harmony)
		{
			ClassInjector.RegisterTypeInIl2Cpp<BuffGiveOnFaction>();
			harmony.PatchAll(typeof(BuffGiveOnFaction));
		}

		[HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnBattleStart))]
		[HarmonyPostfix]
		private static void OnBattleStart(BattleActionModel action, BattleUnitModel __instance, BattleUnitModel target, BATTLE_EVENT_TIMING timing)
		{
			foreach (var ability in action._skill.GetSkillAbilityScript())
			{
				var scriptName = ability.scriptName;
				if (scriptName.Contains("BuffGiveOnFaction_"))
				{
					CustomEncounterHook.LOG.LogInfo("Registered [BuffGiveOnFaction_]");
					if (ability.buffData == null) continue;

					var factionname = scriptName.Replace("BuffGiveOnFaction_", "");
					var parsed_factionname = UNIT_FACTION.Parse<UNIT_FACTION>(factionname);

					var keyword = ability.buffData.buffKeyword;
					var keyword_status = BUFF_UNIQUE_KEYWORD.Parse<BUFF_UNIQUE_KEYWORD>(keyword);

					var potency_check = ability.buffData.stack;
					var count_check = ability.buffData.turn;

					if (target._faction == UNIT_FACTION.PLAYER);
                    {
						BattleUnitModel model = action.Model;
						action.OnUseBuffBySkill(keyword_status, potency_check, count_check);
					}

				};
			}
		}
    }
}
