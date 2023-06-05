using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    internal class GenStep_Settlement : RimWorld.GenStep_Settlement
    {
        public override int SeedPart => 1931078471;

        protected override bool CanScatterAt(IntVec3 loc, Map map)
        {
            return true;
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            map.ParentFaction.def.GetModExtension<CustomGenOption>().Generate(loc, map);
        }
    }
}