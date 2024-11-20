using HarmonyLib;
using UnhollowerRuntimeLib;
using Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LimbusSandbox.Patches
{
    internal class PlaygroundStage : MonoBehaviour
    {
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<PlaygroundStage>();
            harmony.PatchAll(typeof(PlaygroundStage));
        }

        [HarmonyPatch(typeof(StageStaticDataList), nameof(StageStaticDataList.GetStage))]
        [HarmonyPrefix]
        private static bool PreGetStage(ref int id, ref StageStaticData __result)
        {
            switch (id)
            {
                case 6006583:
                    var json = File.ReadAllText(Path.Combine(Plugin.pluginPath, "encounter.json"));                   
                    var read = JsonUtility.FromJson<StageStaticData>(json);
                    //read.id = 10101;
                    //read.story.exit = null;
                    __result = read;
                    return false;
                case -1:
                    id = 1;
                    break;
            }

            return true;
        }

        [HarmonyPatch(typeof(StageController), nameof(StageController.StartStage))]
        [HarmonyPostfix]
        private static void StageChangeNodeID()
        {
            var stageInfo = GlobalGameManager.Instance.CurrentStage.NormalNodeInfo;
            stageInfo.currentChapterID = 101;
            stageInfo.currentPartID = 1;
            if (StaticDataManager.Instance.GetStage(stageInfo.currentStageID) == null) stageInfo.currentStageID = 10106; 
            else stageInfo.currentStageID = StageController.Instance._stageModel._classInfo.id;
            stageInfo.currentStageClearState = CLEARNODE_STATE.NOTCLEAR;
        }

        [HarmonyPatch(typeof(UserStageNodeStateData), nameof(UserStageNodeStateData.GetClearNodeState))]
        [HarmonyPatch(new[] { typeof(int), typeof(int), typeof(int) })]
        [HarmonyPrefix]
        private static void GetClearNodeState(ref CLEARNODE_STATE __result, int mainchapterid, int subchaperid, int nodeid)
        {
            if (nodeid == 696901) __result = CLEARNODE_STATE.NOTCLEAR;
        }

        [HarmonyPatch(typeof(PartStaticDataList), nameof(PartStaticDataList.Init))]
        [HarmonyPostfix]
        public static void AddPlayground()
        {
            //damn i dont care anymore
            var subchapterJSON = @"{""id"":6969,""illustSprName"":"""",""sprName"":""chapter0"",""subChapterNumber"":""1"",""mapSizeRow"":0,""mapSizeColoumn"":0,""region"":""p"",""unlockCondition"":{""mainChapterId"":0,""subChapterId"":0,""nodeId"":0,""unlockCode"":101},""stageNodeList"":[{""nodeId"":696901,""stageId"":6006583,""posx"":0,""posy"":0,""nodeIllustIdString"":"""",""unlockCondition"":{""mainChapterId"":0,""subChapterId"":0,""nodeId"":0,""unlockCode"":101,""possession"":{""type"":"""",""id"":0,""num"":0,""tags"":[]}},""upper"":0,""stageNodeType"":""NORMAL""}],""nextMainchapterid"":101,""nextSubchapterid"":102,""lastnodeid"":0,""nodeIconPath"":""""}";
            var subchapterUIJSON = @"{""chapterId"":91,""subchapterId"":6969,""nodeId"":696901,""storyIdInTheaterData"":0,""isUnlockByUnlockCode"":false,""unlockCode"":101,""relatedData"":{""chapterId"":0,""subchapterId"":0,""nodeId"":0},""uiConfig"":{""customChapterText"":""1"",""chapterTagIconType"":""BATTLE"",""region"":""p"",""mapAreaId"":101,""timeLine"":""PLAYGROUND"",""illustId"":""""},""type"":""STAGE_NODE""}";


            var subchapter = JsonUtility.FromJson<SubChapterData>(subchapterJSON);
            var subchapterUI = JsonUtility.FromJson<SubchapterUIDataInPart>(subchapterUIJSON);

            StaticDataManager.Instance._partList.GetPart(1).subchapterList.Insert(0, subchapter);
            StaticDataManager.Instance._partList.GetPart(1).subchapterUIList.Insert(0, subchapterUI);
        }
    }
}
