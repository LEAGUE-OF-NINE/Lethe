using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem.IO;
using MainUI;
using SimpleJSON;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Lethe.Patches
{
    public class Maps : Il2CppSystem.Object
    {
        private static ManualLogSource Log => LetheHooks.LOG;
        private static readonly Dictionary<string, MapPresetConfig> mapPresets = new();

        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<Maps>();
            harmony.PatchAll(typeof(Maps));
        }

        [HarmonyPatch(typeof(LobbyUIPresenter), nameof(LobbyUIPresenter.Initialize))]
        [HarmonyPostfix]
        private static void PostMainUILoad()
        {
            mapPresets.Clear(); // Clear the existing map presets before loading new ones
            foreach (var modPath in Directory.GetDirectories(LetheMain.modsPath.FullPath))
            {
                var expectedPath = Path.Combine(modPath, "custom_maps");
                if (!Directory.Exists(expectedPath)) continue;

                foreach (var jsonPath in Directory.GetFiles(expectedPath, "*.json", SearchOption.AllDirectories))
                {
                    try
                    {
                        // Load the map configuration from the JSON file
                        MapPresetConfig config = LoadMapConfigFromJson(jsonPath);
                        if (config == null) continue;

                        // Use the map name as the key to ensure uniqueness
                        if (!string.IsNullOrWhiteSpace(config.Name))
                        {
                            if (mapPresets.ContainsKey(config.Name))
                            {
                                Log.LogWarning($"Duplicate map name detected: {config.Name}. Skipping duplicate.");
                                continue;
                            }

                            mapPresets[config.Name] = config;
                            Log.LogInfo($"Loaded map preset: {config.Name}");
                        }
                        else
                        {
                            Log.LogWarning($"Map configuration in {jsonPath} is missing a name. Skipping.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogError($"Error processing map JSON at {jsonPath}: {ex.Message}");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(BattleMapManager), nameof(BattleMapManager.CreateBattleMapPreset))]
        [HarmonyPrefix]
        private static bool CreateBattleMapPreset(string mapName)
        {
            Log.LogWarning($"CreateBattleMapPreset: {mapName}");
            if (mapName.StartsWith("!custom_"))
            {
                Log.LogWarning("Detected custom map prefix. Loading custom preset...");

                mapPresets.TryGetValue(mapName, out var config);

                if (config == null)
                {
                    Log.LogError($"Did not find map {mapName} in dictionary, aborting...");
                    return false;
                }

                var preset = CreateEmptyMapPreset(mapName);
                if (preset == null)
                {
                    Log.LogError($"Failed to create empty preset for map {mapName}, aborting...");
                    return false;
                }

                ClearMap(preset);
                foreach (var wall in config.Walls)
                {
                    AddWall(preset, wall);
                }
                foreach (var floor in config.Floors)
                {
                    AddFloor(preset, floor);
                }

                BattleMapManager.Instance._mapObject.AddMapPreset(preset, mapName, -1);
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(BattleMapManager), nameof(BattleMapManager.LoadAndAddMap))]
        [HarmonyPrefix]
        private static bool LoadAndAddMap(string mapID, float mapSize)
        {
            Log.LogInfo($"LoadAndAddMap called with mapID: {mapID}");
            return true;
        }

        [HarmonyPatch(typeof(BattleMapManager), nameof(BattleMapManager.ChangeMap))]
        [HarmonyPrefix]
        private static bool ChangeMap(string mapID, float mapSize)
        {
            Log.LogInfo($"ChangeMap called with map id: {mapID}");
            return true;
        }

        // This creates and returns an empty map preset, it does NOT register the preset in the battlemapmanager.
        private static BattleMapPreset CreateEmptyMapPreset(string mapName)
        {
            var preset = BattleMapManager.Instance.CreateBattleMapPreset("Cp5_DrillingVessel_1F");
            preset._id = mapName;
            preset.name = mapName;
            if (preset._floorRootTransform != null && preset._backRootTransform != null)
            {
                ClearMap(preset);
                return preset;
            }
            else
            {
                Log.LogError("Failed to create empty map preset. floorRootTransform is null!");
                return null;
            }
        }

        private static void ClearMap(BattleMapPreset preset)
        {
            if (preset._floorRootTransform != null)
            {
                int childCount = preset._floorRootTransform.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    var child = preset._floorRootTransform.GetChild(i);
                    UnityEngine.Object.Destroy(child.gameObject);
                }
                Log.LogWarning("Cleared all children of floorRootTransform.");
            }
            else
            {
                Log.LogError("floorRootTransform is null!");
            }

            if (preset._backRootTransform != null)
            {
                int childCount = preset._backRootTransform.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    var child = preset._backRootTransform.GetChild(i);
                    UnityEngine.Object.Destroy(child.gameObject);
                }
                Log.LogWarning("Cleared all children of backRootTransform.");
            }
            else
            {
                Log.LogError("backRootTransform is null!");
            }
        }

        private static void AddWall(BattleMapPreset preset, MapElement element)
        {
            GameObject newChild = new("Wall");
            newChild.transform.SetParent(preset._backRootTransform, false);

            ApplyTransform(newChild.transform, element);

            SpriteRenderer spriteRenderer = newChild.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = LoadSpriteFromFile(element.Image);

            if (spriteRenderer.sprite != null)
            {
                Log.LogInfo($"Wall sprite successfully added: {element.Image}");
            }
            else
            {
                Log.LogError($"Failed to load wall sprite {element.Image}. Check the path or resource availability.");
            }
        }

        private static void AddFloor(BattleMapPreset preset, MapElement element)
        {
            GameObject newChild = new("Floor");
            newChild.transform.SetParent(preset._floorRootTransform, false);

            ApplyTransform(newChild.transform, element);

            SpriteRenderer spriteRenderer = newChild.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = LoadSpriteFromFile(element.Image);

            if (spriteRenderer.sprite != null)
            {
                Log.LogInfo($"Floor sprite successfully added: {element.Image}");
            }
            else
            {
                Log.LogError($"Failed to load floor sprite {element.Image}. Check the path or resource availability.");
            }
        }

        private static Sprite LoadSpriteFromFile(string fileName)
        {
            try
            {
                var data = File.ReadAllBytes(fileName);
                var tex = new Texture2D(2, 2);
                ImageConversion.LoadImage(tex, data);
                var sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                return sprite;
            }
            catch (Exception ex)
            {
                LetheHooks.LOG.LogError("Error loading sprite " + fileName + ": " + ex);
                return null;
            }
        }

        private class MapElement
        {
            public string Image { get; set; }
            public float[] Position { get; set; } = { 0f, 0f, 0f };
            public float[] Rotation { get; set; } = { 0f, 0f, 0f };
            public float[] Scale { get; set; } = { 1f, 1f, 1f };
        }

        private class MapPresetConfig
        {
            public string Name { get; set; } = "Default Map";
            public List<MapElement> Walls { get; set; } = new();
            public List<MapElement> Floors { get; set; } = new();
        }

        private static MapPresetConfig LoadMapConfigFromJson(string jsonFilePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(jsonFilePath);
                var jsonNode = JSON.Parse(jsonContent);
                if (jsonNode == null)
                {
                    Log.LogError($"Failed to parse JSON from {jsonFilePath}");
                    return null;
                }

                var config = new MapPresetConfig
                {
                    Name = jsonNode["Name"] ?? "Default Map",
                    Walls = ParseMapElements(jsonNode["Walls"]),
                    Floors = ParseMapElements(jsonNode["Floors"])
                };

                string directory = Path.GetDirectoryName(jsonFilePath);
                foreach (var wall in config.Walls)
                {
                    wall.Image = ResolveRelativePath(directory, wall.Image);
                }
                foreach (var floor in config.Floors)
                {
                    floor.Image = ResolveRelativePath(directory, floor.Image);
                }

                return config;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error reading JSON file {jsonFilePath}: {ex.Message}");
                return null;
            }
        }

        private static List<MapElement> ParseMapElements(JSONNode node)
        {
            var elements = new List<MapElement>();
            if (node == null || !node.IsArray) return elements;

            foreach (JSONNode item in node.AsArray)
            {
                var element = new MapElement
                {
                    Image = item["Image"].Value ?? string.Empty,
                    Position = ParseFloatArray(item["Position"], new[] { 0f, 0f, 0f }),
                    Rotation = ParseFloatArray(item["Rotation"], new[] { 0f, 0f, 0f }),
                    Scale = ParseFloatArray(item["Scale"], new[] { 1f, 1f, 1f })
                };

                elements.Add(element);
            }

            return elements;
        }

        private static float[] ParseFloatArray(JSONNode node, float[] defaultValues)
        {
            if (node == null || !node.IsArray) return defaultValues;

            var values = new float[defaultValues.Length];
            for (int i = 0; i < defaultValues.Length; i++)
            {
                values[i] = node[i]?.AsFloat ?? defaultValues[i];
            }

            return values;
        }

        private static string ResolveRelativePath(string baseDirectory, string relativePath)
        {
            return Path.Combine(baseDirectory, relativePath);
        }

        private static void ApplyTransform(Transform transform, MapElement element)
        {
            transform.localPosition = new Vector3(
                element.Position[0],
                element.Position[1],
                element.Position[2]
            );

            transform.localRotation = Quaternion.Euler(
                element.Rotation[0],
                element.Rotation[1],
                element.Rotation[2]
            );

            transform.localScale = new Vector3(
                element.Scale[0],
                element.Scale[1],
                element.Scale[2]
            );
        }
    }
}