using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using BepInEx.Configuration;

namespace Lethe.Patches
{
    internal class ego_resource
    {
        private static Dictionary<string, ATTRIBUTE_TYPE> attrList = new()
        {
            { "WRATH", ATTRIBUTE_TYPE.CRIMSON },
            { "LUST", ATTRIBUTE_TYPE.SCARLET },
            { "SLOTH", ATTRIBUTE_TYPE.AMBER },
            { "GLUTTONY", ATTRIBUTE_TYPE.SHAMROCK },
            { "GLOOM", ATTRIBUTE_TYPE.AZURE },
            { "PRIDE", ATTRIBUTE_TYPE.INDIGO },
            { "ENVY", ATTRIBUTE_TYPE.VIOLET }
        };

        public static void Setup(Harmony harmony)
        {
            harmony.PatchAll(typeof(ego_resource));

            //at some point i stopped caring
            var skibidi_or_not = LetheMain.config;
            foreach (var attr in attrList)
            {
                skibidi_or_not.Bind("STARTING_EGO_RESOURCE", attr.Key, 0, $"starting ego resource for {attr.Key} ({attr.Value}) (might stack on dungeons)");
            }
        }

        [HarmonyPatch(typeof(StageController), nameof(StageController.StartStage))]
        [HarmonyPrefix]
        private static void change_starting_ego_resource()
        {
            var skibidi_or_not = LetheMain.config;
            foreach (var attr in attrList)
            {
                try
                {
                    skibidi_or_not.TryGetEntry<int>(new ConfigDefinition("STARTING_EGO_RESOURCE", attr.Key), out var entry);
                    SinManager.Instance.AddSinStock(UNIT_FACTION.PLAYER, attr.Value, entry.Value);
                }
                catch (Exception ex)
                {
                    LetheMain.LogError("you baka!!!!" + ex.Message);
                }
            }
        }
    }
}
