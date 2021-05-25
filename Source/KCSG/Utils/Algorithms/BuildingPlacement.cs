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
                    else return false;
                }
            }
            return result;
        }

        public static CustomVector PlaceAt(CustomVector point, StructureLayoutDef building, CustomVector[][] grid)
        {
            CustomVector result = new CustomVector(0, 0);
            RectUtils.HeightWidthFromLayout(building, out int height, out int width);
            List<CustomVector> doors = GetDoorsInLayout(building);

            for (int i = (int)point.X; i < width + point.X; i++)
            {
                for (int j = (int)point.Y; j < height + point.Y; j++)
                {
                    CellType type = doors.FindAll(d => i == d.X + point.X && j == d.Y + point.Y).Any() ? CellType.DOOR : CellType.BUILDING;
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

        public static List<CustomVector> Run(SettlementLayoutDef sld, CustomVector[][] grid, List<CustomVector> points, int maxTries, Random r, out Dictionary<CustomVector, StructureLayoutDef> vectStruct)
        {
            vectStruct = new Dictionary<CustomVector, StructureLayoutDef>();

            Dictionary<string, int> structCount = new Dictionary<string, int>();
            
            List<CustomVector> doors = new List<CustomVector>();
            foreach (CustomVector vector in points)
            {
                for (int i = 0; i < maxTries; i++)
                {
                    sld.allowedStructuresConverted.TryRandomElementByWeight(p => GetWeight(p, structCount), out StructOption option);
                    StructureLayoutDef b = DefDatabase<StructureLayoutDef>.AllDefsListForReading.FindAll(s => s.tags.Contains(option.structureLayoutTag)).RandomElement();
                    if (CanPlaceAt(vector, b, grid))
                    {
                        vectStruct.Add(vector, b);
                        doors.Add(PlaceAt(vector, b, grid));
                        if (structCount.ContainsKey(option.structureLayoutTag))
                        {
                            structCount[option.structureLayoutTag]++;
                        }
                        else
                        {
                            structCount.Add(option.structureLayoutTag, 1);
                        }
                        CurrentGenerationOption.vectors.Remove(vector);
                        break;
                    }
                }
            }
            return doors;
        }

        public static float GetWeight(StructOption structOption, Dictionary<string, int> structCount)
        {
            bool containKey = structCount.ContainsKey(structOption.structureLayoutTag);
            if (containKey)
            {
                int count = structCount.TryGetValue(structOption.structureLayoutTag);
                if (count < structOption.minCount)
                {
                    return 2f;
                }
                else if (structOption.maxCount != -1 && count == structOption.maxCount)
                {
                    return 0f;
                }
                else
                {
                    return 1f;
                }
            }
            else
            {
                return 3f;
            }
        }

        private static List<CustomVector> GetDoorsInLayout(StructureLayoutDef building)
        {
            List<CustomVector> doors = new List<CustomVector>();
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
                                doors.Add(new CustomVector(i, lineN));
                            }
                        }
                    }
                    lineN++;
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
    }
}