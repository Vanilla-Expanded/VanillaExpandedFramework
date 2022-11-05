using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public class CompProperties_DeepExtractor : CompProperties_Resource
    {
        public CompProperties_DeepExtractor()
        {
            compClass = typeof(CompDeepExtractor);
        }

        public ThingDef deepThing;
        public int ticksPerPortion = 60;
        public string noStorageLeftKey = "PipeSystem_CantExtract";
        public bool onlyExtractToNet = false;
        public bool onlyExtractToGround = false;
        public bool useDeepCountPerPortion = true;
        public bool showDeepCountLeft = true;

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;

            if (onlyExtractToNet && onlyExtractToGround)
                yield return "onlyExtractToNet and onlyExtractToGround cannot both be true";
        }
    }
}