using RimWorld;
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
            CGO.factionSettlement = map.ParentFaction.def.GetModExtension<CustomGenOption>();

            if (CGO.factionSettlement.symbolResolver == null) GenStepUtils.Generate(map, loc, CGO.factionSettlement);
            else GenStepUtils.Generate(map, loc, CGO.factionSettlement, CGO.factionSettlement.symbolResolver);
        }
    }
}