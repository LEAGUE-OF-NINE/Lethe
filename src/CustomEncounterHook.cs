using System;
using System.Linq;
using BattleUI;
using BattleUI.Operation;
using BepInEx;
using BepInEx.Logging;
using Dungeon;
using HarmonyLib;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.IO;
using Il2CppSystem.Runtime.Serialization.Formatters.Binary;
using MainUI;
using Server;
using SimpleJSON;
using Utils;

namespace CustomEncounter
{
    public class CustomEncounterHook : MonoBehaviour
    {
        public static CustomEncounterHook Instance;
        internal static StageStaticData Encounter;
        internal static ManualLogSource Log;

        internal static void Setup(ManualLogSource log)
        {
            ClassInjector.RegisterTypeInIl2Cpp<CustomEncounterHook>();
            Log = log;

            GameObject obj = new("carra.customencounter.bootstrap");
            DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.HideAndDontSave;
            Instance = obj.AddComponent<CustomEncounterHook>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                EncounterHelper.SaveEncounters();
                EncounterHelper.SaveIdentities();
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                Log.LogInfo("Entering custom fight");
                try
                {
                    var json = File.ReadAllText(CustomEncounterMod.EncounterConfig);
                    Log.LogInfo("Fight data:\n" + json);
                    Encounter = JsonUtility.FromJson<StageStaticData>(json);
                    Log.LogInfo("Success, please go to excavation 1 to start the fight.");
                }
                catch (Exception ex)
                {
                    Log.LogError("Error loading custom fight: " + ex.Message);
                }
            }
        }

        [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
        [HarmonyPostfix]
        private static void SetLoginInfo(LoginSceneManager __instance)
        {
            __instance.tmp_loginAccount.text = "CustomEncounter v" + CustomEncounterMod.VERSION;
        }
        
        [HarmonyPatch(typeof(StaticDataManager), nameof(StaticDataManager.LoadStaticDataFromJsonFile))]
        [HarmonyPostfix]
        private static void LoadStaticDataFromJsonFile(StaticDataManager __instance, string dataClass, ref List<JSONNode> nodeList)
        {
            Log.LogInfo($"Dumping {dataClass}");
            var root = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "limbus_data", dataClass));
            int i = 0;
            foreach (var jsonNode in nodeList)
            {
                File.WriteAllText(Path.Combine(root.FullPath, $"{i}.json"), jsonNode.ToString(2));
            }
            
            root = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_limbus_data", dataClass));
            foreach (var file in Directory.GetFiles(root.FullPath, "*.json"))
            {
                try
                {
                    Log.LogInfo($"Loading {file}");
                    nodeList.Add(JSONNode.Parse(File.ReadAllText(file)));
                }
                catch (Exception ex)
                {
                    Log.LogError($"Error parsing {file}: {ex.GetType()} {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(UserDataManager), nameof(UserDataManager.UpdateData))]
        [HarmonyPostfix]
        private static void UpdateData(UserDataManager __instance, UpdatedFormat updated)
        {
            var unlockedPersonalities = __instance._personalities._personalityList._list;
            unlockedPersonalities.Clear();
            foreach (var personalityStaticData in Singleton<StaticDataManager>.Instance.PersonalityStaticDataList.list)
            {
                var personality = new Personality(personalityStaticData.ID)
                {
                    _gacksung = 4,
                    _level = 45,
                    _acquireTime = new DateUtil()
                };
                unlockedPersonalities.Add(personality);
            }
        }

        [HarmonyPatch(typeof(StageStaticDataList), nameof(StageStaticDataList.GetStage))]
        [HarmonyPrefix]
        private static bool PreGetStage(ref int id, ref StageStaticData __result)
        {
            switch (id)
            {
                case 1 when Encounter != null:
                    __result = Encounter;
                    return false;
                case -1:
                    id = 1;
                    break;
            }

            return true;
        }

        [HarmonyPatch(typeof(PassiveUIManager), nameof(PassiveUIManager.SetData))]
        [HarmonyPrefix]
        private static void PassiveUIManagerSetData(PassiveUIManager __instance)
        {
            // stub, to catch errors
        }
       
        [HarmonyPatch(typeof(DamageStatistics), nameof(DamageStatistics.SetResult))]
        [HarmonyPrefix]
        private static void DamageStatisticsSetResult(DamageStatistics __instance)
        {
            // stub, to catch errors
        }

        [HarmonyPatch(typeof(BattleObjectManager), nameof(BattleObjectManager.CreateAllyUnits), typeof(List<PlayerUnitData>))]

        [HarmonyPrefix]
        private static void CreateAllyUnits(BattleObjectManager __instance, ref List<PlayerUnitData> sortedParticipants)
        {
            int order = sortedParticipants.Count;
            var customAssistantFolder = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_assistant"));
            Log.LogInfo("Scanning custom assistant data");
            foreach (var file in Directory.GetFiles(customAssistantFolder.FullPath, "*.json"))
            {
                try
                {
                    var assistantJson = JSONNode.Parse(File.ReadAllText(file));
                    var personalityStaticDataList = new PersonalityStaticDataList();
                    var assistantJsonList = new List<JSONNode>();
                    assistantJsonList.Add(assistantJson);
                    personalityStaticDataList.Init(assistantJsonList);
                    foreach (var personalityStaticData in personalityStaticDataList.list)
                    {
                        Log.LogInfo($"Adding assistant at {order} Owner: {personalityStaticData.ID}");
                        var personality = new CustomPersonality(11001, 45, 4, 0, false)
                        {
                            _classInfo = personalityStaticData,
                            _battleOrder = order++
                        };
                        var egos = new[] { new Ego(20101, EGO_OWNED_TYPES.USER)  };
                        var unit = new PlayerUnitData(personality, new Il2CppReferenceArray<Ego>(egos), false);
                        sortedParticipants.Add(unit);
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError($"Error parsing assistant data {file}: {ex.GetType()}: {ex.Message}");
                }
            }
        }
    }
}