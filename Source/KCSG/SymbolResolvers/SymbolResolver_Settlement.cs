using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    internal class SymbolResolver_Settlement : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            CGO.currentGenStep = "Generating settlement";

            Map map = BaseGen.globalSettings.map;
            rp.faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);

            if (CGO.useStructureLayout)
            {
                this.HandleRuin(rp);
                this.AddHostilePawnGroup(rp.faction, map, rp);

                BaseGen.symbolStack.Push("kcsg_roomsgenfromstructure", rp, null);

                if (CGO.factionSettlement.preGenClear)
                    GenUtils.PreClean(map, rp.rect, CGO.structureLayoutDef.roofGrid, CGO.factionSettlement.fullClear);
            }
            else
            {
                this.HandleRuin(rp);
                this.AddHostilePawnGroup(rp.faction, map, rp);

                if (CGO.settlementLayoutDef.vanillaLikeDefense)
                {
                    int dWidth = (Rand.Bool ? 2 : 4);
                    ResolveParams rp3 = rp;
                    rp3.rect = new CellRect(rp.rect.minX - dWidth, rp.rect.minZ - dWidth, rp.rect.Width + (dWidth * 2), rp.rect.Height + (dWidth * 2));
                    rp3.faction = rp.faction;
                    rp3.edgeDefenseWidth = dWidth;
                    rp3.edgeThingMustReachMapEdge = new bool?(rp.edgeThingMustReachMapEdge ?? true);
                    BaseGen.symbolStack.Push("edgeDefense", rp3, null);
                }

                this.GenerateRooms(CGO.settlementLayoutDef, map, rp);

                if (CGO.factionSettlement.preGenClear)
                    GenUtils.PreClean(map, rp.rect, CGO.factionSettlement.fullClear);
            }
        }

        private void AddHostilePawnGroup(Faction faction, Map map, ResolveParams rp)
        {
            Lord singlePawnLord;
            if (faction.def.pawnGroupMakers.Any(pgm => pgm.options.Any(k => !k.kind.RaceProps.EatsFood)))
                singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBaseNoEat(faction, rp.rect.CenterCell), map, null);
            else
                singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map, null);

            TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
            ResolveParams resolveParams = rp;
            resolveParams.rect = rp.rect;
            resolveParams.faction = faction;
            resolveParams.singlePawnLord = singlePawnLord;
            resolveParams.pawnGroupKindDef = CGO.settlementLayoutDef?.groupKindDef ?? rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement;
            resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms)));
            if (resolveParams.pawnGroupMakerParams == null && faction.def.pawnGroupMakers.Any(pgm => pgm.kindDef == PawnGroupKindDefOf.Settlement))
            {
                resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms
                {
                    tile = map.Tile,
                    faction = faction,
                    points = rp.settlementPawnGroupPoints ?? RimWorld.BaseGen.SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange,
                    inhabitants = true,
                    seed = rp.settlementPawnGroupSeed
                };
            }
            if (CGO.settlementLayoutDef != null) resolveParams.pawnGroupMakerParams.points *= CGO.settlementLayoutDef.pawnGroupMultiplier;
            if (faction.def.pawnGroupMakers.Any(pgm => pgm.kindDef == PawnGroupKindDefOf.Settlement)) BaseGen.symbolStack.Push("pawnGroup", resolveParams, null);
        }

        private void HandleRuin(ResolveParams rp)
        {
            CGO.currentGenStep = "";
            CGO.currentGenStepMoreInfo = "";
            if (CGO.factionSettlement.shouldRuin)
            {
                foreach (string resolver in CGO.factionSettlement.ruinSymbolResolvers)
                {
                    if (!(CGO.factionSettlement.ruinSymbolResolvers.Contains("kcsg_randomroofremoval") && resolver == "kcsg_scatterstuffaround"))
                        BaseGen.symbolStack.Push(resolver, rp, null);
                }
            }
        }

        private void GenerateRooms(SettlementLayoutDef sld, Map map, ResolveParams rp)
        {
            int seed = new Random().Next(0, 100000);

            CGO.offset = rp.rect.Corners.ElementAt(2);
            CGO.grid = GridUtils.GenerateGrid(seed, sld, map);

            BaseGen.symbolStack.Push("kcsg_roomgenfromlist", rp, null);
        }
    }
}