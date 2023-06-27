using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class GenOption
    {
        public static CustomGenOption customGenExt;
        public static SettlementLayoutDef settlementLayout;
        public static StructureLayoutDef structureLayout;
        // Falling structures
        public static FallingStructure fallingExt;
        public static StructureLayoutDef fallingLayout;
        // List of corpses to rot
        public static List<Corpse> corpsesToRot = new List<Corpse>();
        // List of tileDef and their center
        public static Dictionary<CellRect, TileDef> tiledRects = new Dictionary<CellRect, TileDef>();

        private static Dictionary<IntVec3, Mineable> mineables;

        public static StuffableOptions StuffableOptions => settlementLayout?.stuffableOptions;

        public static RoadOptions RoadOptions => settlementLayout.roadOptions;

        public static PropsOptions PropsOptions => settlementLayout.propsOptions;

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
            for (int i = 0; i < corpsesToRot.Count; i++)
            {
                var corpse = corpsesToRot[i];
                corpse.timeOfDeath = Mathf.Max(Find.TickManager.TicksGame - 60000 * Rand.RangeInclusive(5, 15), 0);
                corpse.TryGetComp<CompRottable>()?.RotImmediately();
            }
            corpsesToRot.Clear();
        }
    }
}