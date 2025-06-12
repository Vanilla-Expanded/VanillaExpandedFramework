
﻿using RimWorld;

namespace VEF.Buildings;

public class CompProperties_ScheduleExtended : CompProperties_Schedule
{
    public float minLight;
    public float maxLight = 1f;
    public string sunlightMessage = null;

    // We could handle this with a single bool?, but I feel this is clearer this way.
    public bool disableUnderRoof = false;
    public bool disableWithoutRoof = false;
    public string disabledDueToRoofMessage = null;

    public CompProperties_ScheduleExtended() => compClass = typeof(CompScheduleExtended);
}