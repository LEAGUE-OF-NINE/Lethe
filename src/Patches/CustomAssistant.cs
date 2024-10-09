using System;
using System.Collections.Generic;
using BattleUI;
using HarmonyLib;
using Il2CppSystem.IO;
using SimpleJSON;
using UnhollowerBaseLib;
using Utils;

namespace CustomEncounter.Patches;

public class CustomAssistant : Il2CppSystem.Object
{
    private static readonly Dictionary<int, PersonalityStaticData> CustomPersonalityRegistry = new();
    
    public static void Setup(Harmony harmony)
    {
        // ClassInjector.RegisterTypeInIl2Cpp<CustomAssistant>();
        harmony.PatchAll(typeof(CustomAssistant));
    }

    [HarmonyPatch(typeof(PassiveUIManager), nameof(PassiveUIManager.SetData))]
    [HarmonyPrefix]
    private static void PassiveUIManagerSetData(PassiveUIManager __instance)
    {
        for (var i = 0; i < CustomPersonalityRegistry.Count; i++)
            __instance._passiveIconSlotList.Add(__instance._passiveIconSlotList.GetFirstElement());
        // TODO: Error happens here because custom units mess with the passive bar on the left, is there a better way to fix this?
        // stub, to catch errors
    }

    [HarmonyPatch(typeof(PersonalityStaticDataList), nameof(PersonalityStaticDataList.GetData))]
    [HarmonyPrefix]
    private static bool PersonalityStaticDataListGetData(ref int id, ref PersonalityStaticData __result)
    {
        if (CustomPersonalityRegistry.TryGetValue(id, out __result))
            return false;
        return true;
    }

    [HarmonyPatch(typeof(BattleObjectManager), nameof(BattleObjectManager.CreateAllyUnits),
        typeof(Il2CppSystem.Collections.Generic.List<PlayerUnitData>))]
    [HarmonyPrefix]
    private static void CreateAllyUnits(BattleObjectManager __instance,
        ref Il2CppSystem.Collections.Generic.List<PlayerUnitData> sortedParticipants)
    {
        CustomPersonalityRegistry.Clear();

        var order = sortedParticipants.Count;
        CustomEncounterHook.LOG.LogInfo("Scanning custom assistant data");
        foreach (var file in Directory.GetFiles(CustomEncounterHook.CustomAssistantDir.FullName, "*.json"))
            try
            {
                var assistantJson = JSONNode.Parse(File.ReadAllText(file));
                var personalityStaticDataList = new PersonalityStaticDataList();
                var assistantJsonList = new Il2CppSystem.Collections.Generic.List<JSONNode>();
                assistantJsonList.Add(assistantJson);
                personalityStaticDataList.Init(assistantJsonList);
                foreach (var personalityStaticData in personalityStaticDataList.list)
                {
                    CustomEncounterHook.LOG.LogInfo($"Adding assistant at {order} Owner: {personalityStaticData.ID}");
                    var personality = new CustomPersonality(11001, 45, 4, 0, false)
                    {
                        _classInfo = personalityStaticData,
                        _battleOrder = order++
                    };
                    var egos = new[] { new Ego(20101, EGO_OWNED_TYPES.USER) };
                    var unit = new PlayerUnitData(personality, new Il2CppReferenceArray<Ego>(egos), false);
                    CustomPersonalityRegistry[personalityStaticData.ID] = personalityStaticData;
                    sortedParticipants.Add(unit);
                }
            }
            catch (Exception ex)
            {
                CustomEncounterHook.LOG.LogError($"Error parsing assistant data {file}: {ex.GetType()}: {ex.Message}");
            }
    }
}