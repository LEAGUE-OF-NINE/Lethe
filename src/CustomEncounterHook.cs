using System;
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
            if (Input.GetKeyDown(KeyCode.F10))
            {
                EncounterHelper.SaveEncounters();
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                Log.LogInfo("Entering custom fight");
                try
                {
                    var json = File.ReadAllText(CustomEncounterMod.EncounterConfig);
                    Log.LogInfo("Fight data:\n" + json);
                    EncounterHelper.ExecuteEncounter(new()
                    {
                        Name = "Custom Fight",
                        StageData = JsonUtility.FromJson<StageStaticData>(json),
                        StageType = STAGE_TYPE.STORY_DUNGEON
                    });
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