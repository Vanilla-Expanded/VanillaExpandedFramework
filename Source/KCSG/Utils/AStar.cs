using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KCSG.Utils
{
    public static class AStar
    {
        public static int gridCols;
        public static int gridRows;

        public static List<KVector> Run(KVector start, KVector end, KVector[][] grid, bool neig8)
        {
            gridCols = grid.Count();
            gridRows = grid[0].Count();

            List<KVector> Path = new List<KVector>();
            List<KVector> OpenList = new List<KVector>();
            List<KVector> ClosedList = new List<KVector>();
            List<KVector> adjacencies;
            KVector current = start;

            OpenList.Add(start);

            while (OpenList.Count > 0 && !ClosedList.Exists(v => v.X == end.X && v.Y == end.Y))
            {
                current = OpenList[0];
                OpenList.Remove(current);
                ClosedList.Add(current);
                adjacencies = GetAdjacentNodes(current, grid, neig8);

                foreach (KVector n in adjacencies)
                {
                    if (!ClosedList.Contains(n) && (n.type == Type.NONE || n.type == Type.ROAD || n.type == Type.DOOR || n.type == Type.MAINROAD))
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
                Console.WriteLine("AStar error: end point not found in ClosedList.");
                return null;
            }

            KVector temp = ClosedList[ClosedList.IndexOf(current)];
            if (temp == null)
            {
                Console.WriteLine("AStar error: temp is null.");
                return null;
            }

            while (temp != start && temp != null)
            {
                Path.Add(temp);
                temp = temp.Parent;
            }

            Log.Message($"Path lenght: {Path.Count}");
            return Path;
        }

        private static List<KVector> GetAdjacentNodes(KVector n, KVector[][] grid, bool neig8)
        {
            List<KVector> temp = new List<KVector>();

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
