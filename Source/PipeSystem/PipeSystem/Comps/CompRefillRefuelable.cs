using RimWorld;
using Verse;

namespace PipeSystem
{
    public class CompRefillRefuelable : CompResource
    {
        private CompRefuelable compRefuelable;

        public new CompProperties_RefillRefuelable Props => (CompProperties_RefillRefuelable)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compRefuelable = parent.GetComp<CompRefuelable>();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(250))
                CompTickRare();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (compRefuelable != null && compRefuelable.Fuel < compRefuelable.TargetFuelLevel)
            {
                var toAdd = compRefuelable.TargetFuelLevel - compRefuelable.Fuel; // The amount of fuel needed by compRefuelable
                var resourceNeeded = toAdd * Props.ratio; // Converted to the amount of resource
                // Check if needed resource is more that stored resource
                var stored = PipeNet.Stored;
                var resourceCanBeUsed = resourceNeeded < stored ? resourceNeeded : stored; // Can we spare all of it?

                compRefuelable.Refuel(resourceCanBeUsed / Props.ratio);
                PipeNet.DrawAmongStorage(resourceCanBeUsed);
            }
        }
    }
}
