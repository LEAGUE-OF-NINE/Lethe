using System;
using System.Collections.Generic;
using CustomEncounter.SkillAbility;
using HarmonyLib;
using UnhollowerRuntimeLib;

namespace CustomEncounter.Patches;

public class Skills : Il2CppSystem.Object
{

    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Skills>();
        harmony.PatchAll(typeof(Skills));
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
            return;
        }
    }

}