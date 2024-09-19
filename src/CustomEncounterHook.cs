using System;
using System.Collections.Generic;
using Addressable;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem.IO;
using SD;
using Server;
using SimpleJSON;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CustomEncounter;

public class CustomEncounterHook : MonoBehaviour
{
    private static StageStaticData _encounter;
    private static ManualLogSource _log;

    private static DirectoryInfo _customAppearanceDir, _customSpriteDir, _customLocaleDir;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            EncounterHelper.SaveLocale();
            EncounterHelper.SaveEncounters();
            EncounterHelper.SaveIdentities();
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            _log.LogInfo("Entering custom fight");
            try
            {
                var json = File.ReadAllText(CustomEncounterMod.EncounterConfig);
                _log.LogInfo("Fight data:\n" + json);
                _encounter = JsonUtility.FromJson<StageStaticData>(json);
                _log.LogInfo("Success, please go to excavation 1 to start the fight.");
            }
            catch (Exception ex)
            {
                _log.LogError("Error loading custom fight: " + ex.Message);
            }
        }
    }

    internal static void Setup(ManualLogSource log)
    {
        ClassInjector.RegisterTypeInIl2Cpp<CustomEncounterHook>();
        _log = log;

        GameObject obj = new("carra.customencounter.bootstrap");
        DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.HideAndDontSave;
        
        _customAppearanceDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_appearance"));
        _customSpriteDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_sprites"));
        _customLocaleDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_limbus_locale"));
    }

    [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
    [HarmonyPostfix]
    private static void SetLoginInfo(LoginSceneManager __instance)
    {
        __instance.tmp_loginAccount.text = "CustomEncounter v" + CustomEncounterMod.VERSION;
    }

    [HarmonyPatch(typeof(StaticDataManager), nameof(StaticDataManager.LoadStaticDataFromJsonFile))]
    [HarmonyPrefix]
    private static void LoadStaticDataFromJsonFile(StaticDataManager __instance, string dataClass,
        ref Il2CppSystem.Collections.Generic.List<JSONNode> nodeList)
    {
        _log.LogInfo($"Dumping {dataClass}");
        var root = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "limbus_data", dataClass));
        var i = 0;
        foreach (var jsonNode in nodeList)
        {
            File.WriteAllText(Path.Combine(root.FullName, $"{i}.json"), jsonNode.ToString(2));
            i++;
        }

        root = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_limbus_data", dataClass));
        foreach (var file in Directory.GetFiles(root.FullName, "*.json"))
            try
            {
                _log.LogInfo($"Loading {file}");
                nodeList.Add(JSONNode.Parse(File.ReadAllText(file)));
            }
            catch (Exception ex)
            {
                _log.LogError($"Error parsing {file}: {ex.GetType()} {ex.Message}");
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
            case 1 when _encounter != null:
                __result = _encounter;
                return false;
            case -1:
                id = 1;
                break;
        }

        return true;
    }

    [HarmonyPatch(typeof(AbnormalityAppearance_Cromer1p), nameof(AbnormalityAppearance_Cromer1p.OnEndBehaviour))]
    [HarmonyPrefix]
    private static void KromerOnEndBehavior(AbnormalityAppearance_Cromer1p __instance)
    {
        // stub, to catch errors
    }
    
    [HarmonyPatch(typeof(DamageStatistics), nameof(DamageStatistics.SetResult))]
    [HarmonyPrefix]
    private static void DamageStatisticsSetResult(DamageStatistics __instance)
    {
        // stub, to catch errors
    }

    [HarmonyPatch(typeof(BuffAbility_EgoAwakenDongrangTree),
        nameof(BuffAbility_EgoAwakenDongrangTree.OnRoundStart_After_Event))]
    [HarmonyPrefix]
    private static void BuffAbility_EgoAwakenDongrangTreeOnRoundStart_After_Event(
        BuffAbility_EgoAwakenDongrangTree __instance)
    {
        // stub, to catch errors
    }

    public static bool CreateSkinForModel(BattleUnitView view, BattleUnitModel unit, Transform parent, ref CharacterAppearance __result)
    {
        return CreateSkin(view, unit.GetAppearanceID(), parent, ref __result);
    }

    public static bool CreateSkin(BattleUnitView view, string appearanceID, Transform parent, ref CharacterAppearance __result)
    {
        var label = "";
        _log.LogInfo($"Loading asset {label}: {appearanceID}");

        var prefix = "!custom_";
        if (appearanceID.StartsWith(prefix))
        {
            appearanceID = appearanceID.Substring(prefix.Length);
            var prefabPath = "";
            if (appearanceID.Contains("/"))
            {
                var appearancePath = appearanceID.Split("/".ToCharArray(), 2);
                appearanceID = appearancePath[0];
                prefabPath = appearancePath[1];
            }
            
            var path = Path.Combine(_customAppearanceDir.FullName, appearanceID + ".bundle");
            if (!File.Exists(path))
            {
                _log.LogError("Cannot find asset bundle at: " + path);
                return true;
            }

            var bundle = AssetBundle.LoadFromFile(path);
            try
            {
                if (prefabPath == "")
                {
                    foreach (var assetName in bundle.AllAssetNames())
                    {
                        _log.LogInfo("Found asset: " + assetName);
                    }

                    prefabPath = bundle.AllAssetNames()[0];
                }
               
                _log.LogInfo("Using prefab: " + prefabPath);
                var asset = bundle.LoadAsset(prefabPath);
                var gameObject = Instantiate(asset, parent.position, parent.rotation, parent).Cast<GameObject>();
                var appearance = gameObject.GetComponent<CharacterAppearance>();
                if (appearance != null)
                {
                    appearance.charInfo.appearanceID = appearanceID;
                    appearance.Initialize(view);
                    __result = appearance;
                    return false;
                }
                
                _log.LogError(appearanceID + ": appearance is null!?");
            }
            finally
            {
                bundle.Unload(false);
            }
        }
        
        prefix = "!abno_";
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
        
        prefix = "!identity_";
        if (appearanceID.StartsWith(prefix))
        {
            label = SDCharacterSkinUtil._LABEL_PERSONALITY;
            appearanceID = appearanceID.Substring(prefix.Length);
        }

        if (label == "") return true;

        var res = AddressableManager.Instance.LoadAssetSync<GameObject>(label, appearanceID, parent);
        if (res == null) return true;

        var skin = res.Item1.GetComponent<CharacterAppearance>();
        if (skin == null) return true;
        skin.Initialize(view);
        skin.charInfo.appearanceID = appearanceID;
        __result = skin;
        return false;
    }

    public static void LoadCustomLocale<T>(DirectoryInfo root, string name, JsonDataList<T> list) where T : LocalizeTextData, new()
    {
        _log.LogInfo("Checking for custom locale: " + name);
        root = Directory.CreateDirectory(Path.Combine(root.FullName, name));
        foreach (var file in Directory.GetFiles(root.FullName, "*.json"))
        {
            var localeJson = JSONNode.Parse(File.ReadAllText(file));
            _log.LogInfo("Loading custom locale: " + file);
            foreach (var keyValuePair in localeJson)
            {
                var valueJson = keyValuePair.value.ToString(2);
                
                try
                {
                    var value = JsonUtility.FromJson<T>(valueJson);
                    if (value == null)
                    {
                        throw new NullReferenceException("json parse result is null");
                    }
                    list._dic[keyValuePair.key] = value;
                    _log.LogInfo("Loaded custom locale for " + keyValuePair.key);
                }
                catch (Exception ex)
                {
                    _log.LogError("Cannot load custom locale for " + keyValuePair.key + ", reason: " + ex);
                    _log.LogError(valueJson);
                }
            }
        }
    }

    private static void LoadCustomLocale(TextDataManager __instance, LOCALIZE_LANGUAGE lang)
    {
        var root = Directory.CreateDirectory(Path.Combine(_customLocaleDir.FullName, lang.ToString()));
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

    private static bool _localizeDataLoaded;
    
    [HarmonyPatch(typeof(MainUI.LobbyUIPresenter), nameof(MainUI.LobbyUIPresenter.Initialize))]
    [HarmonyPostfix]
    private static void PostMainUILoad()
    {
        if (!_localizeDataLoaded)
        {
            LoadCustomLocale(Singleton<TextDataManager>.Instance, GlobalGameManager.Instance.Lang);
            _localizeDataLoaded = true;
        }
    }

    [HarmonyPatch(typeof(StageController), nameof(StageController.InitStage))]
    [HarmonyPrefix]
    private static void PreInitStage(StageController __instance, bool isCleared, bool isSandbox)
    {
        _log.LogInfo("Pre-InitStage " + isCleared + " " + isSandbox);
    }

    [HarmonyPatch(typeof(StageController), nameof(StageController.InitStage))]
    [HarmonyPostfix]
    private static void PostInitStage(StageController __instance, bool isCleared, bool isSandbox)
    {
        _log.LogInfo("Post-InitStage " + isCleared + " " + isSandbox);
    }

    private static readonly Dictionary<string, Sprite> Sprites = new();
    private static readonly Dictionary<string, bool> SpriteExist = new();

    private static Sprite LoadSpriteFromFile(string fileName)
    {
        if (SpriteExist.TryGetValue(fileName, out var spriteExists) && !spriteExists)
            return null;
        
        if (Sprites.TryGetValue(fileName, out var sprite) && sprite != null)
            return sprite;
        
        try
        {
            var path = Path.Combine(_customSpriteDir.FullName, fileName + ".png");
            if (!File.Exists(path))
            {
                SpriteExist[fileName] = false;
                return null;
            }
           
            SpriteExist[fileName] = true;
            var data = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2);
            ImageConversion.LoadImage(tex, data);
            sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            Sprites[fileName] = sprite;
            return sprite;
        }
        catch (Exception ex)
        {
            _log.LogError("Error loading sprite " + fileName + ": " + ex);
            return null;
        }
    }

    [HarmonyPatch(typeof(PlayerUnitSpriteList), nameof(PlayerUnitSpriteList.GetNormalProfileSprite))]
    [HarmonyPrefix]
    private static bool GetNormalProfileSprite(int personalityId, ref Sprite __result)
    {
        var customSprite = LoadSpriteFromFile("profile_" + personalityId);
        if (customSprite == null) return true;
        __result = customSprite;
        return false;
    }
    
   
    [HarmonyPatch(typeof(PlayerUnitSpriteList), nameof(PlayerUnitSpriteList.GetInfoSprite))]
    [HarmonyPrefix]
    private static bool GetInfoSprite(int personalityId, ref Sprite __result)
    {
        return GetNormalProfileSprite(personalityId, ref __result);
    }
   
    [HarmonyPatch(typeof(PlayerUnitSpriteList), nameof(PlayerUnitSpriteList.GetCGData))]
    [HarmonyPrefix]
    private static bool GetCGData(int personalityId, ref Sprite __result)
    {
        var customSprite = LoadSpriteFromFile("cg_" + personalityId);
        if (customSprite == null) return true;
        __result = customSprite;
        return false;
    }
    
    [HarmonyPatch(typeof(SkillModel), nameof(SkillModel.GetSkillSprite))]
    [HarmonyPrefix]
    private static bool GetSkillSprite(SkillModel __instance, ref Sprite __result)
    {
        var customSprite = LoadSpriteFromFile("skill_" + __instance.GetID());
        if (customSprite == null) return true;
        __result = customSprite;
        return false;
    }
    
}