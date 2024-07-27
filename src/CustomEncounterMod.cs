using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using System.Linq;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.IO;
using SD;
using UnityEngine;

namespace CustomEncounter
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CustomEncounterMod : BasePlugin
    {
        public const string GUID = "carra.CustomEncounter";
        public const string NAME = "CustomEncounter";
        public const string VERSION = "0.0.5";
        public const string AUTHOR = "Carra";
        public static Action<string, Action> LogFatalError { get; set; }
        public static Action<string> LogError { get; set; }
        public static Action<string> LogWarning { get; set; }
       //  public static UIBase UiBase { get; private set; }
      
        public static string EncounterConfig = Path.Combine(Paths.ConfigPath, "encounter.json");

        public override void Load()
        {
            LogError = (string log) => { Log.LogError(log); Debug.LogError(log); };
            LogWarning = (string log) => { Log.LogWarning(log); Debug.LogWarning(log); };
            LogFatalError = (string log, Action action) => { LogError(log); Debug.LogError(log); };
            try
            {
                CustomEncounterHook.Setup(Log);
                Harmony harmony = new(NAME);
                harmony.PatchAll(typeof(CustomEncounterHook));
                foreach (var declaredMethod in AccessTools.GetDeclaredMethods(typeof(SDCharacterSkinUtil)))
                {
                    if (declaredMethod.Name == nameof(SDCharacterSkinUtil.CreateSkin))
                    {
                        Log.LogInfo($"Patching {declaredMethod.Name}");
                        var isDirect = declaredMethod.GetParameters().ToArray().Any(x => x.Name == "appearanceID");
                        harmony.Patch(declaredMethod,
                            prefix: new HarmonyMethod(AccessTools.Method(typeof(CustomEncounterHook), isDirect ? nameof(CustomEncounterHook.CreateSkin) : nameof(CustomEncounterHook.CreateSkinForModel))));
                    }
                }
                
                registerJsonListHook(harmony);
                
                EncounterHelper.Log = Log;
                if (!File.Exists(EncounterConfig))
                {
                    File.Create(EncounterConfig).Close();
                }
            }
            catch (Exception e)
            {
                LogFatalError("Unknown fatal error!!!\n", () => {});
                LogError(e.ToString());
            }
        }

        public static void registerJsonListHook(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_SkillTag>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_UI>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_StageText>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_KeywordDictionary>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_StoryText>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_Personality>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_BattleKeywords>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_ActionEvents>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_AttributeText>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_Quest>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_StageChapter>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_Passive>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_Ego>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_EgoGift>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_Buf>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_Enemy>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_Character>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_IAPProduct>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_StoryTheater>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_Skill>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_StagePart>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_Announcer>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_IntroduceCharacter>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_StoryDungeonStageName>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_BattlePassMission>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_Item>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_AbnormalityContents>();
            ClassInjector.RegisterTypeInIl2Cpp<JsonDataListHookTextData_AbnormalityEvents>();
            
            harmony.PatchAll(typeof(JsonDataListHookTextData_SkillTag));
            harmony.PatchAll(typeof(JsonDataListHookTextData_UI));
            harmony.PatchAll(typeof(JsonDataListHookTextData_StageText));
            harmony.PatchAll(typeof(JsonDataListHookTextData_KeywordDictionary));
            harmony.PatchAll(typeof(JsonDataListHookTextData_StoryText));
            harmony.PatchAll(typeof(JsonDataListHookTextData_Personality));
            harmony.PatchAll(typeof(JsonDataListHookTextData_BattleKeywords));
            harmony.PatchAll(typeof(JsonDataListHookTextData_ActionEvents));
            harmony.PatchAll(typeof(JsonDataListHookTextData_AttributeText));
            harmony.PatchAll(typeof(JsonDataListHookTextData_Quest));
            harmony.PatchAll(typeof(JsonDataListHookTextData_StageChapter));
            harmony.PatchAll(typeof(JsonDataListHookTextData_Passive));
            harmony.PatchAll(typeof(JsonDataListHookTextData_Ego));
            harmony.PatchAll(typeof(JsonDataListHookTextData_EgoGift));
            harmony.PatchAll(typeof(JsonDataListHookTextData_Buf));
            harmony.PatchAll(typeof(JsonDataListHookTextData_Enemy));
            harmony.PatchAll(typeof(JsonDataListHookTextData_Character));
            harmony.PatchAll(typeof(JsonDataListHookTextData_IAPProduct));
            harmony.PatchAll(typeof(JsonDataListHookTextData_StoryTheater));
            harmony.PatchAll(typeof(JsonDataListHookTextData_Skill));
            harmony.PatchAll(typeof(JsonDataListHookTextData_StagePart));
            harmony.PatchAll(typeof(JsonDataListHookTextData_Announcer));
            harmony.PatchAll(typeof(JsonDataListHookTextData_IntroduceCharacter));
            harmony.PatchAll(typeof(JsonDataListHookTextData_StoryDungeonStageName));
            harmony.PatchAll(typeof(JsonDataListHookTextData_BattlePassMission));
            harmony.PatchAll(typeof(JsonDataListHookTextData_Item));
            harmony.PatchAll(typeof(JsonDataListHookTextData_AbnormalityContents));
            harmony.PatchAll(typeof(JsonDataListHookTextData_AbnormalityEvents));
        }
    }
}
