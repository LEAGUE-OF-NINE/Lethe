using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.IL2CPP;
using HarmonyLib;
using LimbusSandbox.Fixes;
using LimbusSandbox.Patches;
using LimbusSandbox.SkillAbility;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using LimbusSandbox.Passives;
using LimbusSandbox.ps2;

namespace LimbusSandbox
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BasePlugin
    {
        internal static new ManualLogSource Log;
        public const string GUID = "leagueofskibidi.LimbusSandbox";
        public const string NAME = "LimbusSandbox";
        public const string VERSION = "0.0.1";

        public static string pluginPath = Path.Combine(Paths.PluginPath, "LimbusSandbox");
        public static string customDataPath = Path.Combine(pluginPath, "customData");
        public static string vanillaDataPath = Path.Combine(pluginPath, "dumpedData");
        public static string __testpath = Path.Combine(pluginPath, "mods");
        public static string customAppearancePath = Path.Combine(pluginPath, "customAppearance");

        public static List<string> localeTags = new() { "abBattleResultHint", "abnormalityCotentData", "abnormalityEventList", "actionEvents", "announcer", "attributeList", "battleHint", "battlePass", "battlePassMission", "battleSpeechBubbleText", "bufList", "buffAbilityList", "characterList", "choiceEventResultDesc", "danteAbilityDataList", "danteNoteCategoryKeyword", "danteNoteDesc", "dungeonArea", "dungeonName", "dungeonNodeText", "dungeonStartBuffs", "egoGiftCategory", "egoGiftData", "egoList", "enemyList", "fileDownloadDesc", "gachaTitle", "iapProductText", "iapStickerText", "illustGetConditionText", "introduceCharacter", "itemList", "keywordDictionary", "keywordList", "loginUIList", "mentalConditionList", "mirrorDungeonEgoGiftLockedDescList", "mirrorDungeonEnemyBuffDescList", "mirrorDungeonThemeList", "normalBattleResultHint", "panicInfo", "passiveList", "personalityList", "quest", "railwayDungeonBuffText", "railwayDungeonNodeText", "railwayDungeonStationName", "railwayDungeonText", "skillList", "skillTagList", "stageChapter", "stageNodeText", "stagePart", "storyDungeonNodeText", "storyTheater", "threadDungeon", "tutorialDesc", "uiList", "unlockCodeList", "userBanner", "userTicket_EGOBg", "userTicket_L", "userTicket_R" };
        public static ConfigFile Configuration;

        public override void Load()
        {
            // Plugin startup logic
            Configuration = Config;
            LoadConfiguration();
            Log = base.Log;

            var harmony = new Harmony(NAME);
            //yes
            SandboxLoader.Setup(harmony);
            ps_cs.Setup(harmony);
            //patches
            Configuration.TryGetEntry<bool>(new ConfigDefinition("LaunchSettings", "usePrivateServer"), out var ps);
            if (ps.Value) PrivateServer.Setup(harmony);
            GameDataLoader.Setup(harmony);
            PlaygroundStage.Setup(harmony);
            CustomAppearance.Setup(harmony);
            Configuration.TryGetEntry<bool>(new ConfigDefinition("LaunchSettings", "ToggleCRC"), out var toggleCRC);
            if (toggleCRC.Value) PatchCalculation.Setup(harmony);
            BattleDialogue.Setup(harmony);

            //passives
            RollAllHeads.Setup(harmony);

            //skillability
            EvadeThenUseSkill.Setup(harmony);
            CustomDuelViewer.Setup(harmony);
            ChangeAppearance.Setup(harmony);
            ChangeSkillMotion.Setup(harmony);

            //fixes
            AppearancePatch.Setup(harmony);

            Directory.CreateDirectory(pluginPath);
            Directory.CreateDirectory(customDataPath);
            Directory.CreateDirectory(vanillaDataPath);
            Directory.CreateDirectory(customAppearancePath);
            Directory.CreateDirectory(__testpath);
        }

        public static void LoadConfiguration()
        {
            //crc toggle config
            Configuration.Bind
            (
                "LaunchSettings",
                "ToggleCRC",
                false,
                "Toggle file checks from the game. Makes the game load faster. Disable when downloading updates."
            );

            Configuration.Bind
            (
                "LaunchSettings",
                "ServerURL",
                "https://api.lethelc.site/",
                "The URL of the private server (e.g. http://127.0.0.1:8080), leave blank to use defaults."
            );

            Configuration.Bind
           (
               "LaunchSettings",
               "usePrivateServer",
               true,
               "risky strats"
           );

            Configuration.Bind
            (
                "General",
                "ReloadData",
                KeyCode.Alpha8,
                "Key bind to reload data ingame."
            );
        }


    }
}
