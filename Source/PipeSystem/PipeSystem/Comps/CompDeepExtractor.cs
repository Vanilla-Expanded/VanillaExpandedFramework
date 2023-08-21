using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PipeSystem
{
    public class CompDeepExtractor : CompResource
    {
        public List<IntVec3> lumpCells;
        public bool cycleOver = true;
        public bool noCapacity = false;

        private List<IntVec3> adjCells;
        private CompPowerTrader compPower;
        private CompFlickable compFlickable;
        private int nextProduceTick = -1;

        private int globalCount = 0;

        private float rawProductionPerDay;
        private float rawProductionPerTick;

        public new CompProperties_DeepExtractor Props => (CompProperties_DeepExtractor)props;

        public float RawProduction
        {
            get
            {
                if (noCapacity || lumpCells.Count == 0 || (compPower != null && !compPower.PowerOn) || (compFlickable != null && !compFlickable.SwitchIsOn))
                    return 0;

                return rawProductionPerDay;
            }
        }

        public float RawProductionPerTick => rawProductionPerTick;

        /// <summary>
        /// Get comps, cache adjacent cells used in item spawning, calculate raw production per day/tick, get all deep cells
        /// </summary>
        /// <param name="respawningAfterLoad"></param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            compPower = parent.GetComp<CompPowerTrader>();
            compFlickable = parent.GetComp<CompFlickable>();

            adjCells = GenAdj.CellsAdjacent8Way(parent).ToList();

            rawProductionPerDay = (GenDate.TicksPerDay / Props.ticksPerPortion) * (Props.useDeepCountPerPortion ? Props.deepThing.deepCountPerPortion : 1);
            rawProductionPerTick = rawProductionPerDay / GenDate.TicksPerDay;

            // Get the cells
            lumpCells = new List<IntVec3>();
            var treated = new HashSet<IntVec3>();
            var toCheck = new Queue<IntVec3>();

            var thing = Props.deepThing.defName;
            var cell = parent.Position;
            var map = parent.Map;

            toCheck.Enqueue(cell);
            treated.Add(cell);

            while (toCheck.Count > 0)
            {
                var temp = toCheck.Dequeue();
                lumpCells.Add(temp);

                var neighbours = GenAdjFast.AdjacentCellsCardinal(temp);
                for (int i = 0; i < neighbours.Count; i++)
                {
                    var n = neighbours[i];
                    if (n.InBounds(map) && !treated.Contains(n) && map.deepResourceGrid.ThingDefAt(n) is ThingDef r && r.defName == thing)
                    {
                        globalCount += map.deepResourceGrid.CountAt(n);
                        treated.Add(n);
                        toCheck.Enqueue(n);
                    }
                }
            }

            lumpCells.SortByDescending(c => c.DistanceTo(cell));

            LongEventHandler.ExecuteWhenFinished(delegate
            {
                StartSustainer();
            });
        }

        /// <summary>
        /// Stop cycle, reset next production tick
        /// </summary>
        /// <param name="map"></param>
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            nextProduceTick = -1;
            cycleOver = true;
        }

        /// <summary>
        /// Save production tick, cycle, capacity reached
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref nextProduceTick, "nextProduceTick", 0);
            Scribe_Values.Look(ref noCapacity, "noCapacity", false);
            Scribe_Values.Look(ref cycleOver, "cycleOver", true);
        }

        /// <summary>
        /// Manage production, sustainer, cycle
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();
            if (parent.Spawned
                && !noCapacity
                && lumpCells.Count > 0
                && (compPower == null || compPower.PowerOn)
                && (compFlickable == null || compFlickable.SwitchIsOn))
            {
                var ticksGame = Find.TickManager.TicksGame;
                if (nextProduceTick == -1)
                {
                    nextProduceTick = ticksGame + Props.ticksPerPortion;
                    cycleOver = false;
                }
                else if (ticksGame >= nextProduceTick)
                {
                    TryProducePortion();
                    nextProduceTick = ticksGame + Props.ticksPerPortion;
                    cycleOver = false;
                }

                sustainer?.Maintain();
            }
            else
            {
                EndSustainer();
                cycleOver = true;
                noCapacity = PipeNet.storages.Count > 0 && (int)PipeNet.AvailableCapacity <= 1;
            }
        }

        /// <summary>
        /// Spawn resource or push it into the net
        /// </summary>
        private void TryProducePortion()
        {
            // Get resource
            bool nextResource = GetNextResource(out ThingDef resDef, out int count, out IntVec3 cell);
            // Resource is null or not the wanted one -> Return
            if (resDef == null || resDef.defName != Props.deepThing.defName)
                return;

            var map = parent.Map;
            // Resource comp is here
            if (nextResource && !Props.onlyExtractToGround && PipeNet is PipeNet net && net.storages.Count >= 1)
            {
                var available = (int)net.AvailableCapacity;
                noCapacity = available <= 1;

                if (noCapacity == false)
                {
                    var initialCount = Props.useDeepCountPerPortion ? resDef.deepCountPerPortion : 1;
                    var min = initialCount > count ? count : initialCount;
                    var extractAmount = min > available ? available : min;

                    parent.Map.deepResourceGrid.SetAt(cell, resDef, count - extractAmount);
                    net.DistributeAmongStorage(extractAmount, out _);
                    globalCount -= extractAmount;
                    StartSustainer();

                    if (!cycleOver) cycleOver = true;
                }
                else
                {
                    EndSustainer();
                }
            }
            // If there is no resource comp
            // Deplete resource by one
            else if (nextResource && !Props.onlyExtractToNet)
            {
                StartSustainer();
                var initialCount = Props.useDeepCountPerPortion ? resDef.deepCountPerPortion : 1;
                var extractAmount = initialCount > count ? count : initialCount;

                parent.Map.deepResourceGrid.SetAt(cell, resDef, count - extractAmount);
                // Spawn items
                var t = ThingMaker.MakeThing(resDef);
                t.stackCount = extractAmount;
                GenPlace.TryPlaceThing(t, adjCells.RandomElement(), map, ThingPlaceMode.Near);
                globalCount -= extractAmount;

                if (!cycleOver) cycleOver = true;
            }

            lumpCells.RemoveAll(c => map.deepResourceGrid.ThingDefAt(c) == null);
        }

        /// <summary>
        /// Check first cell of lumpCells for any matching resource. Remove cell from the list if empty.
        /// </summary>
        /// <param name="resDef">Resource ThingDef</param>
        /// <param name="countPresent">Resource count</param>
        /// <param name="cell">Resource cell</param>
        /// <returns>Found resource ?</returns>
        private bool GetNextResource(out ThingDef resDef, out int countPresent, out IntVec3 cell)
        {
            var map = parent.Map;
            if (lumpCells.Count > 0)
            {
                var c = lumpCells[0];

                if (map.deepResourceGrid.ThingDefAt(c) is ThingDef r)
                {
                    resDef = r;
                    countPresent = map.deepResourceGrid.CountAt(c);
                    cell = c;
                    return true;
                }
                else
                {
                    resDef = null;
                    countPresent = 0;
                    cell = c;
                    lumpCells.RemoveAt(0);
                    return false;
                }
            }
            resDef = null;
            countPresent = 0;
            cell = IntVec3.Invalid;
            return false;
        }

        /// <summary>
        /// Show deepcount left, capacity and if there is no deep resource
        /// </summary>
        /// <returns>Info string</returns>
        public override string CompInspectStringExtra()
        {
            var str = base.CompInspectStringExtra();
            if (parent.Spawned)
            {
                if (Props.showDeepCountLeft)
                {
                    str += "\n" + "PipeSystem_ResourceLeft".Translate(globalCount);
                }
                if (noCapacity)
                {
                    str += "\n" + Props.noStorageLeftKey.Translate();
                }
                if (lumpCells.Count == 0)
                {
                    str += "\n" + "DeepDrillNoResources".Translate();
                }
            }
            return str;
        }
    }
}