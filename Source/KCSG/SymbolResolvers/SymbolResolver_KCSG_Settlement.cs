using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    internal class SymbolResolver_KCSG_Settlement : SymbolResolver
    {
        private void PreClean(Map map, bool clearEverything, ResolveParams rp)
        {
            if (clearEverything)
            {
                foreach (IntVec3 c in rp.rect)
                {
                    c.GetThingList(map).ToList().ForEach((t) => t.DeSpawn());
                    map.roofGrid.SetRoof(c, null);
                }
                map.roofGrid.RoofGridUpdate();
            }
            else
            {
                foreach (IntVec3 c in rp.rect)
                {
                    c.GetThingList(map).ToList().FindAll(t1 => t1.def.category == ThingCategory.Filth).ForEach((t) => t.DeSpawn());
                }
            }
        }

        private void AddHostilePawnGroup(Faction faction, Map map, ResolveParams rp)
        {
            Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map, null);
            TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
            ResolveParams resolveParams = rp;
            resolveParams.rect = rp.rect;
            resolveParams.faction = faction;
            resolveParams.singlePawnLord = singlePawnLord;
            resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement);
            resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms)));
            if (resolveParams.pawnGroupMakerParams == null && faction.def.pawnGroupMakers.Any(pgm => pgm.kindDef == PawnGroupKindDefOf.Settlement))
            {
                resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
                resolveParams.pawnGroupMakerParams.tile = map.Tile;
                resolveParams.pawnGroupMakerParams.faction = faction;
                resolveParams.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange);
                resolveParams.pawnGroupMakerParams.inhabitants = true;
                resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
            }
            if (faction.def.pawnGroupMakers.Any(pgm => pgm.kindDef == PawnGroupKindDefOf.Settlement)) BaseGen.symbolStack.Push("pawnGroup", resolveParams, null);
        }
        
        private void GenerateRooms(SettlementLayoutDef sld, Map map, ResolveParams rp)
        {
            List<StructureLayoutDef> authorizedLayouts = DefDatabase<StructureLayoutDef>.AllDefsListForReading.FindAll(s => s.tags.Any(t => sld.allowedTags.Contains(t)));
            Log.Message(authorizedLayouts.Count + " auhtorized layouts.");

            map.terrainGrid.SetTerrain(rp.rect.Last(), TerrainDefOf.Bridge);
        }

        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);

            if (FactionSettlement.tempUseStructureLayout)
            {
                this.AddHostilePawnGroup(faction, map, rp);

                ResolveParams usl_rp = rp;
                usl_rp.faction = faction;
                BaseGen.symbolStack.Push("kcsg_roomsgenfromstructure", usl_rp, null);

                this.PreClean(map, false, rp);
            }
            else
            {
                SettlementLayoutDef sld = DefDatabase<SettlementLayoutDef>.GetNamed(FactionSettlement.temp);

                this.PreClean(map, true, rp);
                if (sld.pathThing != null)
                {
                    ResolveParams sld_rp_1 = rp;
                    sld_rp_1.floorDef = sld.pathThing;
                    sld_rp_1.allowBridgeOnAnyImpassableTerrain = true;
                    BaseGen.symbolStack.Push("floor", sld_rp_1, null);
                }

                this.GenerateRooms(sld, map, rp);

                if (sld.vanillaLikeDefense)
                {
                    int dWidth = (Rand.Bool ? 2 : 4);
                    ResolveParams rp3 = rp;
                    rp3.rect = new CellRect(rp.rect.minX - dWidth, rp.rect.minZ - dWidth, rp.rect.Width + (dWidth * 2), rp.rect.Height + (dWidth * 2));
                    rp3.faction = faction;
                    rp3.edgeDefenseWidth = dWidth;
                    rp3.edgeThingMustReachMapEdge = new bool?(rp.edgeThingMustReachMapEdge ?? true);
                    BaseGen.symbolStack.Push("edgeDefense", rp3, null);
                }

                this.AddHostilePawnGroup(faction, map, rp);
            }
        }
    }
}