using System;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public static class BuildingPlacement
    {
        public static bool CanPlaceAt(CustomVector point, StructureLayoutDef building, CustomVector[][] grid)
        {
            bool result = true;
            RectUtils.HeightWidthFromLayout(building, out int height, out int width);

            for (int i = (int)point.X - 1; i < width + point.X + 1 && result; i++)
            {
                for (int j = (int)(point.Y - 1); j < height + point.Y + 1 && result; j++)
                {
                    if (IsInBound(i, j, grid.Length, grid[0].Length))
                    {
                        if (grid[i][j] != null && grid[i][j].Type != CellType.NONE)
                            return false;
                    }
                }
            }

            if (!IsInBound((int)(width + point.X), (int)(height + point.Y), grid.Length, grid[0].Length))
                return false;

            return result;
        }

        public static CustomVector PlaceAt(CustomVector point, StructureLayoutDef building, CustomVector[][] grid)
        {
            CustomVector result = new CustomVector(0, 0);
            RectUtils.HeightWidthFromLayout(building, out int height, out int width);
            CustomVector door = GetDoorInLayout(building);

            for (int i = (int)point.X; i < width + point.X; i++)
            {
                for (int j = (int)point.Y; j < height + point.Y; j++)
                {
                    CellType type = i == door.X + point.X && j == door.Y + point.Y ? CellType.DOOR : CellType.BUILDING;
                    if (type == CellType.DOOR)
                    {
                        result.X = i;
                        result.Y = j;
                    }

                    if (grid[i][j] != null)
                        grid[i][j].Type = type;
                    else
                        grid[i][j] = new CustomVector(i, j, type: type);
                }
            }

            return result;
        }

        public static List<CustomVector> Run(List<StructureLayoutDef> buildingsToChooseFrom, CustomVector[][] grid, List<CustomVector> points, int maxTries, Random r, out Dictionary<CustomVector, StructureLayoutDef> vectStruct)
        {
            vectStruct = new Dictionary<CustomVector, StructureLayoutDef>();
            List<CustomVector> doors = new List<CustomVector>();
            foreach (CustomVector vector in points)
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

        private static CustomVector GetDoorInLayout(StructureLayoutDef building)
        {
            foreach (List<string> layout in building.layouts)
            {
                int lineN = 0;

                List<string> rLayout = layout.ListFullCopy();
                rLayout.Reverse();

                foreach (string str in rLayout)
                {
                    string[] array = str.Split(',');
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] != ".")
                        {
                            SymbolDef tempS = DefDatabase<SymbolDef>.GetNamed(array[i]);
                            if (tempS.thingDef != null && tempS.thingDef.altitudeLayer == AltitudeLayer.DoorMoveable)
                            {
                                return new CustomVector(lineN, i);
                            }
                        }
                    }
                    lineN++;
                }
            }

            return new CustomVector(-1, -1);
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
    }
}