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

        // Min/max radius, scales linearly
        public float explosiveMaxRadius;
        public float explosiveMinRadius;

        // Min radius at which the explosion can occur
        public float radiusRequiredForExplosion;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;

            if (parentDef.GetCompProperties<CompProperties_ResourceStorage>() == null)
                yield return $"{nameof(CompProperties_ExplosiveContent)} cannot be used on a thing without {nameof(CompProperties_ResourceStorage)}";
        }
    }
}
