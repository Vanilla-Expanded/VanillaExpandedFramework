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

        private void DrawXMainRoad(CustomVector[][] grid, int mapWidth, int mapHeight, int borderDist, Random r)
        {
            CustomVector v1 = new CustomVector(0, r.Next(borderDist, mapHeight - borderDist));
            CustomVector v2 = new CustomVector(mapWidth - 1, r.Next(borderDist, mapHeight - borderDist));
            List<CustomVector> all = AStar.Run(v1, v2, grid, true);
            all.Add(v1);

            double y;
            for (int i = 0; i < all.Count; i++)
            {
                CustomVector v = all[i];
                y = i + 1 < all.Count ? all[i + 1].Y : v.Y;
                grid[(int)v.X][(int)v.Y].Type = CellType.MAINROAD;
                if (v.Y == y)
                {
                    grid[(int)v.X][(int)v.Y - 1].Type = CellType.MAINROAD;
                    grid[(int)v.X][(int)v.Y + 1].Type = CellType.MAINROAD;
                }
                else if (v.Y < y)
                {
                    grid[(int)v.X][(int)v.Y - 1].Type = CellType.MAINROAD;
                    grid[(int)v.X][(int)v.Y + 1].Type = CellType.MAINROAD;
                    grid[(int)v.X][(int)v.Y + 2].Type = CellType.MAINROAD;
                }
                else if (v.Y > y)
                {
                    grid[(int)v.X][(int)v.Y - 2].Type = CellType.MAINROAD;
                    grid[(int)v.X][(int)v.Y - 1].Type = CellType.MAINROAD;
                    grid[(int)v.X][(int)v.Y + 1].Type = CellType.MAINROAD;
                }
            }
        }

        private void DrawYMainRoad(CustomVector[][] grid, int mapWidth, int mapHeight, int borderDist, Random r)
        {
            CustomVector v1 = new CustomVector(r.Next(borderDist, mapWidth - borderDist), 0);
            CustomVector v2 = new CustomVector(r.Next(borderDist, mapWidth - borderDist), mapHeight - 1);
            List<CustomVector> all = AStar.Run(v1, v2, grid, true);
            all.Add(v1);

            double x;
            for (int i = 0; i < all.Count; i++)
            {
                CustomVector v = all[i];
                x = i + 1 < all.Count ? all[i + 1].X : v.X;
                grid[(int)v.X][(int)v.Y].Type = CellType.MAINROAD;
                if (v.X == x)
                {
                    grid[(int)v.X - 1][(int)v.Y].Type = CellType.MAINROAD;
                    grid[(int)v.X + 1][(int)v.Y].Type = CellType.MAINROAD;
                }
                else if (v.X < x)
                {
                    grid[(int)v.X - 1][(int)v.Y].Type = CellType.MAINROAD;
                    grid[(int)v.X + 1][(int)v.Y].Type = CellType.MAINROAD;
                    grid[(int)v.X + 2][(int)v.Y].Type = CellType.MAINROAD;
                }
                else if (v.X > x)
                {
                    grid[(int)v.X - 2][(int)v.Y].Type = CellType.MAINROAD;
                    grid[(int)v.X - 1][(int)v.Y].Type = CellType.MAINROAD;
                    grid[(int)v.X + 1][(int)v.Y].Type = CellType.MAINROAD;
                }
            }
        }

        private CustomVector[][] GenerateGrid(int seed, SettlementLayoutDef sld, out Dictionary<CustomVector, StructureLayoutDef> vectStruct)
        {
            int mapWidth = sld.settlementSize.x,
                mapHeight = sld.settlementSize.z,
                maxTries = 50,
                radius = 9999;
            // layout choice and radius
            List<StructureLayoutDef> allowed = DefDatabase<StructureLayoutDef>.AllDefsListForReading.FindAll(s => s.tags.Any(t => sld.allowedTags.Contains(t)));
            for (int i = 0; i < allowed.Count; i++)
            {
                RectUtils.HeightWidthFromLayout(allowed[i], out int height, out int width);
                if (height < radius)
                    radius = height;
                if (width < radius)
                    radius = width;
            }
            // Init
            Random r = new Random(seed);
            CustomVector[][] grid = new CustomVector[mapWidth][];
            for (int i = 0; i < mapWidth; i++)
            {
                grid[i] = new CustomVector[mapHeight];
                for (int j = 0; j < mapHeight; j++)
                {
                    grid[i][j] = new CustomVector(i, j);
                }
            }
            // Main road
            DrawXMainRoad(grid, mapWidth, mapHeight, 15, r);
            DrawYMainRoad(grid, mapWidth, mapHeight, 15, r);
            for (int i = 0; i < mapWidth / 100; i++)
            {
                DrawXMainRoad(grid, mapWidth, mapHeight, 15, r);
                DrawYMainRoad(grid, mapWidth, mapHeight, 15, r);
            }
            // Buildings
            List<CustomVector> vectors = PoissonDiskSampling.Run(radius + 1, maxTries, mapWidth, mapHeight, r, grid);
            List<CustomVector> doors = BuildingPlacement.Run(allowed, grid, vectors, maxTries, r, out vectStruct);
            Log.Message($"Door number: {doors.Count}");
            doors.ForEach(d => Log.Message($"{d}"));
            // Delaunay
            List<Triangle> triangulation = /*new List<Triangle>(); */Delaunay.Run(doors, mapWidth, mapHeight).ToList();
            Log.Message($"Triangle number: {triangulation.Count}");
            List<Edge> edges = new List<Edge>();
            foreach (Triangle triangle in triangulation)
            {
                edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
            }
            // A*
            foreach (Edge ed in edges)
            {
                if (ed != null && ed.Point1 != null && ed.Point2 != null)
                {
                    Log.Message($"Edge: {ed.Point1} {ed.Point2}");
                    foreach (CustomVector v in AStar.Run(ed.Point1, ed.Point2, grid, false))
                    {
                        if (v != null)
                        {
                            v.Type = v.Type == CellType.NONE ? CellType.ROAD : v.Type;
                        }
                    }
                }
            }

            return grid;
        }

        private void GenerateRooms(SettlementLayoutDef sld, Map map, ResolveParams rp)
        {
            DateTime startTime = DateTime.Now;
            Log.Message($"Starting generation - {startTime.ToShortTimeString()}");
            int seed = new Random().Next(0, 100000),
                x = rp.rect.Corners.ElementAt(2).x,
                y = rp.rect.Corners.ElementAt(2).z;
            CurrentGenerationOption.offset = rp.rect.Corners.ElementAt(2);

            CustomVector[][] grid = GenerateGrid(seed, sld, out CurrentGenerationOption.vectStruct);

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
                            GenUtils.GenerateTerrainAt(map, cell, TerrainDefOf.Concrete);
                            break;

                        case CellType.MAINROAD:
                            GenUtils.GenerateTerrainAt(map, cell, TerrainDefOf.MetalTile);
                            break;

                        case CellType.DOOR:
                            cell.GetFirstBuilding(map)?.DeSpawn();
                            GenUtils.GenerateTerrainAt(map, cell, TerrainDefOf.PavedTile);
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