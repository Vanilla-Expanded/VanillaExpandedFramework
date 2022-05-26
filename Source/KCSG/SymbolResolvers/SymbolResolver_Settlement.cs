using System;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    internal class SymbolResolver_Settlement : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            GenOption.currentGenStep = "Generating settlement";

            Map map = BaseGen.globalSettings.map;
            rp.faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);

            if (GenOption.ext.useStructureLayout)
            {
                AddHostilePawnGroup(rp.faction, map, rp);

                BaseGen.symbolStack.Push("kcsg_roomsgenfromstructure", rp, null);
            }
            else
            {
                AddHostilePawnGroup(rp.faction, map, rp);

                if (GenOption.settlementLayoutDef.vanillaLikeDefense)
                {
                    int dWidth = Rand.Bool ? 2 : 4;
                    ResolveParams edgeParms = rp;
                    edgeParms.rect = new CellRect(rp.rect.minX - dWidth, rp.rect.minZ - dWidth, rp.rect.Width + (dWidth * 2), rp.rect.Height + (dWidth * 2));
                    edgeParms.faction = rp.faction;
                    edgeParms.edgeDefenseWidth = dWidth;
                    edgeParms.edgeThingMustReachMapEdge = new bool?(rp.edgeThingMustReachMapEdge ?? true);
                    BaseGen.symbolStack.Push("edgeDefense", edgeParms, null);
                }

                int seed = new Random().Next(0, 100000);

                GenOption.offset = rp.rect.Corners.ElementAt(2);
                GenOption.grid = GridUtils.GenerateGrid(seed, GenOption.settlementLayoutDef, map);

                BaseGen.symbolStack.Push("kcsg_roomgenfromlist", rp, null);

                GenUtils.SetRoadInfo(map);
            }
        }

        private void AddHostilePawnGroup(Faction faction, Map map, ResolveParams parms)
        {
            Lord singlePawnLord;
            if (faction.def.pawnGroupMakers.Any(pgm => pgm.options.Any(k => !k.kind.RaceProps.EatsFood)))
                singlePawnLord = parms.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBaseNoEat(faction, parms.rect.CenterCell), map, null);
            else
                singlePawnLord = parms.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, parms.rect.CenterCell), map, null);

            TraverseParms tp = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
            ResolveParams rp = parms;
            rp.rect = parms.rect;
            rp.faction = faction;
            rp.singlePawnLord = singlePawnLord;
            rp.pawnGroupKindDef = GenOption.settlementLayoutDef?.groupKindDef ?? parms.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement;
            rp.singlePawnSpawnCellExtraPredicate = parms.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => map.reachability.CanReachMapEdge(x, tp));
            if (rp.pawnGroupMakerParams == null && faction.def.pawnGroupMakers.Any(pgm => pgm.kindDef == PawnGroupKindDefOf.Settlement))
            {
                rp.pawnGroupMakerParams = new PawnGroupMakerParms
                {
                    tile = map.Tile,
                    faction = faction,
                    points = parms.settlementPawnGroupPoints ?? RimWorld.BaseGen.SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange,
                    inhabitants = true,
                    seed = parms.settlementPawnGroupSeed
                };
            }
            if (GenOption.settlementLayoutDef != null) rp.pawnGroupMakerParams.points *= GenOption.settlementLayoutDef.pawnGroupMultiplier;

            BaseGen.symbolStack.Push("pawnGroup", rp, null);
        }
    }
}