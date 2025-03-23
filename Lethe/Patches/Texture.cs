using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.IO;
using MainUI;
using UnityEngine;

namespace Lethe.Patches;

public class Texture : Il2CppSystem.Object
{

    private static readonly Dictionary<string, Sprite> Sprites = new();
    private static readonly Dictionary<string, bool> SpriteExist = new();
    private static List<string> spritePaths = new();

    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Texture>();
        harmony.PatchAll(typeof(Texture));
    }

    [HarmonyPatch(typeof(LobbyUIPresenter), nameof(LobbyUIPresenter.Initialize))]
    [HarmonyPostfix]
    private static void PostMainUILoad()
    {
        spritePaths.Clear();
        foreach (var modPath in Directory.GetDirectories(LetheMain.modsPath.FullPath))
        {
            var expectedPath = Path.Combine(modPath, "custom_sprites");
            if (!Directory.Exists(expectedPath)) continue;

            foreach (var imgpath in Directory.GetFiles(expectedPath, "*.png", SearchOption.AllDirectories))
                spritePaths.Add(imgpath);
        }
    }
    private static Sprite LoadSpriteFromFile(string fileName)
    {
        if (SpriteExist.TryGetValue(fileName, out var spriteExists) && !spriteExists)
            return null;

        if (Sprites.TryGetValue(fileName, out var sprite) && sprite != null)
            return sprite;

        try
        {
            var path = spritePaths.Find(x => x.Contains(fileName));
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
            LetheHooks.LOG.LogError("Error loading sprite " + fileName + ": " + ex);
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
        var customSprite = LoadSpriteFromFile("skill_" + __instance.GetSkillIconID()) ?? LoadSpriteFromFile("skill_" + __instance.GetID());
        if (customSprite == null) return true;
        __result = customSprite;
        return false;
    }

}