﻿using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Buildings;

public class CompProperties_StatsWhenPowered : CompProperties
{
    public List<StatModifier> poweredStatFactors;
    public List<StatModifier> poweredStatOffsets;
    public List<StatModifier> unpoweredStatFactors;
    public List<StatModifier> unpoweredStatOffsets;

    public bool clearRoomCacheOnPowerChange = false;
    public List<StatDef> clearStatCacheOnPowerChange;

    public bool onlyWorksIndoors = false;
    public bool onlyWorksOutdoors = false;

    public CompProperties_StatsWhenPowered() => compClass = typeof(CompStatsWhenPowered);

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (var error in base.ConfigErrors(parentDef))
            yield return error;

        if (onlyWorksIndoors && onlyWorksOutdoors)
        {
            onlyWorksIndoors = false;
            onlyWorksOutdoors = false;
            yield return $"{parentDef.defName} has {nameof(CompProperties_StatsWhenPowered)} with both {onlyWorksIndoors} and {onlyWorksOutdoors} set to true. Setting both to false to prevent issues.";
        }
    }
}