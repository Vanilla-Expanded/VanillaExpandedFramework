using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public struct CurrentGenerationOption
    {
        public static CustomVector[][] grid;

        public static IntVec2 gridStartPoint;

        public static IntVec3 offset;

        public static SettlementLayoutDef settlementLayoutDef;

        public static StructureLayoutDef structureLayoutDef;

        public static Dictionary<CustomVector, StructureLayoutDef> vectStruct;

        public static List<CustomVector> doors;

        public static bool useStructureLayout;

        public static bool usePathCostReduction;

        public static void ClearAll()
        {
            grid = null;

            gridStartPoint = IntVec2.Invalid;
            offset = IntVec3.Invalid;

            settlementLayoutDef = null;
            structureLayoutDef = null;
            vectStruct = null;
            doors = null;
            
            useStructureLayout = false;
            usePathCostReduction = false;
        }
    }
}