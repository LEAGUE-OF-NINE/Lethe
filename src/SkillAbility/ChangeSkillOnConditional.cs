using System;
using System.Collections.Generic;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using BepInEx;

namespace Lethe.SkillAbility
{
	internal class ChangeSkillOnConditional : MonoBehaviour
	{
		public static void Setup(Harmony harmony)
		{
			ClassInjector.RegisterTypeInIl2Cpp<ChangeSkillOnConditional>();
			harmony.PatchAll(typeof(ChangeSkillOnConditional));
		}

		[HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnStartTurn_BeforeLog))]
		[HarmonyPostfix]
		private static void StartTurn_BeforeLog(BattleActionModel action, BattleUnitModel __instance)
		{
			foreach (var ability in action._skill.GetSkillAbilityScript())
			{
				var scriptName = ability.scriptName;
				if (scriptName.Contains("ChangeSkillOnConditional_"))
				{
					LetheHooks.LOG.LogInfo("Registered [ChangeSkillOnConditional_]");
					if (ability.buffData == null) continue;

					var newskillID = Convert.ToInt32(scriptName.Replace("ChangeSkillOnConditional_", ""));
					var whae = delegate (SkillModel x) { return x.GetID() == newskillID; };
					var naenae = __instance.GetSkillList().Find(whae);

					var keyword = ability.buffData.buffKeyword;
					BUFF_UNIQUE_KEYWORD keyword_status;
					if (!Enum.TryParse(keyword, out keyword_status))
					{
						LetheHooks.LOG.LogError($"Invalid status: {keyword}");
						continue;
					}

					var potency_check = ability.buffData.stack;
					var count_check = ability.buffData.turn;
					if (__instance.GetActivatedBuffStack(keyword_status) >= potency_check && __instance.GetActivatedBuffTurn(keyword_status) >= count_check)
					{
						action.ChangeSkill(naenae);
					}
				};
			}
		}
	}
}
