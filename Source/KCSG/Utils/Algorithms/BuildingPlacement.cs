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

        public static List<CustomVector> PlaceAt(CustomVector point, StructureLayoutDef building, CustomVector[][] grid)
        {
            RectUtils.HeightWidthFromLayout(building, out int height, out int width);
            List<CustomVector> doors = GetDoorsInLayout(building);
            List<CustomVector> doorsAdjusted = new List<CustomVector>();

            for (int i = (int)point.X; i < width + point.X; i++)
            {
                for (int j = (int)point.Y; j < height + point.Y; j++)
                {
                    CellType type = doors.FindAll(d => i == d.X + point.X && j == d.Y + point.Y).Any() ? CellType.DOOR : CellType.BUILDING;
                    if (type == CellType.DOOR)
                    {
                        doorsAdjusted.Add(new CustomVector(i, j));
                    }

                    if (grid[i][j] != null)
                        grid[i][j].Type = type;
                    else
                        grid[i][j] = new CustomVector(i, j, type: type);
                }
            }

            return doorsAdjusted;
        }

        public static List<CustomVector> Run(SettlementLayoutDef sld, CustomVector[][] grid, int maxTries)
        {
            CurrentGenerationOption.vectStruct = new Dictionary<CustomVector, StructureLayoutDef>();

            Dictionary<string, int> structCount = new Dictionary<string, int>();

            List<CustomVector> doors = new List<CustomVector>();
            foreach (CustomVector vector in CurrentGenerationOption.vectors.ListFullCopy())
            {
                for (int i = 0; i < maxTries; i++)
                {
                    sld.allowedStructuresConverted.TryRandomElementByWeight(p => GetWeight(p, structCount), out StructOption option);
                    List<StructureLayoutDef> all = DefDatabase<StructureLayoutDef>.AllDefsListForReading.FindAll(s => s.tags.Contains(option.structureLayoutTag));
                    if (!ModLister.RoyaltyInstalled) all.RemoveAll(s => s.requireRoyalty);
                    StructureLayoutDef b = all.RandomElement();

                    if (CanPlaceAt(vector, b, grid))
                    {
                        CurrentGenerationOption.vectStruct.Add(vector, b);
                        doors.AddRange(PlaceAt(vector, b, grid));
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

        private static List<CustomVector> GetDoorsInLayout(StructureLayoutDef building)
        {
            List<CustomVector> doors = new List<CustomVector>();
            foreach (List<string> layout in building.layouts)
            {
                List<string> rLayout = layout.ListFullCopy();
                rLayout.Reverse();

                for (int row = 0; row < rLayout.Count; row++)
                {
                    string[] array = rLayout[row].Split(',');
                    for (int col = 0; col < array.Length; col++)
                    {
                        if (array[col] != ".")
                        {
                            SymbolDef tempS = DefDatabase<SymbolDef>.GetNamed(array[col]);
                            if (tempS.thingDef != null && tempS.thingDef.altitudeLayer == AltitudeLayer.DoorMoveable)
                            {
                                doors.Add(new CustomVector(col, row));
                            }
                        }
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
    }
}