using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;

namespace CustomEncounter
{
    public class CustomEncounterHook : MonoBehaviour
    {
        public static CustomEncounterHook Instance;
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
            // if (Input.GetKeyDown(KeyCode.F10) && 
            //     StaticDataManager.Instance is {IsDataLoaded:true} && 
            //     TextDataManager.Instance is {IsLoadedRemote:true} &&
            //     !AddressableManager.Instance.IsLocalizeDataLoading())
            // {
            //     EncounterPanel.Instance ??= new EncounterPanel(Plugin.UiBase);
            //     EncounterPanel.IsShown = !EncounterPanel.IsShown;
            // }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                //var sd = Singleton<StaticDataManager>.Instance.GetDungeonStage(99999, true, DUNGEON_TYPES.STORY_DUNGEON);
                Log.LogInfo("Break 1");
                var sd = new StageStaticData()
                {
                    id = 99999,
                    _stageType = STAGE_BATTLE_TYPE.Abnormality,
                    stageLevel = 25,
                    recommendedLevel = 25,
                    dangerLevel = 1,
                    staminaCost = 0,
                    participantInfo = new ParticipantsInfo()
                    {
                        min = 5,
                        max = 5
                    },
                    waveList = new List<Wave>(),
                    turnLimit = 99,
                    rewardList = new List<Reward>(),
                    questlist = new List<QuestCondition>(),
                    stageScriptNameAfterClear = "Eunbong_Strong",
                    abnormalityEventList = new List<int>(),
                };
               
                Log.LogInfo("Break 2");

                var quest = new QuestCondition()
                {
                    localizeId = 1,
                    args = new List<string>()
                };
                quest.args.Add("10");
                sd.questlist.Add(quest);

                Log.LogInfo("Break 3");

                var firstWave = new Wave()
                {
                    battleMapInfo = new BattleMapInfo()
                    {
                        mapName = "LCorpElevatorBlue"
                    },
                    unitList = new List<EnemyData>(),
                    enemyPositionID = 28,
                    bgmList = new List<string>()
                };
                Log.LogInfo("Break 4");
                
                firstWave.bgmList.Add("Battle_Cp2_Ally_3");
                firstWave.bgmList.Add("Battle_Cp3_5_Enemy_2");
                firstWave.unitList.Add(new EnemyData()
                {
                    unitID = 8035,
                    unitCount = 1,
                    unitLevel = 25
                });
                firstWave.unitList.Add(new EnemyData()
                {
                    unitID = 8039,
                    unitCount = 1,
                    unitLevel = 25
                });
                sd.waveList.Add(firstWave);
               
                Log.LogInfo("Break 5");

                for (var i = 0; i < 13; i++)
                {
                    var wave = new Wave()
                    {
                        unitList = new List<EnemyData>(),
                        enemyPositionID = 28,
                    };
                    wave.unitList.Add(new EnemyData()
                    {
                        unitID = 8040,
                        unitCount = 1,
                        unitLevel = 25
                    });
                    wave.unitList.Add(new EnemyData()
                    {
                        unitID = 8035,
                        unitCount = 1,
                        unitLevel = 25
                    });
                    sd.waveList.Add(wave);
                }
                sd.SetEnemyData();

                Log.LogInfo("Break 6");
               
                EncounterHelper.ExecuteEncounter(new EncounterData()
                {
                    StageData = sd,
                    StageType = STAGE_TYPE.EXP_DUNGEON,
                });
               
                Log.LogInfo("Break 7");
            }
        }
        
        [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
        [HarmonyPostfix]
        private static void SetLoginInfo(LoginSceneManager __instance)
        {
            __instance.tmp_loginAccount.text = "CustomEncounter v" + CustomEncounterMod.VERSION;
        }
    }
}