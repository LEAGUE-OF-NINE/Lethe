using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppSystem.IO;
using MainUI;
using UnhollowerRuntimeLib;
using UnityEngine;
using Utils;

namespace Lethe.Patches
{
    public class Encounters : Il2CppSystem.Object
    {
        private static readonly Dictionary<int, string> StageIdToFolderPath = new();
        private static List<string> encounterPaths = new();

        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<Encounters>();
            harmony.PatchAll(typeof(Encounters));
        }

        private static void AddLegacyPlayground()
        {
            var subchapterJSON = @"{""id"":5858,""illustSprName"":"""",""sprName"":""chapter0"",""subChapterNumber"":""1"",""mapSizeRow"":0,""mapSizeColoumn"":0,""region"":""p"",""unlockCondition"":{""mainChapterId"":0,""subChapterId"":0,""nodeId"":0,""unlockCode"":101},""stageNodeList"":[{""nodeId"":696901,""stageId"":6006583,""posx"":0,""posy"":0,""nodeIllustIdString"":"""",""unlockCondition"":{""mainChapterId"":0,""subChapterId"":0,""nodeId"":0,""unlockCode"":101,""possession"":{""type"":"""",""id"":0,""num"":0,""tags"":[]}},""upper"":0,""stageNodeType"":""NORMAL""}],""nextMainchapterid"":101,""nextSubchapterid"":102,""lastnodeid"":0,""nodeIconPath"":""""}";
            var subchapterUIJSON = @"{""chapterId"":91,""subchapterId"":5858,""nodeId"":696901,""storyIdInTheaterData"":0,""isUnlockByUnlockCode"":false,""unlockCode"":101,""relatedData"":{""chapterId"":0,""subchapterId"":0,""nodeId"":0},""uiConfig"":{""customChapterText"":""1"",""chapterTagIconType"":""BATTLE"",""region"":""p"",""mapAreaId"":101,""timeLine"":""PLAYGROUND"",""illustId"":""story-id_E041X""},""type"":""STAGE_NODE""}";


            var subchapter = JsonUtility.FromJson<SubChapterData>(subchapterJSON);
            var subchapterUI = JsonUtility.FromJson<SubchapterUIDataInPart>(subchapterUIJSON);

            StaticDataManager.Instance._partList.GetPart(2).subchapterList.Insert(0, subchapter);
            StaticDataManager.Instance._partList.GetPart(2).subchapterUIList.Insert(0, subchapterUI);

            LetheHooks.LOG.LogWarning($"ADDED PLAYGROUND");
        }

        [HarmonyPatch(typeof(PartStaticDataList), nameof(PartStaticDataList.Init))]
        [HarmonyPostfix]
        private static void PostMainUILoad()
        {
            InitPartTwoData();
            AddLegacyPlayground();
            encounterPaths.Clear();
            StageIdToFolderPath.Clear();

            foreach (var modPath in Directory.GetDirectories(LetheMain.modsPath.FullPath))
            {
                var expectedPath = Path.Combine(modPath, "custom_encounters");
                if (!Directory.Exists(expectedPath)) continue;

                foreach (var folder in Directory.GetDirectories(expectedPath))
                {
                    encounterPaths.Add(folder);
                    IndexStageNodeIds(folder);
                }
            }

            // Adding the encounters dynamically
            AddEncounters();
        }
        private static void IndexStageNodeIds(string encounterFolder)
        {
            try
            {
                var subchapterJsonPath = Path.Combine(encounterFolder, "subchapterui.json");

                if (!File.Exists(subchapterJsonPath))
                {
                    LetheHooks.LOG.LogError($"Missing subchapterui.json in folder: {encounterFolder}");
                    return;
                }

                // Read and parse the subchapter JSON
                var subchapterJson = File.ReadAllText(subchapterJsonPath);
                var subchapterData = JsonUtility.FromJson<SubchapterUIDataInPart>(subchapterJson);

                // Index each stageNodeId to its folder path

                if (!StageIdToFolderPath.ContainsKey(subchapterData.nodeId))
                {
                    LetheHooks.LOG.LogWarning($"Adding index to stage id: {subchapterData.nodeId}");
                    StageIdToFolderPath[subchapterData.nodeId] = encounterFolder;
                }


                LetheHooks.LOG.LogWarning($"Indexed stageNodeIds for folder: {encounterFolder}");
            }
            catch (Exception e)
            {
                LetheHooks.LOG.LogError($"Failed to index stageNodeIds for encounter folder {encounterFolder}: {e.Message}");
            }
        }
        private static void AddEncounters()
        {
            foreach (var encounterFolder in encounterPaths)
            {
                try
                {
                    var encounterJsonPath = Path.Combine(encounterFolder, "encounter.json");
                    var subchapterUIJsonPath = Path.Combine(encounterFolder, "subchapterui.json");

                    if (!File.Exists(encounterJsonPath) || !File.Exists(subchapterUIJsonPath))
                    {
                        LetheHooks.LOG.LogError($"Missing required files in folder: {encounterFolder}");
                        continue;
                    }

                    // Read the JSON data
                    var encounterJson = File.ReadAllText(encounterJsonPath);
                    var subchapterUIJson = File.ReadAllText(subchapterUIJsonPath);

                    // Parse the JSON data
                    var encounterData = JsonUtility.FromJson<StageStaticData>(encounterJson);
                    var subchapterUIData = JsonUtility.FromJson<SubchapterUIDataInPart>(subchapterUIJson);
                    var subchapterData = GenerateSubchapterData(subchapterUIData);

                    // If the encounter data is valid, add it dynamically
                    if (encounterData != null && subchapterData != null && subchapterUIData != null)
                    {
                        encounterData.story.exit = null;

                        // Insert the parsed subchapter and subchapterUI at the start of the list
                        StaticDataManager.Instance._partList.GetPart(2).subchapterList.Insert(0, subchapterData);
                        StaticDataManager.Instance._partList.GetPart(2).subchapterUIList.Insert(0, subchapterUIData);

                        LetheHooks.LOG.LogWarning($"ADDED ENCOUNTER FROM {encounterFolder}");
                    }
                    else
                    {
                        LetheHooks.LOG.LogError($"Invalid data in encounter folder: {encounterFolder}");
                    }
                }
                catch (Exception e)
                {
                    LetheHooks.LOG.LogError($"Failed to load encounter from {encounterFolder}: {e.Message}");
                }
            }
        }

