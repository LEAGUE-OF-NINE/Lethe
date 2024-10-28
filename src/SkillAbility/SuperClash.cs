using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using UnhollowerRuntimeLib;

namespace CustomEncounter.SkillAbility
{
    internal class SuperClash : MonoBehaviour
    {
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<SuperClash>();
            harmony.PatchAll(typeof(SuperClash));
        }

        [HarmonyPatch(typeof(Il2CppSystem.Type), nameof(Il2CppSystem.Type.GetType))]
        [HarmonyPatch(new Type[] { typeof(string) })]
        [HarmonyPrefix]
        private static void BattleDuelViewer_SuperClash(ref string typeName)
        {
            if (typeName.Contains("BattleDuelViewer_")
                && !typeName.Contains("BattleDuelViewer_Special")
                && !typeName.Contains("BattleDuelViewer_Tutorial")
                && !typeName.Contains("SkillAbility"))
            {
                try
                {

                    var skillID = Convert.ToInt32(typeName.Replace("BattleDuelViewer_", ""));
                    var skill = StaticDataManager.Instance._skillList.GetData(skillID);
                    if (skill != null)
                    {
                        foreach (var abilityScript in skill.GetAbilityScript(skill.skillTier))
                        {
                            if (abilityScript.scriptName.Contains("BattleDuelViewer_SuperClash"))
                            {
                                typeName = "BattleDuelViewer_8389";
                            }
                        }
                    }

                }
                catch (FormatException e) { }
            }
            else return;
        }


    }
}
