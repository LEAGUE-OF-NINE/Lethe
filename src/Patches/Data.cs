using System;
using System.IO;
using System.Threading;
using BepInEx;
using HarmonyLib;
using LibCpp2IL;
using MainUI;
using SimpleJSON;
using UnhollowerRuntimeLib;
using UnityEngine;
using Server;

namespace Lethe.Patches;

public class Data : Il2CppSystem.Object
{
   
    private static bool _localizeDataLoaded;

    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Data>();
        harmony.PatchAll(typeof(Data));
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

        return true;
    }

    public static void LoadCustomLocale<T>(DirectoryInfo root, string name, JsonDataList<T> list)
        where T : LocalizeTextData, new()
    {
        LetheHooks.LOG.LogInfo("Checking for custom locale: " + name);
        Directory.CreateDirectory(Path.Combine(LetheMain.templatePath.FullPath, "custom_limbus_locale", GlobalGameManager.Instance.Lang.ToString(), name));
        root = Directory.CreateDirectory(Path.Combine(root.FullName, "custom_limbus_locale", GlobalGameManager.Instance.Lang.ToString(), name));
        foreach (var file in Directory.GetFiles(root.FullName, "*.json", SearchOption.AllDirectories))
        {
            var localeJson = JSONNode.Parse(File.ReadAllText(file));
            LetheHooks.LOG.LogInfo("Loading custom locale: " + file);
            foreach (var keyValuePair in localeJson)
            {
                var valueJson = keyValuePair.value.ToString(2);

                try
                {
                    var value = JsonUtility.FromJson<T>(valueJson);
                    if (value == null) throw new NullReferenceException("json parse result is null");
                    list._dic[keyValuePair.key] = value;
                    LetheHooks.LOG.LogInfo("Loaded custom locale for " + keyValuePair.key);
                }
                catch (Exception ex)
                {
                    LetheHooks.LOG.LogError("Cannot load custom locale for " + keyValuePair.key + ", reason: " + ex);
                    LetheHooks.LOG.LogError(valueJson);
                }
            }
        }
    }

    public static void LoadCustomLocale(TextDataManager __instance, LOCALIZE_LANGUAGE lang)
    {
        var __lang = lang.ToString();
        foreach (var modPath in Directory.GetDirectories(LetheMain.modsPath.FullPath))
        {
            var expected = Path.Combine(modPath, "custom_limbus_locale", __lang);
            if (!Directory.Exists(expected)) continue;
            var root = Directory.CreateDirectory(modPath);
            LoadCustomLocale(root, "uiList", __instance._uiList);
            LoadCustomLocale(root, "characterList", __instance._characterList);
            LoadCustomLocale(root, "personalityList", __instance._personalityList);
            LoadCustomLocale(root, "enemyList", __instance._enemyList);
            LoadCustomLocale(root, "egoList", __instance._egoList);
            LoadCustomLocale(root, "skillList", __instance._skillList);
            LoadCustomLocale(root, "passiveList", __instance._passiveList);
            LoadCustomLocale(root, "bufList", __instance._bufList);
            LoadCustomLocale(root, "itemList", __instance._itemList);
            LoadCustomLocale(root, "keywordList", __instance._keywordList);
            LoadCustomLocale(root, "skillTagList", __instance._skillTagList);
            LoadCustomLocale(root, "abnormalityEventList", __instance._abnormalityEventList);
            LoadCustomLocale(root, "attributeList", __instance._attributeList);
            LoadCustomLocale(root, "abnormalityCotentData", __instance._abnormalityCotentData);
            LoadCustomLocale(root, "keywordDictionary", __instance._keywordDictionary);
            LoadCustomLocale(root, "actionEvents", __instance._actionEvents);
            LoadCustomLocale(root, "egoGiftData", __instance._egoGiftData);
            LoadCustomLocale(root, "stageChapter", __instance._stageChapter);
            LoadCustomLocale(root, "stagePart", __instance._stagePart);
            LoadCustomLocale(root, "stageNodeText", __instance._stageNodeText);
            LoadCustomLocale(root, "dungeonNodeText", __instance._dungeonNodeText);
            LoadCustomLocale(root, "storyDungeonNodeText", __instance._storyDungeonNodeText);
            LoadCustomLocale(root, "quest", __instance._quest);
            LoadCustomLocale(root, "dungeonArea", __instance._dungeonArea);
            LoadCustomLocale(root, "battlePass", __instance._battlePass);
            LoadCustomLocale(root, "storyTheater", __instance._storyTheater);
            LoadCustomLocale(root, "announcer", __instance._announcer);
            LoadCustomLocale(root, "normalBattleResultHint", __instance._normalBattleResultHint);
            LoadCustomLocale(root, "abBattleResultHint", __instance._abBattleResultHint);
            LoadCustomLocale(root, "tutorialDesc", __instance._tutorialDesc);
            LoadCustomLocale(root, "iapProductText", __instance._iapProductText);
            LoadCustomLocale(root, "illustGetConditionText", __instance._illustGetConditionText);
            LoadCustomLocale(root, "choiceEventResultDesc", __instance._choiceEventResultDesc);
            LoadCustomLocale(root, "battlePassMission", __instance._battlePassMission);
            LoadCustomLocale(root, "gachaTitle", __instance._gachaTitle);
            LoadCustomLocale(root, "introduceCharacter", __instance._introduceCharacter);
            LoadCustomLocale(root, "userBanner", __instance._userBanner);
        }
    }

    [HarmonyPatch(typeof(LobbyUIPresenter), nameof(LobbyUIPresenter.Initialize))]
    [HarmonyPostfix]
    private static void PostMainUILoad()
    {
        if (_localizeDataLoaded) return;
        _localizeDataLoaded = true;
        LoadCustomLocale(Singleton<TextDataManager>.Instance, GlobalGameManager.Instance.Lang);
        LetheHooks.LOG.LogInfo($"Stopping HTTP listener");
        LetheHooks.StopHttp();
        LetheHooks.LOG.LogInfo($"Dumping static data");
        new Thread(StoreStaticData).Start();
    }

    private static void StoreStaticData()
    {
        foreach (var (dataClass, nodeList) in Login.StaticData)
        {
            LetheHooks.LOG.LogInfo($"Dumping {dataClass}");

            try
            {
                var root = Directory.CreateDirectory(Path.Combine(LetheMain.vanillaDumpPath.FullPath, "limbus_data", dataClass));
                var i = 0;
                foreach (var jsonNode in nodeList)
                {
                    File.WriteAllText(Path.Combine(root.FullName, $"{i}.json"), jsonNode);
                    i++;
                }
            }
            catch (Exception ex)
            {
                LetheHooks.LOG.LogError(ex);
            }
        }
        
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
        StaticDataManager.Instance._partList.GetPart(1).subchapterUIList.Insert(0,subchapterUI);
        
        LetheHooks.LOG.LogWarning($"ADDED PLAYGROUND");
    }    
}
