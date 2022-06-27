using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public class CompProperties_ExplosiveContent : CompProperties_Explosive
    {
        public CompProperties_ExplosiveContent()
        {
            compClass = typeof(CompExplosiveContent);
        }

        public int explosiveMaxRadius;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;

            if (parentDef.GetCompProperties<CompProperties_ResourceStorage>() == null)
                yield return "CompProperties_ExplosiveContent cannot be used on a thing without CompProperties_ResourceStorage";
        }
    }
}
