using System;
using BepInEx;
using BepInEx.Logging;
using Dungeon;
using Dungeon.Map;
using Il2CppSystem.IO;
using Il2CppSystem.Text.RegularExpressions;
using MainUI;
using UnityEngine;

namespace CustomEncounter;

public static class EncounterHelper
{
    internal static ManualLogSource Log;

    public static void SaveToFile(EncounterData data)
    {
        var path = Path.Combine(Paths.ConfigPath, "encounters", Regex.Replace(data.Name, @"\W", "") + ".json");
        Log.LogInfo("Saving encounter to " + path);
        try
        {
            var json = JsonUtility.ToJson(data.StageData, true);
            File.WriteAllText(path, json);
        }
        catch
        {
            Log.LogInfo("Failed saving " + path);
        }
    }
    
    public static void SaveEncounters()
    {
        var uiList = TextDataManager.Instance.UIList;
        Log.LogInfo("Dumping encounters to files..."); 
        Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "encounters"));

        // Story Data
        foreach (var chapterData in StaticDataManager.Instance.partList.list)
        {
            var chapterName = TextDataManager.Instance.stagePart.GetData($"part_{chapterData.ID}").GetPartTitle();
            foreach (var subchapterData in chapterData.subchapters)
            {
                var subChapterName = TextDataManager.Instance.stageChapter
                    .GetData($"chapter_{subchapterData.Region}_{subchapterData.ID}")
                    .GetChapterTitle();
                var name = $"{uiList.GetText("STORY")}-{chapterData.SprName}-{chapterName}-{subChapterName}";
                foreach (var stageNodeInfo in subchapterData.StageNodeList)
                {
                    var type = stageNodeInfo.GetStageNodeType();
                    if (type is STAGEMAP_STAGENODE_TYPE.STORY)
                        continue;

                    if (type is STAGEMAP_STAGENODE_TYPE.DUNGEON)
                    {
                        var dungeonData =
                            StaticDataManager.Instance.DungeonMapList.GetDungeonData(stageNodeInfo.StageId);
                        if (dungeonData == null)
                            continue;

                        var dungeonName = TextDataManager.Instance.StageNodeText.GetData(stageNodeInfo.NodeId)
                            .GetTitle();
                        foreach (var floor in dungeonData.Floors)
                        {
                            foreach (var sector in floor.sectors)
                            {
                                for (var i = 0; i < sector.Nodes.Count; i++)
                                {
                                    var node = sector.Nodes[i];
                                    if (node == null || node.EncounterType is
                                            ENCOUNTER.START or
                                            ENCOUNTER.SAVE or
                                            ENCOUNTER.EVENT)
                                        continue;

                                    StageStaticData stage = null;
                                    switch (node.EncounterType)
                                    {
                                        case ENCOUNTER.BATTLE:
                                        case ENCOUNTER.HARD_BATTLE:
                                            stage = StaticDataManager.Instance.dungeonBattleStageList.GetStage(
                                                node.EncounterId);
                                            break;
                                        case ENCOUNTER.AB_BATTLE:
                                        case ENCOUNTER.BOSS:
                                            stage = StaticDataManager.Instance.abBattleStageList.GetStage(
                                                node.EncounterId);
                                            break;
                                    }

                                    var title = TextDataManager.Instance.StoryDungeonNodeText
                                        .GetData(stageNodeInfo.StageId)?.GetStageText(node.ID)?.GetTitle();
                                    var encounter = new EncounterData()
                                    {
                                        Name = $"{name}-{dungeonName}-{title}-#{i}",
                                        StageData = stage,
                                        StageType = STAGE_TYPE.NORMAL_BATTLE,
                                    };
                                    SaveToFile(encounter);
                                }
                            }
                        }
                    }
                    else
                    {
                        var stage = StaticDataManager.Instance.storyBattleStageList.GetStage(stageNodeInfo.StageId);
                        SaveToFile(new()
                        {
                            Name = TextDataManager.Instance.StageNodeText.GetData(stageNodeInfo.NodeId).GetTitle(),
                            StageData = stage,
                            StageType = STAGE_TYPE.NORMAL_BATTLE,
                        });
                    }
                }
            }
        }

        // EXP Luxcavation
        var expList = StaticDataManager.Instance.ExpDungeonBattleList.GetList().ToArray();
        for (var i = 0; i < expList.Count; i++)
        {
            var expData = expList[i];
            if (i == 0)
            {
                expData = StaticDataManager.Instance.ExpDungeonBattleList.GetStage(-1);
            }
            SaveToFile(new()
            {
                Name = string.Format(uiList.GetText("exp_dungeon_index"), i),
                StageData = expData,
                StageType = STAGE_TYPE.EXP_DUNGEON,
            });
        }

        // Thread Luxcavation
        var threadList = StaticDataManager.Instance.ThreadDungeonDataList.GetList();
        foreach (var threadDungeonData in threadList)
        {
            var name = TextDataManager.Instance.ThreadDungeon.GetData(threadDungeonData.ID).GetName();
            foreach (var threadStage in threadDungeonData.SelectStage)
            {
                SaveToFile(new()
                {
                    Name = $"{name}-{uiList.GetText("recommended_level")}-{threadStage.RecommendedLevel}",
                    StageData = StaticDataManager.Instance.ThreadDungeonBattleList.GetStage(threadStage.StageId),
                    StageType = STAGE_TYPE.THREAD_DUNGEON,
                });
            }
        }

        // Railway Lines
        var railwayList = StaticDataManager.Instance.RailwayDungeonDataList;
        foreach (var railwayData in railwayList.GetList())
        {
            var name = TextDataManager.Instance.RailwayDungeonText.GetData(railwayData.ID).GetName();
            foreach (var railwayStage in railwayData.Sector)
            {
                SaveToFile(new()
                {
                    Name = $"{name}-{railwayStage.nodeId}-{railwayStage.stageId}",
                    StageData = StaticDataManager.Instance.GetDungeonStage(railwayStage.stageId, default, DUNGEON_TYPES.RAILWAY_DUNGEON),
                    StageType = STAGE_TYPE.RAILWAY_DUNGEON,
                });
            }
        }
    }
}