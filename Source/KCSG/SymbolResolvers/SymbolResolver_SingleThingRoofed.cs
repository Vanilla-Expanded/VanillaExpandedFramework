using RimWorld;
using RimWorld.BaseGen;
using System.Linq;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_SingleThingRoofed : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            if (!base.CanResolve(rp))
            {
                return false;
            }

            return this.TryFindSpawnCellForItem(rp.rect, out _);
        }

        public override void Resolve(ResolveParams rp)
        {
            
            ThingDef thingDef = rp.singleThingDef;
            if (thingDef == null)
            {
                thingDef = (from x in ThingSetMakerUtility.allGeneratableItems where x.IsWeapon || x.IsMedicine || x.IsDrug select x).RandomElement<ThingDef>();
            }

            if (thingDef.category == ThingCategory.Item)
            {
                if (!this.TryFindSpawnCellForItem(rp.rect, out IntVec3 intVec))
                {
                    if (rp.singleThingToSpawn != null)
                    {
                        rp.singleThingToSpawn.Destroy(DestroyMode.Vanish);
                    }
                    return;
                }

                ThingDef stuff = GenStuff.RandomStuffInexpensiveFor(thingDef, rp.faction, null);
                Thing thing = ThingMaker.MakeThing(thingDef, stuff);
                thing.stackCount = 1;
                
                if (thing.def.CanHaveFaction && thing.Faction != rp.faction)
                {
                    thing.SetFaction(rp.faction, null);
                }

                CompQuality compQuality = thing.TryGetComp<CompQuality>();
                if (compQuality != null)
                {
                    compQuality.SetQuality(QualityUtility.GenerateQualityBaseGen(), ArtGenerationContext.Outsider);
                }
                
                GenSpawn.Spawn(thing, intVec, BaseGen.globalSettings.map, Rot4.North, WipeMode.Vanish, false)?.SetForbidden(true, false);
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