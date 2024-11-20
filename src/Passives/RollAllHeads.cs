﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using UnhollowerRuntimeLib;

namespace LimbusSandbox.Passives
{
    internal class RollAllHeads : MonoBehaviour
    {
        public static void Setup(Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<RollAllHeads>();
            harmony.PatchAll(typeof(RollAllHeads));
        }



        //this is bad because it checks every coin RAHHHHHHHHHHH
        //try BattleUnitModel.OnStartCoin instead later (im pushing naww)
        [HarmonyPatch(typeof(CoinModel), nameof(CoinModel.Roll))]
        [HarmonyPrefix]
        private static void RollCoin(CoinModel __instance, float prob, BattleActionModel action)
        {
            var id = action._model.UnitDataModel.ClassInfo;
            if (id.PassiveSetInfo.PassiveIdList.Contains(59373351))
            {
                __instance._result = COIN_RESULT.HEAD;
            }
        }
    }
}
