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
        if (typeName.StartsWith("BattleDuelViewer_")) typeName = "BattleDuelViewer_8389";
    }

}