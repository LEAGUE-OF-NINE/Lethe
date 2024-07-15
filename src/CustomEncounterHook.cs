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
                model._minCondtionList ??= new List<EachMentalConditionInfo>();
                model._supporterPassiveList ??= new List<SupporterPassiveModel>();
                model._unitAttributeList ??= new List<UnitAttribute>();
                model._unitKeywordList ??= new List<UNIT_KEYWORD>();
                model._egoSkillModelList ??= new List<SkillModel>();
                model._slotWeightConditionList ??= new List<string>();
                model._defenseSkillIDList ??= new List<int>();

                var skillList = new List<SkillModel>();
                foreach (var unitAttribute in model.ClassInfo.AttributeList)
                {
                    var skillModel = Singleton<StaticDataManager>.Instance.SkillList.GetData(unitAttribute.SkillId);
                    skillList.Add(new SkillModel(skillModel, 45, 1));
                    Log.LogInfo($"Registering skill {unitAttribute.SkillId}");
                }
                model._skillList ??= skillList;
                
                order++;
                Log.LogInfo($"Adding assistant at {order} Owner: {assistantStaticData.ID}");
                BattleUnitModel_Player player = new BattleUnitModel_Player(
                    model, order, order, order);
                
                // add erosion data
                var erosionData = new BattleUnitModel.EgoAndErosionState();
                erosionData._egoList = new List<BattleEgoModel>();
                var baseEgo = Singleton<StaticDataManager>.Instance.EgoList.GetData(20101);
                erosionData._egoList.Add(new BattleEgoModel(UNIT_FACTION.PLAYER, baseEgo, 4));
                player._erosionData ??= erosionData;
                
                // player._actionSlotDetail._skillDictionary.ad
                __instance.AddUnit(player, 45, 4);
            }
        }

        [HarmonyPatch(typeof(ActionSlotDetail), nameof(ActionSlotDetail.SetSkillDictionary), new Type[]{  })]
        [HarmonyPostfix]
        private static void SetSkillDictionary(ActionSlotDetail __instance)
        {
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

            if (__instance._owner._unitDataModel._defenseSkillIDList.Count == 0)
            {
                __instance._owner._unitDataModel._defenseSkillIDList.Add(1100904);
            }
        }
    }
}