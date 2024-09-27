using System;
using UnhollowerRuntimeLib;

namespace CustomEncounter.SkillAbility;

public class SkillAbilityEvadeThenUseSkill : global::SkillAbility
{

    public SkillAbilityEvadeThenUseSkill(IntPtr ptr) : base(ptr)
    {
    }
    
    public override void Init(SkillModel skill, string scriptName, int idx, BuffReferenceData info = null)
    {
        base.Init(skill, scriptName, idx, info);
        CustomEncounterHook.LOG.LogInfo("Registered [SkillAbility_EvadeThenUseSkill]");
    }

    public override void OnFailedEvade(BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing)
    {
        OnEvade(attackerAction, evadeAction, timing);
    }

    public override void OnSucceedEvade(BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing)
    {
        OnEvade(attackerAction, evadeAction, timing);
    }

    private void OnEvade(BattleActionModel attackerAction, BattleActionModel evadeAction, BATTLE_EVENT_TIMING timing)
    {
        CustomEncounterHook.LOG.LogInfo("[SkillAbility_EvadeThenUseSkill] OnEvade");
    }
    
}