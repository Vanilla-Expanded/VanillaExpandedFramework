using System.Collections.Generic;
using RimWorld;
using UnityEngine;
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
        // Falling structures
        public static FallingStructure fExt;
        public static StructureLayoutDef fDef;
        // List of corpses to rot
        public static List<Corpse> corpseToRot = new List<Corpse>();

        public static void DespawnMineableAt(IntVec3 cell)
        {
            if (mineables.ContainsKey(cell) && mineables[cell] != null)
            {
                if (mineables[cell].Spawned)
                    mineables[cell].DeSpawn();

                mineables[cell] = null;
            }
        }

        public static void RotAllThing()
        {
            for (int i = 0; i < corpseToRot.Count; i++)
            {
                var corpse = corpseToRot[i];
                corpse.timeOfDeath = Mathf.Max(Find.TickManager.TicksGame - 60000 * Rand.RangeInclusive(5, 15), 0);
                corpse.TryGetComp<CompRottable>()?.RotImmediately();
            }
            corpseToRot.Clear();
        }
    }
}