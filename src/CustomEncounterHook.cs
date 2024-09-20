using System;
using BepInEx;
using BepInEx.Logging;
using Il2CppSystem.IO;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CustomEncounter;

public class CustomEncounterHook : MonoBehaviour
{
    public static ManualLogSource LOG;
    public static StageStaticData Encounter;

    public static DirectoryInfo CustomAppearanceDir, CustomSpriteDir, CustomLocaleDir, CustomAssistantDir;

    public CustomEncounterHook(IntPtr ptr) : base(ptr)
    {
    }

    internal void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            EncounterHelper.SaveLocale();
            EncounterHelper.SaveEncounters();
            EncounterHelper.SaveIdentities();
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            LOG.LogInfo("Entering custom fight");
            try
            {
                var json = File.ReadAllText(CustomEncounterMod.EncounterConfig);
                LOG.LogInfo("Fight data:\n" + json);
                Encounter = JsonUtility.FromJson<StageStaticData>(json);
                LOG.LogInfo("Success, please go to excavation 1 to start the fight.");
            }
            catch (Exception ex)
            {
                LOG.LogError("Error loading custom fight: " + ex.Message);
            }
        }
    }

    internal static void Setup(ManualLogSource log)
    {
        ClassInjector.RegisterTypeInIl2Cpp<CustomEncounterHook>();
        
        LOG = log;

        GameObject obj = new("CustomEncounterHook");
        DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.HideAndDontSave;
        obj.AddComponent<CustomEncounterHook>();

        CustomAppearanceDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_appearance"));
        CustomSpriteDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_sprites"));
        CustomLocaleDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_limbus_locale"));
        CustomAssistantDir = Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, "custom_assistant"));
    }


}