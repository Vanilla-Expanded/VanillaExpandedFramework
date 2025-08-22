using RimWorld;
using Verse;

namespace VEF.Graphics;

public class CompDirtyMapMeshOnRefuelableChange : ThingComp
{
    public override void ReceiveCompSignal(string signal)
    {
        base.ReceiveCompSignal(signal);

        // Required by Graphic_RefuelableMulti and Graphic_RefuelableSingle
        if (signal is CompRefuelable.RanOutOfFuelSignal or CompRefuelable.RefueledSignal && parent.Spawned)
            parent.DirtyMapMesh(parent.Map);

        // We technically could make a patch when the refuelable is refueled/runs out of fuel
        // (and check if Graphic requires dirtying), but I'd rather avoid that due to performance concerns.
    }
}