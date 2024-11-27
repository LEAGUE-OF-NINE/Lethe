using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SimpleJSON;
using Il2CppSystem;
using Convert = System.Convert;
using HarmonyLib;
using MainUI;

namespace LimbusSandbox.Utilities
{
    internal static class story_dungeon_autogen_to_main_chapter
    {
        public static void Setup(Harmony harmony)
        {
            harmony.PatchAll(typeof(story_dungeon_autogen_to_main_chapter));
        }
        
        /// <summary>
        /// redo later
        /// </summary>
    }
}
