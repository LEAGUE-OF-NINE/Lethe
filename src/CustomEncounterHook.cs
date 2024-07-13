using System;
using System.Linq;
using BattleUI.Operation;
using BepInEx.Logging;
using Dungeon;
using HarmonyLib;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.IO;
using Il2CppSystem.Runtime.Serialization.Formatters.Binary;
using MainUI;

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

        [HarmonyPatch(typeof(BattleObjectManager), nameof(BattleObjectManager.CreateAllyUnits),
            new[] { typeof(PlayerUnitFormation), typeof(int), typeof(int) })]

        [HarmonyPostfix]
        private static void CreateAllyUnits(BattleObjectManager __instance, PlayerUnitFormation formation,
            int startIndex, int participantNum)
        {
            int order = formation.GetParticipants().Count;

            foreach (var assistantStaticData in Singleton<StaticDataManager>.Instance.AssistantUnitList.GetList())
            {
                UnitDataModel_Assistant model = new UnitDataModel_Assistant(assistantStaticData);
                model._passiveList ??= new List<PassiveModel>();
                model._addCondtionList ??= new List<EachMentalConditionInfo>();
                model._associationList ??= new List<UNIT_KEYWORD>();
                model._skillList ??= new List<SkillModel>();
                model._minCondtionList ??= new List<EachMentalConditionInfo>();
                model._supporterPassiveList ??= new List<SupporterPassiveModel>();
                model._unitAttributeList ??= new List<UnitAttribute>();
                model._unitKeywordList ??= new List<UNIT_KEYWORD>();
                model._egoSkillModelList ??= new List<SkillModel>();
                model._slotWeightConditionList ??= new List<string>();
                model._defenseSkillIDList ??= new List<int>();
                BattleUnitModel_Player player = new BattleUnitModel_Player(
                    model, assistantStaticData.ID, order++, order);
                
                // needed for some reason
                
                // player._actionSlotDetail._skillDictionary.ad
                __instance.AddUnit(player, 45, 4);
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

        private static string attributeName(ATTRIBUTE_TYPE attr)
        {
            switch (attr)
            {
                case ATTRIBUTE_TYPE.CRIMSON:
                    return "CRIMSON";
                case ATTRIBUTE_TYPE.SCARLET:
                    return "SCARLET";
                case ATTRIBUTE_TYPE.AMBER:
                    return "AMBER";
                case ATTRIBUTE_TYPE.SHAMROCK:
                    return "SHAMROCK";
                case ATTRIBUTE_TYPE.AZURE:
                    return "AZURE";
                case ATTRIBUTE_TYPE.INDIGO:
                    return "INDIGO";
                case ATTRIBUTE_TYPE.VIOLET:
                    return "VIOLET";
                case ATTRIBUTE_TYPE.WHITE:
                    return "WHITE";
                case ATTRIBUTE_TYPE.BLACK:
                    return "BLACK";
                case ATTRIBUTE_TYPE.SPECIAL_RED:
                    return "SPECIAL RED";
                case ATTRIBUTE_TYPE.SPECIAL_PALE:
                    return "SPECIAL PALE";
                case ATTRIBUTE_TYPE.NEUTRAL:
                    return "NEUTRAL";
                case ATTRIBUTE_TYPE.NONE:
                    return "NONE";
                default:
                    return attr.ToString();
            }
        }

        [HarmonyPatch(typeof(ActionSlotDetail), nameof(ActionSlotDetail.SetSkillDictionary), new Type[]{  })]
        [HarmonyPostfix]
        private static void SetSkillDictionary(ActionSlotDetail __instance)
        {
            Log.LogInfo("===================================");
            Log.LogInfo("Owner: " + __instance._owner.InstanceID);
            Log.LogInfo("ATTRIBUTES: " + string.Join(", ", 
                __instance._owner._unitDataModel.ClassInfo.AttributeList.ToArray().Select(x => x.SkillId)));
            
            Log.LogInfo("SKILL POOL");
            foreach (var keyValuePair in __instance._owner.GetSkillPool())
            {
                Log.LogInfo("Key: " + keyValuePair.key + ", Value: " + keyValuePair.value);
            }
            
            Log.LogInfo("SKILL DICTIONARY");
            foreach (var keyValuePair in __instance._skillDictionary)
            {
                Log.LogInfo("Key: " + attributeName(keyValuePair.key) + ", Value: " + string.Join(", ", keyValuePair.value.ToArray().ToArray()));
            }
            Log.LogInfo("===================================");

            if (__instance._skillDictionary.Count == 0)
            {
                var attackSkills = __instance._owner._unitDataModel.ClassInfo.AttributeList;
                Log.LogInfo($"Unit {__instance._owner.InstanceID} has {attackSkills.Count} skills");
                foreach (var attribute in attackSkills)
                {
                    var attackSkill = attribute.GetSkill();
                    var defenseType = attackSkill.GetDefenseType(1);
                    if (defenseType != DEFENSE_TYPE.NONE && defenseType != DEFENSE_TYPE.ATTACK)
                        continue;
                    var sin = attackSkill.GetAttributeType(1);
                    if (!__instance._skillDictionary.TryGetValue(sin, out var sinSkills))
                        sinSkills = new List<int>();
                    sinSkills.Add(attackSkill.ID);
                    __instance._skillDictionary[sin] = sinSkills;
                }
            }
        }
    }
}