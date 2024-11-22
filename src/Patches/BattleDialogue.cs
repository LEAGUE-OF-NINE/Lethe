using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using UnhollowerRuntimeLib;
using System.Reflection;
using Voice;
using FMOD;

namespace LimbusSandbox.Patches
{
    internal class BattleDialogue : MonoBehaviour
    {
        private static Dictionary<string, string> why = new()
        {
            { "PlayBattleSelectVoice", "battle_select" }, 
            { "PlayBattleStartStageVoice", "battle_startstage" }, 
            { "PlayBattleEndCommandVoice", "battle_endcommand" }, 
            { "PlayBattleBreakVoice", "battle_break" }, 
            { "PlayBattleEnemyBreakVoice", "battle_enemy_break" }, 
            { "PlayBattleKillVoice", "battle_kill" }, 
            { "PlayBattleAllyDeadVoice", "battle_allydead" }, 
            { "PlayBattleDeadVoice", "battle_dead" }
        };
        
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<BattleDialogue>();
            harmony.PatchAll(typeof(BattleDialogue));
            foreach (var method in typeof(VoiceGenerator).GetMethods())
            {
                if (why.Keys.Contains(method.Name))
                {
                    Plugin.Log.LogInfo($"Patching voice {method.Name}");
                    harmony.Patch(method, null, new HarmonyMethod(typeof(BattleDialogue),nameof(PlayDialogueVoice)));
                }
            }
        }

        private static void PlayDialogueVoice(int id, bool isSpecial, int index, MethodBase __originalMethod)
        {
            Plugin.Log.LogWarning($"DETECT PLAY SOUND [{__originalMethod.Name}]");
            var soundID = $"{why[__originalMethod.Name]}_{id}_{index}";
            Plugin.Log.LogWarning(soundID);
            var view = BattleObjectManager.Instance.GetView(BattleObjectManager.Instance.GetModelByCharacterID(id));

            var list = TextDataManager.Instance._personalityVoiceText.GetDataList(id.ToString())?.dataList;
            if (list == null) return;
            Func<TextData_PersonalityVoice, bool> func = (x) => { return soundID == x.id; };
            var find = list.Find(func);
            if (find != null)
            {
                view._uiManager.ShowDialog(new BattleUI.Dialog.BattleDialogLine(find.dlg, ""));
            }
            else return;
        }

        [HarmonyPatch(typeof(BattleUnitView), nameof(BattleUnitView.SetPlayVoice))]
        [HarmonyPostfix]
        private static void SpawnBattleDialogueVoice(BattleUnitView __instance, BattleCharacterVoiceType key, bool isSpecial, BattleSkillViewer skillViewer)
        {
            var unitID = __instance.unitModel.GetUnitID();
            var soundID = "";
            switch (key)
            {

                /*case BattleCharacterVoiceType.Select:
                    soundID = @$"battle_select_{unitID}_1";
                    break;
                case BattleCharacterVoiceType.BattleStart:
                    soundID = @$"battle_startstage_{unitID}_1";
                    break;
                case BattleCharacterVoiceType.EndCommand:
                    soundID = @$"battle_endcommand_{unitID}_1";
                    break;
                case BattleCharacterVoiceType.AllyBreak:
                    soundID = @$"battle_break_{unitID}_1";
                    break;*/
                //somehow the sht doesnt detect stagger voice
                case BattleCharacterVoiceType.EnemyBreak:
                    soundID = @$"battle_enemy_break_{unitID}_1";
                    break;
                /*case BattleCharacterVoiceType.Kill:
                    soundID = @$"battle_kill_{unitID}_1";
                    break;
                case BattleCharacterVoiceType.AllyDead:
                    soundID = @$"battle_allydead_{unitID}_1";
                    break;
                case BattleCharacterVoiceType.S1:
                case BattleCharacterVoiceType.S2:
                case BattleCharacterVoiceType.S3:
                    break;
                case BattleCharacterVoiceType.Dead:
                    soundID = $@"battle_kill_{unitID}_1";
                    break;*/
            }

            var list = TextDataManager.Instance._personalityVoiceText.GetDataList(unitID.ToString())?.dataList;
            if (list == null) return;
            foreach (var bruh in list)
            {
                if (bruh.id != soundID) continue;
                __instance._uiManager.ShowDialog(new BattleUI.Dialog.BattleDialogLine(bruh.dlg, ""));
            }


        }

    }
}
