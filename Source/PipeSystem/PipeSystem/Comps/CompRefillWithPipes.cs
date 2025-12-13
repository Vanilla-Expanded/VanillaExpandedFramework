using RimWorld;
using System.Linq;
using Verse;

namespace PipeSystem
{
    public class CompRefillWithPipes : CompResource
    {
        public CompRefuelable compRefuelable;


        public new CompProperties_RefillWithPipes Props => (CompProperties_RefillWithPipes)props;

        /// <summary>
        /// Try get parent as Building_ItemProcessor. Get CompRefuelable matching Props.thing
        /// </summary>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);


            var comps = parent.GetComps<CompRefuelable>().ToList();
            for (int i = 0; i < comps.Count; i++)
            {
                var comp = comps[i];
                if (comp.Props.fuelFilter.Allows(Props.thing))
                {
                    compRefuelable = comp;
                    break;
                }
            }
        }

        /// <summary>
        /// Refill CompRefuelable. Return the amount used
        /// </summary>
        /// <param name="available">Amount available to refuel</param>
        /// <returns>Amount used</returns>
        public float Refill(float available)
        {
            if (compRefuelable == null)
                return 0f;

            var toAdd = (compRefuelable.TargetFuelLevel - compRefuelable.Fuel) / compRefuelable.Props.FuelMultiplierCurrentDifficulty; // The amount of fuel needed by compRefuelable
            // Don't drain the refuelable if it's over the target level
            if (toAdd <= 0f)
                return 0f;

            var resourceNeeded = toAdd * Props.ratio; // Converted to the amount of resource
            // Check if needed resource is more that available resource
            var resourceCanBeUsed = resourceNeeded < available ? resourceNeeded : available; // Can we spare all of it?
            // Refuel
            compRefuelable.Refuel(resourceCanBeUsed / Props.ratio);
            // Return amount used
            return resourceCanBeUsed;
        }
    }
}