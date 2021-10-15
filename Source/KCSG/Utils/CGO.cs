using System;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public struct CGO
    {
        /*-------- Loading screen --------*/
        public static bool useCustomWindowContent;

        public static DateTime dateTime;

        public static string currentGenStep;

        public static string currentGenStepMoreInfo;

        public static List<string> allTip;

        public static bool tipAvailable;

        /*------ Generation options ------*/
        public static CustomVector[][] grid;

        public static IntVec3 offset;

        public static int radius;

        public static SettlementLayoutDef settlementLayoutDef;

        public static StructureLayoutDef structureLayoutDef;

        public static Dictionary<CustomVector, StructureLayoutDef> vectStruct;

        public static List<CustomVector> doors;

        public static List<CustomVector> vectors;

        public static List<TerrainDef> preRoadTypes;

        public static bool useStructureLayout;

        public static bool usePathCostReduction;

        public static CustomGenOption factionSettlement;

        /*------ Falling structure ------*/
        public static FallingStructure fallingStructure;

        public static StructureLayoutDef fallingStructureChoosen;

        public static void ClearFalling()
        {
            fallingStructure = null;
            fallingStructureChoosen = null;
        }

        public static void ClearUI()
        {
            useCustomWindowContent = false;
            dateTime = default;
            currentGenStep = "";
            currentGenStepMoreInfo = "";
            UIMenuBackgroundManager.background = null;
        }

        public static void ClearAll()
        {
            grid = null;
            factionSettlement = null;
            if (vectStruct != null) vectStruct.Clear();
            if (doors != null) doors.Clear();
            if (vectors != null) vectors.Clear();
            if (preRoadTypes != null) preRoadTypes.Clear();
            useStructureLayout = false;
            usePathCostReduction = false;
        }
    }
}