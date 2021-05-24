using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KCSG
{
    class GridUtils
    {
        private static void DrawXMainRoad(CustomVector[][] grid, int mapWidth, int mapHeight, int borderDist, Random r)
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

        private static void DrawYMainRoad(CustomVector[][] grid, int mapWidth, int mapHeight, int borderDist, Random r)
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

        public static CustomVector[][] GenerateGrid(int seed, SettlementLayoutDef sld, Map map, out Dictionary<CustomVector, StructureLayoutDef> vectStruct)
        {
            CurrentGenerationOption.usePathCostReduction = false;
            int mapWidth = sld.settlementSize.x,
                mapHeight = sld.settlementSize.z,
                maxTries = 50,
                radius = 9999;

            // layout choice and radius
            for (int i = 0; i < sld.allowedStructures.Count; i++)
            {
                foreach (StructureLayoutDef item in DefDatabase<StructureLayoutDef>.AllDefsListForReading.FindAll(s => s.tags.Contains(sld.allowedStructuresConverted[i].structureLayoutTag)))
                {
                    RectUtils.HeightWidthFromLayout(item, out int height, out int width);
                    if (height < radius)
                        radius = height;
                    if (width < radius)
                        radius = width;
                }
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
            // Exclude non bridgeable
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    TerrainDef t = map.terrainGrid.TerrainAt(new IntVec3(CurrentGenerationOption.offset.x + i, 0, CurrentGenerationOption.offset.y + j));
                    if (t.HasTag("Water") && (t.affordances == null || !t.affordances.Contains(TerrainAffordanceDefOf.Bridgeable)))
                    {
                        Log.Message(t.defName);
                        grid[i][j].Type = CellType.WATER;
                    }
                        
                }
            }
            // Main road
            DrawXMainRoad(grid, mapWidth, mapHeight, 15, r);
            DrawYMainRoad(grid, mapWidth, mapHeight, 15, r);
            for (int i = 0; i < mapWidth / 100; i++)
            {
                if (i == 0)
                {
                    DrawXMainRoad(grid, mapWidth, mapHeight, mapWidth / 2, r);
                    DrawYMainRoad(grid, mapWidth, mapHeight, mapHeight / 2, r);
                }
                else
                {
                    DrawXMainRoad(grid, mapWidth, mapHeight, 50, r);
                    DrawYMainRoad(grid, mapWidth, mapHeight, 50, r);
                }
            }
            CurrentGenerationOption.usePathCostReduction = true;
            // Buildings
            List<CustomVector> vectors = PoissonDiskSampling.Run(radius + 1, maxTries, mapWidth, mapHeight, r, grid);
            List<CustomVector> doors = BuildingPlacement.Run(sld, grid, vectors, maxTries, r, out vectStruct);
            // Delaunay
            List<Triangle> triangulation = Delaunay.Run(doors, mapWidth, mapHeight).ToList();
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
                    List<CustomVector> astar = AStar.Run(ed.Point1, ed.Point2, grid, false);
                    if (astar != null)
                    {
                        foreach (CustomVector v in astar)
                        {
                            if (v != null)
                            {
                                v.Type = v.Type == CellType.NONE ? CellType.ROAD : v.Type;
                            }
                        }
                    }
                }
            }

            return grid;
        }
    }
}
