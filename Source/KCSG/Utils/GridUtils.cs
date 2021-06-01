using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KCSG
{
    internal class GridUtils
    {
        /// <summary>
        /// Draw X road
        /// </summary>
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

        /// <summary>
        /// Draw Y road
        /// </summary>
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

        /// <summary>
        /// Run delaunay and astar
        /// </summary>
        /// <param name="grid">The grid in wich we generate roads</param>
        /// <param name="doors">List of doors, filled int AddRoadToGrid()</param>
        public static void AddRoadToGrid(CustomVector[][] grid, List<CustomVector> doors)
        {
            int mapWidth = CurrentGenerationOption.settlementLayoutDef.settlementSize.x,
                mapHeight = CurrentGenerationOption.settlementLayoutDef.settlementSize.z;
            // Delaunay to find neigbours doors to path to, and ensure each doors will have a road
            List<Triangle> triangulation = Delaunay.Run(doors, mapWidth, mapHeight).ToList();
            List<Edge> edges = new List<Edge>();
            foreach (Triangle triangle in triangulation)
            {
                edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
            }
            // A* to find the path in the grid
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
        }

        /// <summary>
        /// Generate a grid, add the preexisting vanilla road if needed. Add main roads. Fill the potential point list, add buildings on grid and fill the origin/layout dictionnary
        /// </summary>
        /// <param name="seed">Seed for Random</param>
        /// <param name="sld">SettlementLayoutDef to get all StructureLayoutDef authorized</param>
        /// <param name="map">The map</param>
        /// <returns></returns>
        public static CustomVector[][] GenerateGrid(int seed, SettlementLayoutDef sld, Map map)
        {
            int mapWidth = sld.settlementSize.x,
                mapHeight = sld.settlementSize.z;

            // Search for the smallest sized allowed structure. It will be the radius between potential building spawn points
            CurrentGenerationOption.radius = 9999;
            for (int i = 0; i < sld.allowedStructures.Count; i++)
            {
                foreach (StructureLayoutDef item in DefDatabase<StructureLayoutDef>.AllDefsListForReading.FindAll(s => s.tags.Contains(sld.allowedStructuresConverted[i].structureLayoutTag)))
                {
                    RectUtils.HeightWidthFromLayout(item, out int height, out int width);
                    if (height < CurrentGenerationOption.radius)
                        CurrentGenerationOption.radius = height;
                    if (width < CurrentGenerationOption.radius)
                        CurrentGenerationOption.radius = width;
                }
            }
            CurrentGenerationOption.radius += 2; // Add to radius to ensure no building touch one another

            // Initialize the grid used for generation
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

            // Exclude non bridgeable cells
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    TerrainDef t = map.terrainGrid.TerrainAt(new IntVec3(CurrentGenerationOption.offset.x + i, 0, CurrentGenerationOption.offset.y + j));
                    if (t.HasTag("Water") && (t.affordances == null || !t.affordances.Contains(TerrainAffordanceDefOf.Bridgeable)))
                    {
                        grid[i][j].Type = CellType.WATER;
                    }
                }
            }
            // Main roads
            CurrentGenerationOption.usePathCostReduction = false; // No path cost reduction for main road, for them to be distinct
            DrawXMainRoad(grid, mapWidth, mapHeight, 15, r);
            DrawYMainRoad(grid, mapWidth, mapHeight, 15, r);
            // If bigger sized settlement, add more main roads
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
            // Get vanilla world road type and use it if present
            if (CurrentGenerationOption.preRoadTypes?.Count > 0)
            {
                for (int i = 0; i < mapWidth; i++)
                {
                    for (int j = 0; j < mapHeight; j++)
                    {
                        TerrainDef t = map.terrainGrid.TerrainAt(new IntVec3(CurrentGenerationOption.offset.x + i, 0, CurrentGenerationOption.offset.y + j));
                        if (CurrentGenerationOption.preRoadTypes.Contains(t))
                        {
                            grid[i][j].Type = CellType.MAINROAD; // Add vanilla generated road to the grid
                        }
                    }
                }
            }
            CurrentGenerationOption.usePathCostReduction = true; // Renable path cost reduction

            CurrentGenerationOption.vectors = PoissonDiskSampling.Run(50, mapWidth, mapHeight, r, grid); // Get all possible points with radius
            CurrentGenerationOption.doors = BuildingPlacement.Run(sld, grid, 50); // Place buildings on grid and add them/their origin into vectStruct
            return grid;
        }
    }
}