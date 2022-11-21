using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PipeSystem
{
    public class CompProperties_ConvertResourceToThing : CompProperties_Resource
    {
        public CompProperties_ConvertResourceToThing()
        {
            compClass = typeof(CompConvertToThing);
        }

        public int ratio = 1;
        public int maxOutputStackSize = -1;
        public ThingDef thing;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string err in base.ConfigErrors(parentDef))
                yield return err;

            if (!typeof(Building_Storage).IsAssignableFrom(parentDef.thingClass))
                yield return "Can't use CompProperties_ConvertResourceToThing with a thing that don't have Building_Storage as thingClass.";
            if (parentDef.comps.FindAll(c => c is CompProperties_ConvertResourceToThing).Count > 1)
                yield return "Can't use multiple CompProperties_ConvertResourceToThing on the same thing.";
            if (thing == null)
                yield return "Can't use CompProperties_ConvertResourceToThing with a null thing.";
        }
    }
}