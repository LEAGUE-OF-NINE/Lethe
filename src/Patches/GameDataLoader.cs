using HarmonyLib;
using UnhollowerRuntimeLib;
using MainUI;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Addressable;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System.Collections;
using BepInEx;
using UnityEngine.ProBuilder;

//dump static data & locale
//load custom static data & locale
namespace LimbusSandbox.Patches
{
    internal class GameDataLoader : MonoBehaviour
    {
        public static bool GotVanillaData = false;
        public static Dictionary<string, Il2CppSystem.Collections.Generic.List<JSONNode>> vanillaDataClass = new();

        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<GameDataLoader>();
            harmony.PatchAll(typeof(GameDataLoader));
        }

        [HarmonyPatch(typeof(LobbyUIPresenter), nameof(LobbyUIPresenter.Initialize))]
        [HarmonyPostfix]
        private static void PostMainUILoad()
        {
            if (GotVanillaData == false)
            {
                SaveLocale();
            }
            
            GotVanillaData = true;
            LoadCustomLocale(TextDataManager.Instance, GlobalGameManager.Instance.Lang);
        }

        //save static data
        //load custom static data
        [HarmonyPatch(typeof(StaticDataManager), nameof(StaticDataManager.LoadStaticDataFromJsonFile))]
        [HarmonyPrefix]
        private static void LoadStaticData(string dataClass, ref Il2CppSystem.Collections.Generic.List<JSONNode> nodeList)
        {
            var newDirectory = Directory.CreateDirectory(Path.Combine(Plugin.vanillaDataPath, "StaticData", dataClass));
            var newCustomDirectory = Directory.CreateDirectory(Path.Combine(Plugin.customDataPath, "StaticData", dataClass));
            if (GotVanillaData == false)
            {
                vanillaDataClass[dataClass] = nodeList;
                //dump vanilla data
                SandboxLoader.Log.LogWarning($"DUMPING {dataClass}");
                var count = 0;
                foreach (var node in nodeList)
                {
                    File.WriteAllText(Path.Combine(newDirectory.FullName, $"{count}.json"), node.ToString(2));
                    count++;
                }
            }

            //load custom data
            foreach (var customData in Directory.GetFiles(newCustomDirectory.FullName))
            {
                Plugin.Log.LogWarning($"LOADING FILE {dataClass}/{customData.Substring(newCustomDirectory.FullName.Length + 1)}");
                try
                {
                    var node = JSONNode.Parse(File.ReadAllText(customData));
                    nodeList.Insert(0, node);
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"ERROR!!! {ex.Message}");
                }
            }

