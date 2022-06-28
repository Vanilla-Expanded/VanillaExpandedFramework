using System.Collections.Generic;
using RimWorld;
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
        public static StructureLayoutDef structureLayoutDef;

        public static ThingDef generalWallStuff;
        public static List<IntVec3> usedSpots;
        public static Dictionary<IntVec3, Mineable> mineables;

        public static FallingStructure fallingStructure;
        public static StructureLayoutDef fallingStructureChoosen;

        public static void ClearFalling()
        {
            fallingStructure = null;
            fallingStructureChoosen = null;
        }

        public static void DespawnMineableAt(IntVec3 cell)
        {
            if (mineables.ContainsKey(cell) && mineables[cell] != null)
            {
                if (mineables[cell].Spawned)
                    mineables[cell].DeSpawn();

                mineables[cell] = null;
            }
        }
    }
}