using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public struct CurrentGenerationOption
    {
        public static IntVec2 gridStartPoint;

        public static SettlementLayoutDef settlementLayoutDef;

        public static StructureLayoutDef structureLayoutDef;

        public static bool useStructureLayout;

        public static Dictionary<CustomVector, StructureLayoutDef> vectStruct;

        public static IntVec3 offset;

        public static void ClearAll()
        {
            structureLayoutDef = null;
            settlementLayoutDef = null;
            useStructureLayout = false;
            vectStruct = null;
            gridStartPoint = IntVec2.Invalid;
            offset = IntVec3.Invalid;
        }
    }
}