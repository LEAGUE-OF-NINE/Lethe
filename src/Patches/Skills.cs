using System.Collections.Generic;
using CustomEncounter.SkillAbility;
using HarmonyLib;
using UnhollowerRuntimeLib;

namespace CustomEncounter.Patches;

public class Skills : Il2CppSystem.Object
{

    private static Dictionary<string, Il2CppSystem.Type> typeOverrides = new();
   
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Skills>();
        harmony.PatchAll(typeof(Skills));
        typeOverrides["SkillAbility_EvadeThenUseSkill"] = UnhollowerRuntimeLib.Il2CppType.Of<SkillAbilityEvadeThenUseSkill>();
    }

    [HarmonyPatch(typeof(Il2CppSystem.Type), nameof(Il2CppSystem.Type.GetType), typeof(string))]
    [HarmonyPrefix]
    private static bool PatchGetType(string typeName, ref Il2CppSystem.Type __result)
    {
        if (!typeOverrides.TryGetValue(typeName, out __result)) return true;
        CustomEncounterHook.LOG.LogInfo($"Using custom type {typeName}");
        return false;
    }

}