            //test
            foreach (var _path in Directory.GetDirectories(Plugin.__testpath))
            {
                var skibiddy = Path.Combine(_path, "StaticData", dataClass);
                if (Directory.Exists(skibiddy))
                {
                    foreach (var customData in Directory.GetFiles(skibiddy))
                    {
                        Plugin.Log.LogWarning($"LOADING FILE mods/{_path.Substring(Plugin.__testpath.Length + 1)}/StaticData/{dataClass}/{customData.Substring(skibiddy.Length + 1)}");
                        try
                        {
                            var node = JSONNode.Parse(File.ReadAllText(customData));
                            nodeList.Insert(0, node);
                        }
                        catch (Exception ex)
                        {
                            Plugin.Log.LogError($"ERROR!!! {ex.Message}");
                        }
                    }
                }
                else continue;
            }
        }

        private static void DumpLocale<T>(DirectoryInfo root, string name, JsonDataList<T> list)
            where T : LocalizeTextData, new()
        {
            var obj = new JSONObject();
            foreach (var keyValuePair in list._dic)
                obj[keyValuePair.key] = JSONNode.Parse(JsonUtility.ToJson(keyValuePair.value));

            File.WriteAllText(Path.Combine(root.FullName, $"{name}.json"), obj.ToString(2));
        }

        private static void SaveLocale()
        {
            var textManager = Singleton<TextDataManager>.Instance;
            var lang = GlobalGameManager.Instance.Lang;
            Plugin.Log.LogInfo("Dumping locale data: " + lang);
            var root = Directory.CreateDirectory(Path.Combine(Plugin.vanillaDataPath, "Locale", lang.ToString()));
            Directory.CreateDirectory(Path.Combine(Plugin.customDataPath, "Locale", lang.ToString()));
            DumpLocale(root, "uiList", textManager._uiList);
            DumpLocale(root, "characterList", textManager._characterList);
            DumpLocale(root, "personalityList", textManager._personalityList);
            DumpLocale(root, "enemyList", textManager._enemyList);
            DumpLocale(root, "egoList", textManager._egoList);
            DumpLocale(root, "skillList", textManager._skillList);
            DumpLocale(root, "passiveList", textManager._passiveList);
            DumpLocale(root, "bufList", textManager._bufList);
            DumpLocale(root, "itemList", textManager._itemList);
            DumpLocale(root, "keywordList", textManager._keywordList);
            DumpLocale(root, "skillTagList", textManager._skillTagList);
            DumpLocale(root, "abnormalityEventList", textManager._abnormalityEventList);
            DumpLocale(root, "attributeList", textManager._attributeList);
            DumpLocale(root, "abnormalityCotentData", textManager._abnormalityCotentData);
            DumpLocale(root, "keywordDictionary", textManager._keywordDictionary);
            DumpLocale(root, "actionEvents", textManager._actionEvents);
            DumpLocale(root, "egoGiftData", textManager._egoGiftData);
            DumpLocale(root, "stageChapter", textManager._stageChapter);
            DumpLocale(root, "stagePart", textManager._stagePart);
            DumpLocale(root, "stageNodeText", textManager._stageNodeText);
            DumpLocale(root, "dungeonNodeText", textManager._dungeonNodeText);
            DumpLocale(root, "storyDungeonNodeText", textManager._storyDungeonNodeText);
            DumpLocale(root, "quest", textManager._quest);
            DumpLocale(root, "dungeonArea", textManager._dungeonArea);
            DumpLocale(root, "battlePass", textManager._battlePass);
            DumpLocale(root, "storyTheater", textManager._storyTheater);
            DumpLocale(root, "announcer", textManager._announcer);
            DumpLocale(root, "normalBattleResultHint", textManager._normalBattleResultHint);
            DumpLocale(root, "abBattleResultHint", textManager._abBattleResultHint);
            DumpLocale(root, "tutorialDesc", textManager._tutorialDesc);
            DumpLocale(root, "iapProductText", textManager._iapProductText);
            DumpLocale(root, "illustGetConditionText", textManager._illustGetConditionText);
            DumpLocale(root, "choiceEventResultDesc", textManager._choiceEventResultDesc);
            DumpLocale(root, "battlePassMission", textManager._battlePassMission);
            DumpLocale(root, "gachaTitle", textManager._gachaTitle);
            DumpLocale(root, "introduceCharacter", textManager._introduceCharacter);
            DumpLocale(root, "userBanner", textManager._userBanner);
        }

        public static void LoadCustomLocale<T>(DirectoryInfo root, string name, JsonDataList<T> list)
    where T : LocalizeTextData, new()
        {
            Plugin.Log.LogInfo("Checking for custom locale: " + name);
            root = Directory.CreateDirectory(Path.Combine(root.FullName, name));
            foreach (var file in Directory.GetFiles(root.FullName, "*.json"))
            {
                var localeJson = JSONNode.Parse(File.ReadAllText(file));
                Plugin.Log.LogInfo("Loading custom locale: " + file);
                foreach (var keyValuePair in localeJson)
                {
                    var valueJson = keyValuePair.value.ToString(2);

                    try
                    {
                        var value = JsonUtility.FromJson<T>(valueJson);
                        if (value == null) throw new NullReferenceException("json parse result is null");
                        list._dic[keyValuePair.key] = value;
                        Plugin.Log.LogInfo("Loaded custom locale for " + keyValuePair.key);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogError("Cannot load custom locale for " + keyValuePair.key + ", reason: " + ex);
                        Plugin.Log.LogError(valueJson);
                    }
                }
            }
        }

        public static void LoadCustomLocale(TextDataManager __instance, LOCALIZE_LANGUAGE lang)
        {
            var root = Directory.CreateDirectory(Path.Combine(Plugin.customDataPath, "Locale", lang.ToString()));
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
}

