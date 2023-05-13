using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class GenOption
    {
        public static CustomGenOption ext;
        public static SettlementLayoutDef sld;
        public static StructureLayoutDef structureLayoutDef;

        public static ThingDef generalWallStuff;
        public static List<IntVec3> usedSpots;

        // Falling structures
        public static FallingStructure fExt;
        public static StructureLayoutDef fDef;

        // List of corpses to rot
        public static List<Corpse> corpseToRot = new List<Corpse>();

        private static Dictionary<IntVec3, Mineable> mineables;

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

        /// <summary>
        /// Fill the mineable list from all mineable in rect of map
        /// </summary>
        /// <param name="rect">Rect to search</param>
        /// <param name="map">Map</param>
        public static void GetAllMineableIn(CellRect rect, Map map)
        {
            mineables = new Dictionary<IntVec3, Mineable>();
            foreach (var cell in rect)
            {
                if (cell.InBounds(map))
                    mineables.Add(cell, cell.GetFirstMineable(map));
            }
        }

        /// <summary>
        /// Get mineable at cell
        /// </summary>
        /// <param name="cell">Cell</param>
        /// <returns>Mineable</returns>
        public static Mineable GetMineableAt(IntVec3 cell)
        {
            if (mineables.ContainsKey(cell))
            {
                return mineables[cell];
            }

            return null;
        }

        /// <summary>
        /// Despawn the mineable on cell
        /// </summary>
        /// <param name="cell">Cell</param>
        public static void DespawnMineableAt(IntVec3 cell)
        {
            if (mineables.ContainsKey(cell) && mineables[cell] != null && mineables[cell].Spawned)
            {
                mineables[cell].DeSpawn();
                mineables[cell] = null;
            }
        }

        /// <summary>
        /// Rot all corpses in corpseToRot
        /// </summary>
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