using System;
using System.Collections.Generic;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using BepInEx;

namespace Lethe.SkillAbility
{
	internal class CustomFurioso : MonoBehaviour
	{
		public static void Setup(Harmony harmony)
		{
			ClassInjector.RegisterTypeInIl2Cpp<CustomFurioso>();
			harmony.PatchAll(typeof(CustomFurioso));
		}

		[HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnStartTurn_BeforeLog))]
		[HarmonyPostfix]
		private static void StartTurn_BeforeLog(BattleActionModel action, BattleUnitModel __instance)
		{
			foreach (var ability in action._skill.GetSkillAbilityScript())
			{
				var scriptName = ability.scriptName;
				if (scriptName.Contains("CustomFurioso_"))
				{
					LetheHooks.LOG.LogInfo("Registered [CustomFurioso_]");
					if (ability.buffData == null) continue;

					var newskillID = Convert.ToInt32(scriptName.Replace("CustomFurioso_", ""));
					var whae = delegate (SkillModel x) { return x.GetID() == newskillID; };
					var naenae = __instance.GetSkillList().Find(whae);

					var keyword_status1 = BUFF_UNIQUE_KEYWORD.MDcFaBa;
					var keyword_status2 = BUFF_UNIQUE_KEYWORD.MDcFaBb;
					var keyword_status3 = BUFF_UNIQUE_KEYWORD.MDcFaBd;

					var potency_check = ability.buffData.stack;
					var count_check = ability.buffData.turn;
					if (__instance.GetActivatedBuffStack(keyword_status1) >= 3 && __instance.GetActivatedBuffStack(keyword_status2) >= 3 && __instance.GetActivatedBuffStack(keyword_status3) >= 3)
					{
						action.ChangeSkill(naenae);
					}
				};
			}
		}
	}
}
