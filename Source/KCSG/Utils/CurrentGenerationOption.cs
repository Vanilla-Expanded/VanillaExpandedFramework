using KCSG.Utils;
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

        public static Dictionary<KVector, StructureLayoutDef> vectStruct;

        public static void ClearAll()
        {
            structureLayoutDef = null;
            settlementLayoutDef = null;
            useStructureLayout = false;
            vectStruct = null;
            gridStartPoint = IntVec2.Invalid;
        }
    }
}