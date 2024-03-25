using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.IO;
using Il2CppSystem.Runtime.Serialization.Formatters.Binary;

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
            //     EncounterPanel.Instance ??= new EncounterPanel(CustomEncounterMod.UiBase);
            //     EncounterPanel.IsShown = !EncounterPanel.IsShown;
            // }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                //var sd = Singleton<StaticDataManager>.Instance.GetDungeonStage(99999, true, DUNGEON_TYPES.STORY_DUNGEON);
                Log.LogInfo("Entering custom fight");
                var sd = StaticDataManager.Instance.abBattleStageList.GetStage(10348);
                var ms = new MemoryStream();
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, sd);
                ms.Position = 0;
                sd = formatter.Deserialize(ms).Cast<StageStaticData>();
                ms.Close();
                Log.LogInfo("Copied");
                sd.waveList[0].unitList[0].unitCount = 3;
                EncounterHelper.ExecuteEncounter(new EncounterData()
                {
                    StageData = sd,
                    StageType = STAGE_TYPE.STORY_DUNGEON,
                });
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