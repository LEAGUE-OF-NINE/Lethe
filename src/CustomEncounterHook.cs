using System;
using System.Linq;
using System.Reflection;
using Addressable;
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
using SD;
using Server;
using SimpleJSON;
using UI.Utility;
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
        [HarmonyPrefix]
        private static void LoadStaticDataFromJsonFile(StaticDataManager __instance, string dataClass, ref List<JSONNode> nodeList)
        {
            Log.LogInfo($"Dumping {dataClass}");
            var root = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "limbus_data", dataClass));
            int i = 0;
            foreach (var jsonNode in nodeList)
            {
                File.WriteAllText(Path.Combine(root.FullPath, $"{i}.json"), jsonNode.ToString(2));
                i++;
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
            for (int i = 0; i < CUSTOM_PERSONALITY_REGISTRY.Count; i++)
            {
                __instance._passiveIconSlotList.Add(__instance._passiveIconSlotList.GetFirstElement());
            }
            // TODO: Error happens here because custom units mess with the passive bar on the left, is there a better way to fix this?
            // stub, to catch errors
        }
       
        [HarmonyPatch(typeof(DamageStatistics), nameof(DamageStatistics.SetResult))]
        [HarmonyPrefix]
        private static void DamageStatisticsSetResult(DamageStatistics __instance)
        {
            // stub, to catch errors
        }
       
        [HarmonyPatch(typeof(BuffAbility_EgoAwakenDongrangTree), nameof(BuffAbility_EgoAwakenDongrangTree.OnRoundStart_After_Event))]
        [HarmonyPrefix]
        private static void BuffAbility_EgoAwakenDongrangTreeOnRoundStart_After_Event(BuffAbility_EgoAwakenDongrangTree __instance)
        {
            // stub, to catch errors
        }

        private static System.Collections.Generic.Dictionary<int, PersonalityStaticData> CUSTOM_PERSONALITY_REGISTRY = new();
        
        [HarmonyPatch(typeof(PersonalityStaticDataList), nameof(PersonalityStaticDataList.GetData))]
        [HarmonyPrefix]
        private static bool PersonalityStaticDataListGetData(ref int id, ref PersonalityStaticData __result)
        {
            if (CUSTOM_PERSONALITY_REGISTRY.TryGetValue(id, out __result))
                return false;
            return true;
        }

        public static bool CreateSkinForModel(BattleUnitView view, BattleUnitModel unit, Transform parent,
            out DelegateEvent handler, ref CharacterAppearance __result)
        {
            return CreateSkin(view, unit.GetAppearanceID(), parent, out handler, ref __result);
        }

        public static bool CreateSkin(BattleUnitView view, string appearanceID, Transform parent,
            out DelegateEvent handle, ref CharacterAppearance __result)
        {
            var label = "";
            handle = null;
            Log.LogInfo($"Loading asset {label}: {appearanceID}");

            var prefix = "!abno_";
            if (appearanceID.StartsWith(prefix))
            {
                label = SDCharacterSkinUtil._LABEL_ABNORMALITY;
                appearanceID = appearanceID.Substring(prefix.Length);
            }

            prefix = "!enemy_";
            if (appearanceID.StartsWith(prefix))
            {
                label = SDCharacterSkinUtil._LABEL_ENEMY;
                appearanceID = appearanceID.Substring(prefix.Length);
            }

            if (label == "")
            {
                return true;
            }

            var res = AddressableManager.Instance.LoadAssetSync<GameObject>(label, appearanceID, parent);
            if (res == null)
            {
                return true;
            }
           
            var skin = res.Item1.GetComponent<CharacterAppearance>();
            if (skin != null) {
              skin.Initialize(view);
              skin.charInfo.appearanceID = appearanceID;
              handle = res.Item2;
              __result = skin;
              return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(BattleObjectManager), nameof(BattleObjectManager.CreateAllyUnits), typeof(List<PlayerUnitData>))]
        [HarmonyPrefix]
        private static void CreateAllyUnits(BattleObjectManager __instance, ref List<PlayerUnitData> sortedParticipants)
        {
            CUSTOM_PERSONALITY_REGISTRY.Clear();
            
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
                        CUSTOM_PERSONALITY_REGISTRY[personalityStaticData.ID] = personalityStaticData;
                        sortedParticipants.Add(unit);
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError($"Error parsing assistant data {file}: {ex.GetType()}: {ex.Message}");
                }
            }
        }
        
        [HarmonyPatch(typeof(StageController), nameof(StageController.InitStage))]
        [HarmonyPrefix]
        private static void PreInitStage(StageController __instance, bool isCleared, bool isSandbox)
        {
            Log.LogInfo("Pre-InitStage " + isCleared + " " + isSandbox);
        }
        
        [HarmonyPatch(typeof(StageController), nameof(StageController.InitStage))]
        [HarmonyPostfix]
        private static void PostInitStage(StageController __instance, bool isCleared, bool isSandbox)
        {
            Log.LogInfo("Post-InitStage " + isCleared + " " + isSandbox);
        }
        
    }
}