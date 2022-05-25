using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public static class BuildingPlacement
    {
        public static bool CanPlaceAt(CustomVector point, StructureLayoutDef building, CustomVector[][] grid)
        {
            bool result = true;
            for (int i = (int)point.X - 1; i < building.width + point.X + 1 && result; i++)
            {
                for (int j = (int)(point.Y - 1); j < building.height + point.Y + 1 && result; j++)
                {
                    if (IsInBound(i, j, grid.Length, grid[0].Length))
                    {
                        if (grid[i][j] != null && grid[i][j].Type != CellType.NONE)
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return result;
        }

        public static float GetWeight(StructOption structOption, Dictionary<string, int> structCount)
        {
            bool containKey = structCount.ContainsKey(structOption.tag);
            if (containKey)
            {
                int count = structCount.TryGetValue(structOption.tag);
                if (count < structOption.count.min)
                {
                    return 2f;
                }
                else if (structOption.count.max != -1 && count == structOption.count.max)
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
            List<CustomVector> doors = GetDoorsInLayout(building);
            List<CustomVector> doorsAdjusted = new List<CustomVector>();

            for (int i = (int)point.X; i < building.width + point.X; i++)
            {
                for (int j = (int)point.Y; j < building.height + point.Y; j++)
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
            CGO.vectStruct = new Dictionary<CustomVector, StructureLayoutDef>();

            Dictionary<string, int> structCount = new Dictionary<string, int>();

            List<CustomVector> doors = new List<CustomVector>();
            foreach (CustomVector vector in CGO.vectors.ListFullCopy())
            {
                for (int i = 0; i < maxTries; i++)
                {
                    sld.allowedStructures.TryRandomElementByWeight(p => GetWeight(p, structCount), out StructOption option);
                    List<StructureLayoutDef> all = DefDatabase<StructureLayoutDef>.AllDefsListForReading.FindAll(s => s.tags.Contains(option.tag));
                    StructureLayoutDef b = LayoutUtils.ChooseLayoutFrom(all);

                    if (CanPlaceAt(vector, b, grid))
                    {
                        CGO.vectStruct.Add(vector, b);
                        doors.AddRange(PlaceAt(vector, b, grid));
                        if (structCount.ContainsKey(option.tag))
                        {
                            structCount[option.tag]++;
                        }
                        else
                        {
                            structCount.Add(option.tag, 1);
                        }
                        CGO.vectors.Remove(vector);
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
                            SymbolDef tempS = DefDatabase<SymbolDef>.GetNamedSilentFail(array[col]);
                            if (tempS != null && tempS.thingDef != null && tempS.thingDef.altitudeLayer == AltitudeLayer.DoorMoveable)
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