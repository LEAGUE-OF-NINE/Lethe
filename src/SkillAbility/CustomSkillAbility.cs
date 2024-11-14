using System;
using UnhollowerRuntimeLib;

namespace CustomEncounter.SkillAbility;

public class CustomSkillAbility : global::SkillAbility
{

    protected CustomSkillAbility(IntPtr ptr) : base(ptr)
    {
    }
    
    public CustomSkillAbility() : base(ClassInjector.DerivedConstructorPointer<CustomSkillAbility>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

}