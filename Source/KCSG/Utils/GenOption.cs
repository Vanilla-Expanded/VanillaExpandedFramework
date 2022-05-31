namespace KCSG
{
    public static class GenOption
    {
        public static CustomGenOption ext;

        public static SettlementLayoutDef settlementLayoutDef;

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