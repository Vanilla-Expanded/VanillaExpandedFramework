using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public class GenOption
    {
        public static StuffableOptions StuffableOptions
        {
            get
            {
                if (sld != null)
                    return sld.stuffableOptions;

                return null;
            }
        }

        public static RoadOptions RoadOptions => sld.roadOptions;
        public static PropsOptions PropsOptions => sld.propsOptions;


        public static CustomGenOption ext;

        public static SettlementLayoutDef sld;
        public static ThingDef generalWallStuff;
        public static List<IntVec3> usedSpots;

        public static StructureLayoutDef structureLayoutDef;

        /*------ Falling structure ------*/
        public static FallingStructure fallingStructure;
        public static StructureLayoutDef fallingStructureChoosen;

        public static void ClearFalling()
        {
            fallingStructure = null;
            fallingStructureChoosen = null;
        }
    }
}