using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;

//I have no fucking idea what I'm doing!!!!! raaagh!!!!!
namespace CustomEncounter.SkillAbility;

public class SkillAbility_BuffStackAdderNCorpOnStartBattle : global::SkillAbility
{
  private int _BUFF_STACK_ADDER;

  private const UNIT_KEYWORD _ASSOCIATION = UNIT_KEYWORD.N_CORP_FNATIC;

  protected override Dictionary<BATTLE_EVENT_TIMING, bool> _triggerDictionary => new Dictionary<BATTLE_EVENT_TIMING, bool>()
  {
    {
      BATTLE_EVENT_TIMING.BEFORE_ROLL_COIN_ACTION,
      false
    },
    {
      BATTLE_EVENT_TIMING.AFTER_ROLL_COIN_ACTION,
      false
    },
    {
      BATTLE_EVENT_TIMING.ON_END_COIN,
      false
    },
    {
      BATTLE_EVENT_TIMING.ON_SUCCESS_ATTACK,
      false
    }
  };

  protected bool CheckReinforced(UNIT_FACTION faction) 
  {
    Predicate<BattleUnitModel> predicate = null;

    if (faction == UNIT_FACTION.N_CORP_FNATIC) // Assuming you want to check faction here
    {
        predicate = x => x.IncludedInAssociation(UNIT_KEYWORD.UNIT_KEYWORD.N_CORP_FNATIC);
    }

    List<BattleUnitModel> battleUnitModelList;
    List<BattleUnitModel> all = (List<BattleUnitModel>) ((List<>) battleUnitModelList).FindAll((Predicate<>) predicate);

    return all.Count > 0;
  }

  public override void Init(SkillModel skill, string scriptName, int idx, BuffStaticData info = null)
  {
    base.Init(skill, scriptName, idx, info);
    string s = Regex.Replace(scriptName.Split('(', ')')[0], "\\D", "");
    this._BUFF_STACK_ADDER = s == null || s.Length <= 0 ? 0 : int.Parse(s);
  }

  public override void OnBattleStart(BattleActionModel action) => this.SetAllTrigger(true);

  public override int GetBuffStackAdder(
    UnitModel unit,
    BattleActionModel action,
    BattleUnitModel target,
    BUFF_UNIQUE_KEYWORD keyword)
    {
      return this._BUFF_STACK_ADDER;
    }
}