using System;
using System.Collections.Generic;
using Verse;

namespace KCSG.Utils
{
    public static class BuildingPlacement
    {
        public static bool CanPlaceAt(KVector point, StructureLayoutDef building, KVector[][] grid)
        {
            bool result = true;
            KCSG_Utilities.HeightWidthFromLayout(building, out int height, out int width);

            for (int i = (int)point.X - 1; i < width + point.X + 1 && result; i++)
            {
                for (int j = (int)(point.Y - 1); j < height + point.Y + 1 && result; j++)
                {
                    if (IsInBound(i, j, grid.Length, grid[0].Length))
                    {
                        if (grid[i][j] != null && grid[i][j].type != Type.NONE)
                            return false;
                    }
                }
            }

            if (!IsInBound((int)(width + point.X), (int)(height + point.Y), grid.Length, grid[0].Length))
                return false;

            return result;
        }

        public static KVector PlaceAt(KVector point, StructureLayoutDef building, KVector[][] grid)
        {
            KVector result = new KVector(0, 0);
            KCSG_Utilities.HeightWidthFromLayout(building, out int height, out int width);
            KVector door = GetDoorInLayout(building);

            for (int i = (int)point.X; i < width + point.X; i++)
            {
                for (int j = (int)point.Y; j < height + point.Y; j++)
                {
                    Type type = i == door.X + point.X && j == door.Y + point.Y ? Type.DOOR : Type.BUILDING;
                    if (type == Type.DOOR)
                    {
                        result.X = i;
                        result.Y = j;
                    }

                    if (grid[i][j] != null)
                        grid[i][j].type = type;
                    else
                        grid[i][j] = new KVector(i, j, type: type);
                }
            }

            return result;
        }

        public static List<KVector> Run(List<StructureLayoutDef> buildingsToChooseFrom, KVector[][] grid, List<KVector> points, int maxTries, Random r, out Dictionary<KVector, StructureLayoutDef> vectStruct)
        {
            List<KVector> doors = new List<KVector>();
            vectStruct = new Dictionary<KVector, StructureLayoutDef>();
            foreach (KVector vector in points)
            {
                for (int i = 0; i < maxTries; i++)
                {
                    StructureLayoutDef b = buildingsToChooseFrom[r.Next(buildingsToChooseFrom.Count)];
                    if (CanPlaceAt(vector, b, grid))
                    {
                        vectStruct.Add(vector, b);
                        doors.Add(PlaceAt(vector, b, grid));
                        break;
                    }
                }
            }
            return doors;
        }

        private static bool IsInBound(int X, int Y, int gridWidth, int gridHeight)
        {
            if (X < 0)
                return false;
            if (X >= gridWidth)
                return false;
            if (Y < 0)
                return false;
            if (Y >= gridHeight)
                return false;
            return true;
        }

        private static KVector GetDoorInLayout(StructureLayoutDef building)
        {
            KCSG_Utilities.HeightWidthFromLayout(building, out int height, out int width);

            SymbolDef sy = null;
            List<string> la = null;
            foreach (List<string> layout in building.layouts)
            {
                List<string> allSymbList = new List<string>();
                layout.ForEach(s => allSymbList.AddRange(s.Split(',')));
                foreach (string s in allSymbList)
                {
                    sy = DefDatabase<SymbolDef>.AllDefsListForReading.Find(symb => symb.symbol == s);
                    if (sy?.thing != null && DefDatabase<ThingDef>.AllDefsListForReading.Find(t => t.defName == sy.thing) is ThingDef thing && thing.altitudeLayer == AltitudeLayer.DoorMoveable)
                    {
                        break;
                    }
                }
                if (sy != null)
                {
                    la = allSymbList.ListFullCopy();
                    break;
                }
            }

            if (sy == null)
                Log.Error("Something when wrong in: BuildingPlacement -> GetDoorInLayout");

            for (int i = 0; i < width - 1; i++)
            {
                string[] temp = la[i].Split(',');
                for (int j = 0; j < height - 1; j++)
                {
                    if ((i == 0 || (width - 1 > 0 && i == width - 1)) && (j == 0 || (height - 1 > 0 && j == height - 1)) && temp[j] == sy.defName)
                    {
                        return new KVector(i, j);
                    }
                }
            }

            return new KVector(-1, -1);
        }
    }

    /*public static class BuildingPlacement
    {
        public static bool CanPlaceAt(KVector point, int[] building, KVector[][] grid)
        {
            bool result = true;
            for (int i = (int)point.X - 1; i < building[0] + point.X + 1 && result; i++)
            {
                for (int j = (int)(point.Y - 1); j < building[1] + point.Y + 1 && result; j++)
                {
                    if (IsInBound(i, j, grid.Length, grid[0].Length))
                    {
                        if (grid[i][j] != null && grid[i][j].type != Type.NONE)
                            return false;
                    }
                }
            }

            if (!IsInBound((int)(building[0] + point.X), (int)(building[1] + point.Y), grid.Length, grid[0].Length))
                return false;

            return result;
        }

        public static KVector PlaceAt(KVector point, int[] building, KVector[][] grid)
        {
            KVector result = new KVector(0, 0);
            for (int i = (int)point.X; i < building[0] + point.X; i++)
            {
                for (int j = (int)point.Y; j < building[1] + point.Y; j++)
                {
                    Type type = i == building[2] + point.X && j == building[3] + point.Y ? Type.DOOR : Type.BUILDING;
                    if (type == Type.DOOR)
                    {
                        result.X = i;
                        result.Y = j;
                    }

                    if (grid[i][j] != null)
                        grid[i][j].type = type;
                    else
                        grid[i][j] = new KVector(i, j, type: type);
                }
            }

            return result;
        }

        public static List<KVector> Run(List<int[]> buildingsToChooseFrom, KVector[][] grid, List<KVector> points, int maxTries, Random r)
        {
            DateTime dateTime = DateTime.Now;

            List<KVector> doors = new List<KVector>();
            foreach (KVector vector in points)
            {
                for (int i = 0; i < maxTries; i++)
                {
                    int[] b = buildingsToChooseFrom[r.Next(buildingsToChooseFrom.Count)];
                    if (CanPlaceAt(vector, b, grid))
                    {
                        doors.Add(PlaceAt(vector, b, grid));
                        break;
                    }
                }
            }
            Console.WriteLine($"BuildingPlacing completed in {(DateTime.Now - dateTime).TotalMilliseconds} ms");
            return doors;
        }
        private static bool IsInBound(int X, int Y, int gridWidth, int gridHeight)
        {
            if (X < 0)
                return false;
            if (X >= gridWidth)
                return false;
            if (Y < 0)
                return false;
            if (Y >= gridHeight)
                return false;
            return true;
        }
    }*/
}