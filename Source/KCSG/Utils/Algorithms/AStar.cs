using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KCSG
{
    public static class AStar
    {
        public static int gridCols;
        public static int gridRows;

        public static List<CustomVector> Run(CustomVector start, CustomVector end, CustomVector[][] grid, bool neig8)
        {
            gridCols = grid.Count();
            gridRows = grid[0].Count();

            List<CustomVector> Path = new List<CustomVector>();
            List<CustomVector> OpenList = new List<CustomVector>();
            List<CustomVector> ClosedList = new List<CustomVector>();
            List<CustomVector> adjacencies;
            CustomVector current = start;

            OpenList.Add(start);

            while (OpenList.Count > 0 && !ClosedList.Exists(v => v.X == end.X && v.Y == end.Y))
            {               
                current = OpenList[0];
                OpenList.Remove(current);
                ClosedList.Add(current);
                adjacencies = GetAdjacentNodes(current, grid, neig8);

                foreach (CustomVector n in adjacencies)
                {
                    if (!ClosedList.Contains(n) && n.Type != CellType.WATER && n.Type != CellType.BUILDING)
                    {
                        if (!OpenList.Contains(n))
                        {
                            n.Parent = current;
                            n.DistanceToTarget = (float)(Math.Abs(n.X - end.X) + Math.Abs(n.Y - end.Y));
                            n.Cost = n.Weight + n.Parent.Cost;
                            OpenList.Add(n);
                            OpenList = OpenList.OrderBy(v => v.F).ToList();
                        }
                    }
                }
            }

            if (!ClosedList.Exists(v => v.X == end.X && v.Y == end.Y))
            {
                Log.Message("AStar error: end point not found in ClosedList.");
                return null;
            }

            CustomVector temp = ClosedList[ClosedList.IndexOf(current)];
            if (temp == null)
            {
                Log.Message("AStar error: temp is null.");
                return null;
            }

            while (temp != start && temp != null)
            {
                Path.Add(temp);
                temp = temp.Parent;
            }
            return Path;
        }

        private static List<CustomVector> GetAdjacentNodes(CustomVector n, CustomVector[][] grid, bool neig8)
        {
            List<CustomVector> temp = new List<CustomVector>();

            int row = (int)n.Y;
            int col = (int)n.X;

            if (row + 1 < gridRows)
            {
                temp.Add(grid[col][row + 1]);
            }
            if (row - 1 >= 0)
            {
                temp.Add(grid[col][row - 1]);
            }
            if (col - 1 >= 0)
            {
                temp.Add(grid[col - 1][row]);
            }
            if (col + 1 < gridCols)
            {
                temp.Add(grid[col + 1][row]);
            }

            if (neig8)
            {
                if (col - 1 >= 0 && row - 1 >= 0)
                {
                    temp.Add(grid[col - 1][row - 1]);
                }
                if (col + 1 < gridCols && row - 1 >= 0)
                {
                    temp.Add(grid[col + 1][row - 1]);
                }
                if (col - 1 >= 0 && row + 1 < gridRows)
                {
                    temp.Add(grid[col - 1][row + 1]);
                }
                if (col + 1 < gridCols && row + 1 < gridRows)
                {
                    temp.Add(grid[col + 1][row + 1]);
                }
            }

            return temp;
        }
    }
}