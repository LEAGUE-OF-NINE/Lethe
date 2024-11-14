using System;
using Il2CppSystem.Collections.Generic;
using UnhollowerRuntimeLib;

namespace CustomEncounter.SkillAbility;

public class SkillAbilityEvadeThenUseSkill : global::SkillAbility
{

    public SkillAbilityEvadeThenUseSkill() : base(ClassInjector.DerivedConstructorPointer<global::SkillAbility>())
    {
        CustomEncounterHook.LOG.LogInfo($"Mickey mickey");
        ClassInjector.DerivedConstructorBody(this);
        this._copiedTriggerDictionary = new Dictionary<BATTLE_EVENT_TIMING, bool>();
        this._abilityTriggeredDataList = new List<AbilityTriggeredData>();
        this._keywordDic = new Dictionary<SKILL_KEYWORD, byte>();
        CustomEncounterHook.LOG.LogInfo($"What!!!");
    }

    public SkillAbilityEvadeThenUseSkill(IntPtr ptr) : base(ptr)
    {
        CustomEncounterHook.LOG.LogInfo($"Pointer is {ptr.ToInt32()}");
    }
    
    public override void Init(SkillModel skill, string scriptName, int idx, BuffReferenceData info = null)
    {
        CustomEncounterHook.LOG.LogInfo("WHAT THE FUCK PEOPLE HOLY SHIT");
        base.Init(skill, scriptName, idx, info);
        CustomEncounterHook.LOG.LogInfo("Registered [SkillAbility_MeowMeowMeow]");
    }

    public override void OnSucceedAttack(BattleActionModel action, CoinModel coin, BattleUnitModel target, int finalDmg, int realDmg,
        bool isCritical, BATTLE_EVENT_TIMING timing)
    {
        CustomEncounterHook.LOG.LogInfo("Hyperborea");
    }
}
