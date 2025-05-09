using RimWorld;

namespace VFECore;

public class CompProperties_ScheduleActiveAtSunlightRange : CompProperties_Schedule
{
    public float minLight;
    public float maxLight = 1f;

    public CompProperties_ScheduleActiveAtSunlightRange() => compClass = typeof(CompScheduleActiveAtSunlightRange);
}