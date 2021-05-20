using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace KCSG
{
    internal class SymbolResolver_KCSG_Settlement : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            Faction faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);

            if (CurrentGenerationOption.useStructureLayout)
            {
                this.AddHostilePawnGroup(faction, map, rp);

                ResolveParams usl_rp = rp;
                usl_rp.faction = faction;
                BaseGen.symbolStack.Push("kcsg_roomsgenfromstructure", usl_rp, null);

                GenUtils.PreClean(map, rp.rect);
            }
            else
            {
                SettlementLayoutDef sld = CurrentGenerationOption.settlementLayoutDef;

                this.AddHostilePawnGroup(faction, map, rp);

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

                this.GenerateRooms(sld, map, rp);

                GenUtils.PreClean(map, rp.rect);
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
            DateTime startTime = DateTime.Now;
            Log.Message($"Starting generation - {startTime.ToShortTimeString()}");
            int seed = new Random().Next(0, 100000),
                x = rp.rect.Corners.ElementAt(2).x,
                y = rp.rect.Corners.ElementAt(2).z;
            CurrentGenerationOption.offset = rp.rect.Corners.ElementAt(2);

            CustomVector[][] grid = GridUtils.GenerateGrid(seed, sld, map, out CurrentGenerationOption.vectStruct);

            ResolveParams usl_rp = rp;
            usl_rp.faction = rp.faction;
            BaseGen.symbolStack.Push("kcsg_roomgenfromlist", usl_rp, null);

            for (int i = 0; i < grid.Length; i++)
            {
                for (int j = 0; j < grid[i].Length; j++)
                {
                    IntVec3 cell = new IntVec3(x + i, 0, y - j);
                    switch (grid[i][j].Type)
                    {
                        case CellType.ROAD:
                            GenUtils.GenerateTerrainAt(map, cell, sld.roadDef);
                            break;

                        case CellType.MAINROAD:
                            GenUtils.GenerateTerrainAt(map, cell, sld.mainRoadDef);
                            break;

                        default:
                            break;
                    }
                }
            }

            Log.Message($"Generation stopped - {DateTime.Now.ToShortTimeString()} - Time taken {(DateTime.Now - startTime).TotalMilliseconds} ms - Seed was {seed}.");
        }
    }
}