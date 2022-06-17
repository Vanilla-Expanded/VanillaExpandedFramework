using Verse;

namespace KCSG
{
    public class GenOption
    {
        public static StuffableOptions StuffableOptions => sld.stuffableOptions;
        public static RoadOptions RoadOptions => sld.roadOptions;


        public static CustomGenOption ext;

        public static SettlementLayoutDef sld;
        public static ThingDef generalWallStuff;

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