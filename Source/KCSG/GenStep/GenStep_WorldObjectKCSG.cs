using RimWorld;
using RimWorld.Planet;
using Verse;

namespace KCSG
{
    internal class GenStep_WorldObjectKCSG : GenStep_Settlement
    {
        public override int SeedPart => 1969308471;

        protected override bool CanScatterAt(IntVec3 loc, Map map)
        {
            return true;
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            WorldObject worldO = Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == map.Tile && o.def.HasModExtension<FactionSettlement>());
            CurrentGenerationOption.factionSettlement = worldO.def.GetModExtension<FactionSettlement>();

            if (CurrentGenerationOption.factionSettlement.symbolResolver == null) GenStepPatchesUtils.Generate(map, loc, CurrentGenerationOption.factionSettlement);
            else GenStepPatchesUtils.Generate(map, loc, CurrentGenerationOption.factionSettlement, CurrentGenerationOption.factionSettlement.symbolResolver);
        }
    }
}