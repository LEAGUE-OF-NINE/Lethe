using System;
using System.Collections.Generic;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using BepInEx;

namespace CustomEncounter.SkillAbility
{
	internal class ChangeSkillOnMultiConditional : MonoBehaviour
	{
		public static void Setup(Harmony harmony)
		{
			ClassInjector.RegisterTypeInIl2Cpp<ChangeSkillOnMultiConditional>();
			harmony.PatchAll(typeof(ChangeSkillOnMultiConditional));
		}

		[HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnStartTurn_BeforeLog))]
		[HarmonyPostfix]
		private static void StartTurn_BeforeLog(BattleActionModel action, BattleUnitModel __instance)
		{
			var limtracker = 0;
			foreach (var ability in action._skill.GetSkillAbilityScript())
			{
				var scriptName = ability.scriptName;
				if (scriptName.Contains("ChangeSkillOnMultiConditional_"))
				{
					CustomEncounterHook.LOG.LogInfo("Registered [ChangeSkillOnMultiConditional_]");
					if (ability.buffData == null) continue;
					var limit = ability.buffData.Limit;
					if (limtracker >= limit) continue;

					var isolate = scriptName.Replace("ChangeSkillOnMultiConditional_", "");
					string[] fuck = isolate.Split('_');	

					var old_skill_id = fuck.Length > 0 ? fuck[0] : null;
					string extra_buff_2 = fuck.Length > 1 ? fuck[1] : null;
					string extra_buff_3 = fuck.Length > 2 ? fuck[2] : null;
					var newskillID = Convert.ToInt32(old_skill_id);

					var whae = delegate (SkillModel x) { return x.GetID() == newskillID; };
					var naenae = __instance.GetSkillList().Find(whae);

					var keyword = ability.buffData.buffKeyword;
					var keyword_status1 = BUFF_UNIQUE_KEYWORD.Parse<BUFF_UNIQUE_KEYWORD>(keyword);
					var keyword_status2 = BUFF_UNIQUE_KEYWORD.Parse<BUFF_UNIQUE_KEYWORD>(extra_buff_2);
					var keyword_status3 = BUFF_UNIQUE_KEYWORD.Parse<BUFF_UNIQUE_KEYWORD>(extra_buff_3);

					var potency_check = ability.buffData.stack;
					var count_check = ability.buffData.turn;

					var check_one = __instance.GetActivatedBuffStack(keyword_status1) >= potency_check && __instance.GetActivatedBuffTurn(keyword_status1) >= count_check;
					var check_two = __instance.GetActivatedBuffStack(keyword_status2) >= potency_check && __instance.GetActivatedBuffTurn(keyword_status2) >= count_check;
					var check_three = __instance.GetActivatedBuffStack(keyword_status3) >= potency_check && __instance.GetActivatedBuffTurn(keyword_status3) >= count_check;
					if (check_one && check_two && check_three)
					{
						action.ChangeSkill(naenae);
						limtracker += 1;
						CustomEncounterHook.LOG.LogInfo("Limit is currently" + " " + limtracker);
					}
				};
			}
		}
	}
}
