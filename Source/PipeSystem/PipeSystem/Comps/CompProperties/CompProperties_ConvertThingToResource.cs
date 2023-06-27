using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public class CompProperties_ConvertThingToResource : CompProperties_Resource
    {
        public CompProperties_ConvertThingToResource()
        {
            compClass = typeof(CompConvertToResource);
        }

        public int ratio = 1;
        public ThingDef thing;
        public ThingCategoryDef category;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string err in base.ConfigErrors(parentDef))
                yield return err;

            if (parentDef.thingClass != typeof(Building_Storage))
                yield return "Can't use CompProperties_ConvertThingToResource with a thing that don't have Building_Storage as thingClass.";
            if (parentDef.comps.FindAll(c => c is CompProperties_ConvertThingToResource).Count > 1)
                yield return "Can't use multiple CompProperties_ConvertThingToResource on the same thing.";
            if (thing == null & category == null)
                yield return "Can't use CompProperties_ConvertThingToResource with a null thing or category.";
            if (thing != null & category != null)
                yield return "Can't use both thing and category on the same CompProperties_ConvertThingToResource.";
        }
    }
}
