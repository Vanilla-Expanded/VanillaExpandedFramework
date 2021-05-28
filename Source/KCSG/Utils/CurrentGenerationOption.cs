using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public struct CurrentGenerationOption
    {
        public static CustomVector[][] grid;

        public static IntVec2 gridStartPoint;

        public static IntVec3 offset;

        public static int radius;

        public static CellRect fullRect;

        public static SettlementLayoutDef settlementLayoutDef;

        public static StructureLayoutDef structureLayoutDef;

        public static Dictionary<CustomVector, StructureLayoutDef> vectStruct;

        public static List<CustomVector> doors;

        public static List<CustomVector> vectors;

        public static List<TerrainDef> preRoadTypes;

        public static bool useStructureLayout;

        public static bool usePathCostReduction;

        public static void ClearAll()
        {
            grid = null;

            gridStartPoint = IntVec2.Invalid;
            offset = IntVec3.Invalid;
            fullRect = CellRect.Empty;

            settlementLayoutDef = null;
            structureLayoutDef = null;
            vectStruct = null;
            doors = null;
            vectors = null;
            preRoadTypes = null;
            
            useStructureLayout = false;
            usePathCostReduction = false;
        }
    }
}