        private static void InitPartTwoData()
        {
            string jsonTemplate = @"
            {
                ""id"": 2,
                ""sprName"": ""Part1"",
                ""subchapterList"": [],
                ""subchapterUIList"": []
            }";
            PartStaticData newPartData = JsonUtility.FromJson<PartStaticData>(jsonTemplate);

            StaticDataManager.Instance._partList.list.Add(newPartData);
        }

        [HarmonyPatch(typeof(LobbyUIPresenter), nameof(LobbyUIPresenter.Initialize))]
        [HarmonyPostfix]
        private static void InitStageLocale()
        {
            TextData_StagePart text = new()
            {
                parttitle = "Lethe",
            };
            TextDataManager.Instance._stagePart._dic.AddOrReplace("part_2", text);

            TextData_StageText text2 = new()
            {
                title = "Playground"
            };
            TextDataManager.Instance._stageNodeText._dic.AddOrReplace("696901", text2);
        }
        private static SubChapterData GenerateSubchapterData(SubchapterUIDataInPart subchapterUIData)
        {
            string jsonTemplate = @"
            {
                ""id"": 0,
                ""illustSprName"": ""107"",
                ""sprName"": ""chapter7"",
                ""subChapterNumber"": ""1"",
                ""mapSizeRow"": 0,
                ""mapSizeColoumn"": 0,
                ""region"": ""p"",
                ""unlockCondition"": {
                    ""mainChapterId"": 0,
                    ""subChapterId"": 0,
                    ""nodeId"": 0,
                    ""unlockCode"": 100
                },
                ""stageNodeList"": [{
                    ""nodeId"": 0,
                    ""stageId"": 0,
                    ""posx"": 0,
                    ""posy"": 0,
                    ""nodeIllustIdString"": """",
                    ""unlockCondition"": {
                        ""mainChapterId"": 0,
                        ""subChapterId"": 0,
                        ""nodeId"": 0,
                        ""unlockCode"": 101,
                        ""possession"": { ""type"": """", ""id"": 0, ""num"": 0, ""tags"": [] }
                    },
                    ""upper"": 0,
                    ""stageNodeType"": ""NORMAL""
                }],
                ""nextMainchapterid"": 101,
                ""nextSubchapterid"": 102,
                ""lastnodeid"": 0,
                ""nodeIconPath"": """"
            }";

            var nodeId = subchapterUIData.nodeId;

            SubChapterData newSubchapterData = JsonUtility.FromJson<SubChapterData>(jsonTemplate);
            newSubchapterData.id = nodeId;
            var firstStageNode = newSubchapterData.stageNodeList.GetFirstElement();
            firstStageNode.nodeId = nodeId;
            firstStageNode.stageId = nodeId;

            return newSubchapterData;
        }

        [HarmonyPatch(typeof(StageStaticDataList), nameof(StageStaticDataList.GetStage))]
        [HarmonyPrefix]
        private static bool PreGetStage(ref int id, ref StageStaticData __result)
        {
            switch (id)
            {
                case 6006583:
                    var json = File.ReadAllText(LetheMain.EncounterConfig);
                    var read = JsonUtility.FromJson<StageStaticData>(json);
                    read.story.exit = null;
                    __result = read;
                    return false;
                case -1:
                    id = 1;
                    break;
            }

            if (StageIdToFolderPath.TryGetValue(id, out var encounterFolder))
            {
                try
                {
                    var encounterJsonPath = Path.Combine(encounterFolder, "encounter.json");
                    if (!File.Exists(encounterJsonPath))
                    {
                        LetheHooks.LOG.LogError($"Missing encounter.json in folder: {encounterFolder}");
                        return true;
                    }

                    // Read and parse the encounter JSON data
                    var encounterJson = File.ReadAllText(encounterJsonPath);
                    var encounterData = JsonUtility.FromJson<StageStaticData>(encounterJson);

                    // If valid encounter data is found, load it into the result
                    if (encounterData != null)
                    {
                        encounterData.story.exit = null;
                        __result = encounterData; // Set the result to the loaded data

                        LetheHooks.LOG.LogWarning($"Loaded encounter data for stage {id} from {encounterFolder}");
                        return false; // Skip the original method and return the custom data
                    }
                    else
                    {
                        LetheHooks.LOG.LogError($"Invalid encounter data in folder for stage {id}: {encounterFolder}");
                    }
                }
                catch (Exception e)
                {
                    LetheHooks.LOG.LogError($"Failed to load encounter for stage {id} from folder {encounterFolder}: {e.Message}");
                }
            }

            return true;
        }
    }
}