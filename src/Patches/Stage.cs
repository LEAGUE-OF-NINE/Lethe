using System.Diagnostics;
using System.Text.RegularExpressions;
using HarmonyLib;
using UnhollowerRuntimeLib;

namespace Lethe.Patches;

public class Stage : Il2CppSystem.Object
{

    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Stage>();
        harmony.PatchAll(typeof(Stage));
    }
    
    
    [HarmonyPatch(typeof(StageModel), nameof(StageModel.Init))]
    [HarmonyPrefix]
    private static void Prefix_StageModel_Init(StageStaticData stageinfo, StageModel __instance)
    {
        var tag = "assistant_";
        var pattern = @"^assistant_(\d+)_(\d+)_(\d+)$"; 
        var stageScriptList = stageinfo.stageScriptList;
        foreach (var stageScriptString in stageScriptList)
        {
            if (!stageScriptString.StartsWith(tag)) continue;
            var match = Regex.Match(stageScriptString, pattern);
            if (!match.Success)
            {
                LetheHooks.LOG.LogWarning($"Lethe recognized stage script {stageScriptString}, but script name seems malformed?");
                continue;
            }
            var id = int.Parse(match.Groups[1].Value);
            var level = int.Parse(match.Groups[2].Value);
            var uptie = int.Parse(match.Groups[3].Value);
            var seed = (int) Stopwatch.GetTimestamp();
            SingletonBehavior<BattleObjectManager>.Instance.CreateAssistantUnit(id, level, uptie, seed);
        }
    }

   
}