using System;
using System.Collections.Generic;
using System.Linq;
using Dungeon;
using Dungeon.Map;
using MainUI;

namespace CustomEncounter;

public static class EncounterHelper
{
    public static List<(string Name, List<EncounterData> Data)> EncounterLists { get; } = new();

    static EncounterHelper()
    {
        var uiList = TextDataManager.Instance.UIList;

        // Story Data
        foreach (var chapterData in StaticDataManager.Instance.partList.list)
        {
            var chapterName = TextDataManager.Instance.stagePart.GetData($"part_{chapterData.ID}").GetPartTitle();
            foreach (var subchapterData in chapterData.subchapters)
            {
                var subChapterName = TextDataManager.Instance.stageChapter
                    .GetData($"chapter_{subchapterData.Region}_{subchapterData.ID}")
                    .GetChapterTitle();
                var encounterData = new List<EncounterData>();
                var name = $"{uiList.GetText("STORY")} {chapterData.SprName} - {chapterName} - {subChapterName}";
                EncounterLists.Add(new(name, encounterData));
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
                        var dungeonEncounterList = new List<EncounterData>();
                        EncounterLists.Add(new($"{name} - {dungeonName}", dungeonEncounterList));
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
                                    dungeonEncounterList.Add(new()
                                    {
                                        Name = $"{title} #{i}",
                                        StageData = stage,
                                        StageType = STAGE_TYPE.NORMAL_BATTLE,
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        var stage = StaticDataManager.Instance.storyBattleStageList.GetStage(stageNodeInfo.StageId);
                        encounterData.Add(new()
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
        var expEncounters = new List<EncounterData>();
        EncounterLists.Add(new(uiList.GetText("enter_exp_dungeon"), expEncounters));
        for (var i = 0; i < expList.Count; i++)
        {
            var expData = expList[i];
            expEncounters.Add(new()
            {
                Name = string.Format(uiList.GetText("exp_dungeon_index"), i),
                StageData = expData,
                StageType = STAGE_TYPE.EXP_DUNGEON,
            });
        }

        // Thread Luxcavation
        var threadList = StaticDataManager.Instance.ThreadDungeonDataList.GetList();
        var threadEncounters = new List<EncounterData>();
        EncounterLists.Add(new(uiList.GetText("enter_thread_dungeon"), threadEncounters));
        foreach (var threadDungeonData in threadList)
        {
            var name = TextDataManager.Instance.ThreadDungeon.GetData(threadDungeonData.ID).GetName();
            foreach (var threadStage in threadDungeonData.SelectStage)
            {
                threadEncounters.Add(new()
                {
                    Name = $"{name} {uiList.GetText("recommended_level")} {threadStage.RecommendedLevel}",
                    StageData = StaticDataManager.Instance.ThreadDungeonBattleList.GetStage(threadStage.StageId),
                    StageType = STAGE_TYPE.THREAD_DUNGEON,
                });
            }
        }

        // Railway Lines
        var railwayList = StaticDataManager.Instance.RailwayDungeonDataList.GetList().ToArray();
        foreach (var railwayDungeonData in railwayList)
        {
            var data = (
                string.Format(uiList.GetText("mirror_refraction_railway_with_dungeon_name"),
                    TextDataManager.Instance.RailwayDungeonText.GetData(railwayDungeonData.ID).GetName()),
                new List<EncounterData>());
            foreach (var dungeonSector in railwayDungeonData.Sector)
            {
                data.Item2.Add(new()
                {
                    Name = TextDataManager.Instance.RailwayDungeonStationName.GetData(railwayDungeonData.ID)
                        .GetStationName(dungeonSector.NodeId),
                    StageData = StaticDataManager.Instance.GetDungeonStage(dungeonSector.StageId, default,
                        DUNGEON_TYPES.RAILWAY_DUNGEON),
                    StageType = STAGE_TYPE.RAILWAY_DUNGEON,
                });
            }

            EncounterLists.Add(data);
        }
    }

    public static string GetEncounterName(List<EncounterData> list, int index)
        => GetEncounterName(list[index]);

    public static string GetEncounterName(EncounterData data)
    {
        if (data.Name != null)
            return data.Name;

        var enemyList = StaticDataManager.Instance.EnemyUnitList;
        var abList = StaticDataManager.Instance.AbnormalityUnitList;
        var stage = data.StageData;
        var name = stage.EventScriptName;
        if (name == null && stage.stageScriptNameAfterClear is not "AbnormalityEvent" and not "Normal")
            name = stage.stageScriptNameAfterClear;
        if (name == null)
        {
            var unit = stage.GetAllEnemyDataByWave(stage.GetMaxWaveCount() - 1).ToArray().MaxBy(e =>
            {
                var id = e.GetID();
                var data = stage.StageType == STAGE_BATTLE_TYPE.Abnormality
                    ? abList.GetData(id).Cast<UnitStaticData>()
                    : enemyList.GetData(id).Cast<UnitStaticData>();
                return data.GetMaxHp(e.GetLevel());
            });
            name = TextDataManager.Instance.EnemyList.GetData(unit.GetID()).GetName();
        }

        return name;
    }

    public static void ExecuteEncounter(EncounterData encounter)
    {
        var gm = GlobalGameManager.Instance;
        gm.CurrentStage.SetNodeIDs(-1, -1, -1, -1);
        gm.CurrentStage.SetCurrentStageType(encounter.StageType);
        var formation = UserDataManager.Instance.Formations.GetCurrentFormation();
        var support = new SupportPersonality();
        var restrict = new RestrictParticipationData(new RestrictParticipationStaticData(), DUNGEON_TYPES.NONE);
        var unitFormation = new PlayerUnitFormation(formation, support, false, -1, restrict);
        Singleton<StageController>.Instance
            .InitStageModel(encounter.StageData, encounter.StageType, new(), false, unitFormation);
        gm.LoadScene(SCENE_STATE.Battle, (Action)(() => gm.StartStage()));
    }
}