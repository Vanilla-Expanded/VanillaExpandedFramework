using RimWorld;
using Verse;

namespace VFECore;

public class CompScheduleActiveAtSunlightRange : CompSchedule
{
    public new CompProperties_ScheduleActiveAtSunlightRange Props => (CompProperties_ScheduleActiveAtSunlightRange) props;

    public override void PostSpawnSetup(bool respawningAfterLoad) => RecalculateAllowed();

    public override void CompTick()
    {
        if (parent.IsHashIntervalTick(GenTicks.TickRareInterval))
            RecalculateAllowed();
    }

    public override void CompTickRare() => RecalculateAllowed();

    public override void CompTickLong() => RecalculateAllowed();

    private new void RecalculateAllowed() => Allowed = ShouldBeAllowed();

    protected virtual bool ShouldBeAllowed()
    {
        var props = Props;

        var sunGlow = GenCelestial.CurCelestialSunGlow(parent.Map);
        if (props.minLight > props.maxLight)
        {
            if (sunGlow < props.maxLight || sunGlow > props.minLight)
                return false;
        }
        else if (sunGlow < props.minLight || sunGlow > props.maxLight)
            return false;

        var dayPercent = GenLocalDate.DayPercent(parent);
        if (props.startTime > props.endTime)
        {
            if (dayPercent < props.endTime || dayPercent > props.startTime)
                return false;
        }
        else if (dayPercent < props.startTime || dayPercent > props.endTime)
            return false;

        return true;
    }
}