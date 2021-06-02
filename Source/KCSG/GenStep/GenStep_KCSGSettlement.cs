using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System.Linq;
using Verse;

namespace KCSG
{
    internal class GenStep_KCSGSettlement : GenStep_Settlement
    {
        public override int SeedPart => 1931078471;

        protected override bool CanScatterAt(IntVec3 loc, Map map)
        {
            return true;
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            if (map.ParentFaction != null && map.ParentFaction.def.HasModExtension<FactionSettlement>())
            {
                FactionSettlement factionSettlement = map.ParentFaction.def.GetModExtension<FactionSettlement>();

                if (factionSettlement.symbolResolver == null) GenStepPatchesUtils.Generate(map, loc, factionSettlement);
                else GenStepPatchesUtils.Generate(map, loc, factionSettlement, factionSettlement.symbolResolver);
            }
            else if (Find.World.worldObjects.AllWorldObjects.Find(o => o.Tile == map.Tile && o.def.HasModExtension<FactionSettlement>()) is WorldObject worldObject)
            {
                FactionSettlement factionSettlement = worldObject.def.GetModExtension<FactionSettlement>();

                if (factionSettlement.symbolResolver == null) GenStepPatchesUtils.Generate(map, loc, factionSettlement);
                else GenStepPatchesUtils.Generate(map, loc, factionSettlement, factionSettlement.symbolResolver);
            }
        }
    }
}