using HarmonyLib;
using UnhollowerRuntimeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static BattleUI.Abnormality.AbnormalityPartSkills;

namespace LimbusSandbox.SkillAbility
{
    internal class CustomDuelViewer : MonoBehaviour
    {
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<CustomDuelViewer>();
            harmony.PatchAll(typeof(CustomDuelViewer));
        }

        [HarmonyPatch(typeof(Il2CppSystem.Type), nameof(Il2CppSystem.Type.GetType), typeof(string))]
        [HarmonyPrefix]
        private static void PatchGetType(ref string typeName)
        {
            if (!typeName.StartsWith("BattleDuelViewer_")) return;
            if (!int.TryParse(typeName.Substring("BattleDuelViewer_".Length), out var viewerID)) return;
            var skillData = Singleton<StaticDataManager>.Instance.SkillList.GetData(viewerID);
            if (skillData == null) return;
            foreach (var abilityData in skillData.GetAbilityScript(4))
            {
                var scriptName = abilityData.scriptName;
                if (!scriptName.Contains("DuelViewer_")) continue;
                var appearanceId = scriptName.Replace("DuelViewer_", "");
                typeName = $"BattleDuelViewer_{appearanceId}";
                Plugin.Log.LogWarning($"changed duel viewer to {typeName}");
                return;
            }
        }
    }
}
