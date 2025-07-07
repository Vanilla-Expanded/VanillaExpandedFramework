
﻿using RimWorld;
using Verse;

namespace VEF.Buildings;

public class CompScheduleExtended : CompSchedule
{
    public new CompProperties_ScheduleExtended Props => (CompProperties_ScheduleExtended)props;

    public override void PostSpawnSetup(bool respawningAfterLoad) => RecalculateAllowed();

    public override void CompTickInterval(int delta)
    {
        if (parent.IsHashIntervalTick(GenTicks.TickRareInterval, delta))
            RecalculateAllowed();
    }

    public override void CompTickRare() => RecalculateAllowed();

    public override void CompTickLong() => RecalculateAllowed();

    private new void RecalculateAllowed() => Allowed = ShouldBeAllowed();

    protected virtual AcceptanceReport ShouldBeAllowed()
    {
        var target = parent.SpawnedParentOrMe;
        if (target == null)
            return true;

        var props = Props;

        if (props.disableUnderRoof)
        {
            if (RoofUtility.IsAnyCellUnderRoof(target))
                return props.disabledDueToRoofMessage;
        }
        else if (props.disableWithoutRoof && !RoofUtility.IsAnyCellUnderRoof(target))
            return props.disabledDueToRoofMessage;

        var sunGlow = target.Map.skyManager.CurSkyGlow;
        if (props.minLight > props.maxLight)
        {
            if (sunGlow < props.maxLight || sunGlow > props.minLight)
                return props.sunlightMessage;
        }
        else if (sunGlow < props.minLight || sunGlow > props.maxLight)
            return props.sunlightMessage;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (props.startTime != 0f || props.endTime != 1f)
        {
            var dayPercent = GenLocalDate.DayPercent(target);
            if (props.startTime > props.endTime)
            {
                if (dayPercent < props.endTime || dayPercent > props.startTime)
                    return false;
            }
            else if (dayPercent < props.startTime || dayPercent > props.endTime)
                return false;
        }

        return true;
    }

    public override string CompInspectStringExtra()
    {
        if (Allowed)
            return null;

        return ShouldBeAllowed().Reason ?? Props.offMessage;
    }
}