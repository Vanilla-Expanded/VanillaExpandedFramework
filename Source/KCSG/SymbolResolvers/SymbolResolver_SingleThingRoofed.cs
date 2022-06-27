using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_SingleThingRoofed : SymbolResolver
    {
        private IntVec3 spawnPosition;

        public override bool CanResolve(ResolveParams rp)
        {
            if (!base.CanResolve(rp))
            {
                return false;
            }

            return TryFindSpawnCellForItem(rp.rect, out spawnPosition);
        }

        public override void Resolve(ResolveParams rp)
        {
            if (rp.singleThingToSpawn != null && !rp.singleThingToSpawn.Spawned)
            {
                if (spawnPosition.IsValid)
                {
                    Thing thing = rp.singleThingToSpawn;
                    GenSpawn.Spawn(thing, spawnPosition, BaseGen.globalSettings.map, Rot4.North, WipeMode.Vanish, false)?.SetForbidden(true, false);
                }
                else
                {
                    rp.singleThingToSpawn.Destroy(DestroyMode.Vanish);
                }
            }
        }

        private bool TryFindSpawnCellForItem(CellRect rect, out IntVec3 result)
        {
            Map map = BaseGen.globalSettings.map;
            return CellFinder.TryFindRandomCellInsideWith(rect, delegate (IntVec3 c)
            {
                if (c.GetFirstItem(map) != null)
                {
                    return false;
                }
                if (!c.Roofed(map))
                {
                    return false;
                }
                if (c.GetFirstBuilding(map) != null)
                {
                    return false;
                }
                return true;
            }, out result);
        }
    }
}