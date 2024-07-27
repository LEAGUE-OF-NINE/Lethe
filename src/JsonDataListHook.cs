using System;
using System.Reflection;
using Addressable;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using Utils;
using Object = Il2CppSystem.Object;

namespace CustomEncounter;

public class JsonDataListHook<T> : Object where T : LocalizeTextData, new()
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(JsonDataList<T>), "InitRemote", new[] { typeof(List<string>) });
    }

    public static bool Prefix(JsonDataList<T> __instance, List<string> jsonFilePathList)
    {
        AddressableManager.Instance.LoadLocalizeJsonAssetsAsync<T>(jsonFilePathList,
            new Action<LocalizeTextDataRoot<T>>(root =>
            {
                foreach (var data in root.DataList)
                    try
                    {
                        if (!__instance._dic.ContainsKey(data.ID)) __instance._dic.Add(data.ID, data);
                    }
                    catch
                    {
                        Debug.LogError(data.ID + " " + jsonFilePathList.GetFirstElement());
                    }
            }));

        return false;
    }
}

[HarmonyPatch]
internal class JsonDataListHookTextData_Ego : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_Ego>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_Ego> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_Ego>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_Personality : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_Personality>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_Personality> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_Personality>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_ActionEvents : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_ActionEvents>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_ActionEvents> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_ActionEvents>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_AttributeText : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_AttributeText>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_AttributeText> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_AttributeText>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_IAPProduct : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_IAPProduct>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_IAPProduct> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_IAPProduct>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_StageChapter : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_StageChapter>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_StageChapter> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_StageChapter>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_StoryTheater : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_StoryTheater>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_StoryTheater> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_StoryTheater>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_EgoGift : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_EgoGift>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_EgoGift> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_EgoGift>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_KeywordDictionary : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_KeywordDictionary>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_KeywordDictionary> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_KeywordDictionary>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_BattlePassMission : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_BattlePassMission>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_BattlePassMission> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_BattlePassMission>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_Quest : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_Quest>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_Quest> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_Quest>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_SkillTag : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_SkillTag>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_SkillTag> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_SkillTag>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_BattleKeywords : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_BattleKeywords>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_BattleKeywords> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_BattleKeywords>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_Passive : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_Passive>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_Passive> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_Passive>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_IntroduceCharacter : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_IntroduceCharacter>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_IntroduceCharacter> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_IntroduceCharacter>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_AbnormalityEvents : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_AbnormalityEvents>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_AbnormalityEvents> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_AbnormalityEvents>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_StoryDungeonStageName : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_StoryDungeonStageName>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_StoryDungeonStageName> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_StoryDungeonStageName>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_Item : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_Item>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_Item> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_Item>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_StagePart : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_StagePart>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_StagePart> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_StagePart>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_StageText : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_StageText>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_StageText> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_StageText>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_Buf : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_Buf>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_Buf> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_Buf>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_Skill : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_Skill>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_Skill> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_Skill>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_Character : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_Character>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_Character> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_Character>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_Enemy : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_Enemy>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_Enemy> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_Enemy>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_StoryText : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_StoryText>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_StoryText> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_StoryText>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_UI : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_UI>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_UI> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_UI>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_AbnormalityContents : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_AbnormalityContents>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_AbnormalityContents> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_AbnormalityContents>.Prefix(__instance, jsonFilePathList);
}

[HarmonyPatch]
internal class JsonDataListHookTextData_Announcer : MonoBehaviour
{
    public static MethodBase TargetMethod() => JsonDataListHook<TextData_Announcer>.TargetMethod();

    public static bool Prefix(JsonDataList<TextData_Announcer> __instance, List<string> jsonFilePathList) =>
        JsonDataListHook<TextData_Announcer>.Prefix(__instance, jsonFilePathList);
}