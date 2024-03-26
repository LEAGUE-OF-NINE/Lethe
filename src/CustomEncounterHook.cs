using System;
using BepInEx.Logging;
using Dungeon;
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
            if (Input.GetKeyDown(KeyCode.F10))
            {
                EncounterHelper.SaveEncounters();
                EncounterHelper.SaveFormation();
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                Log.LogInfo("Entering custom fight");
                try
                {
                    var json = File.ReadAllText(CustomEncounterMod.EncounterConfig);
                    Log.LogInfo("Fight data:\n" + json);
                    var stageData = JsonUtility.FromJson<StageStaticData>(json);
                    json = File.ReadAllText(CustomEncounterMod.FormationConfig);
                    Log.LogInfo("Formation data:\n" + json);
                    var formation = JsonUtility.FromJson<Formation>(json);
                    EncounterHelper.ExecuteEncounter(new()
                    {
                        Name = "Custom Fight",
                        StageData = stageData,
                        StageType = STAGE_TYPE.STORY_DUNGEON
                    }, formation);
                }
                catch (Exception ex)
                {
                    Log.LogError("Error loading custom fight: " + ex.Message);
                }
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