using System.Collections.Generic;
using Verse;

namespace VEF.Buildings;

public class CompProperties_CustomCauseHediff_AoE : CompProperties
{
    public List<PawnCapacityDef> requiredCapacities;
    public HediffDef hediff;
    public bool mustBeAwake = false;

    public bool sameRoomOnly = true;
    public float range;

    public bool allowHumanlike = true;
    public bool allowInsects = false;
    public bool allowDryads = false;
    public bool allowAnimals = false;
    public bool allowMechanoids = false;
    public bool allowEntities = false;

    public bool worksInside = true;
    public bool worksOutside = true;

    public float startingSeverity = 1f;
    // Used in combination with HediffComp_Disappears.
    // For performance reasons, it shouldn't be too small.
    public int checkInterval = 100;
    // To be constant it should be longer by a few ticks than checkInterval.
    // Minimum of 15 ticks more than interval is recommended due to the way variable tick rate works.
    public int hediffDuration = 120;

    // In case someone wants to do weird stuff.
    protected virtual bool LogWorksBothInsideAndOutsideFieldAreFalse => true;

    public CompProperties_CustomCauseHediff_AoE() => compClass = typeof(CompCustomCauseHediff_AoE);

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (var error in base.ConfigErrors(parentDef))
            yield return error;

        if (LogWorksBothInsideAndOutsideFieldAreFalse && !worksInside && !worksOutside)
            yield return parentDef.defName + $" has {nameof(CompCustomCauseHediff_AoE)} with both {nameof(worksInside)} and {nameof(worksOutside)} set to false. The comp won't do anything at all.";
    }
}