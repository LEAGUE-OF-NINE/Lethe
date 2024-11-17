using System;
using System.Collections.Generic;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using BepInEx;

namespace CustomEncounter.SkillAbility
{
	internal class ReuseSkillOnConditional : MonoBehaviour
	{
		public static void Setup(Harmony harmony)
		{
			ClassInjector.RegisterTypeInIl2Cpp<ReuseSkillOnConditional>();
			harmony.PatchAll(typeof(ReuseSkillOnConditional));
		}

		[HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnStartTurn_BeforeLog))]
		[HarmonyPostfix]
		private static void StartTurn_BeforeLog(BattleActionModel action, BattleUnitModel __instance)
		{
			foreach (var ability in action._skill.GetSkillAbilityScript())
			{
				var scriptName = ability.scriptName;
				if (scriptName.Contains("ReuseSkillOnConditional_"))
				{
					CustomEncounterHook.LOG.LogInfo("Registered [ReuseSkillOnConditional_]");
					if (ability.buffData == null) continue;

					var newskillID = Convert.ToInt32(scriptName.Replace("ReuseSkillOnConditional_", ""));
					var whae = delegate (SkillModel x) { return x.GetID() == newskillID; };
					var naenae = __instance.GetSkillList().Find(whae);

					var keyword = ability.buffData.buffKeyword;
					var keyword_status = BUFF_UNIQUE_KEYWORD.Parse<BUFF_UNIQUE_KEYWORD>(keyword);

					var potency_check = ability.buffData.stack;
					var count_check = ability.buffData.turn;
					if (__instance.GetActivatedBuffStack(keyword_status) >= potency_check && __instance.GetActivatedBuffTurn(keyword_status) >= count_check)
					{
						__instance.ReuseAction(action);
					}
				};
			}
		}
	}
}
