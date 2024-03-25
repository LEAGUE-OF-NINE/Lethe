using System;

namespace CustomEncounter;

[Serializable]
public class EncounterData
{
    public string Name { get; set; }
    public StageStaticData StageData { get; set; }
    public STAGE_TYPE StageType { get; set; }
}