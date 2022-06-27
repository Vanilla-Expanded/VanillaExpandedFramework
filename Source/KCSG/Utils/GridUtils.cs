namespace KCSG
{
    internal class GridUtils
    {
        /*/// <summary>
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
        public static void AddRoadToGrid(Map map, ResolveParams rp)
        {
            int mapWidth = GenOption.settlementLayoutDef.settlementSize.x;
            int mapHeight = GenOption.settlementLayoutDef.settlementSize.z;
            // Delaunay to find neigbours doors to path to, and ensure each doors will have a road
            GenOption.extendedInfo = "Running Delaunay algorithm";
            List<Triangle> triangulation = Delaunay.Run(GenOption.doors, mapWidth, mapHeight).ToList();
            List<Edge> edges = new List<Edge>();
            foreach (Triangle triangle in triangulation)
            {
                edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
            }
            GenOption.extendedInfo = "Running A* algorithm";
            foreach (Edge ed in edges)
            {
                if (ed != null && ed.Point1 != null && ed.Point2 != null)
                {
                    List<CustomVector> astar = AStar.Run(ed.Point1, ed.Point2, GenOption.grid, false);
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
        }*/
    }
}