using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class SpecialTerrainList : MapComponent
    {
        //.ctor... need we say more
        public SpecialTerrainList(Map map) : base(map) { }

        public List<TerrainInstance> terrainInstances = new List<TerrainInstance>();

        public Dictionary<IntVec3, TerrainInstance> terrains = new Dictionary<IntVec3, TerrainInstance>();

        public TerrainInstance[] terrainsArray;

        public HashSet<TerrainDef> terrainDefs = new HashSet<TerrainDef>();

        private bool dirty = false;

        private int index = 0;

        private int cycles = 1;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref terrains, "terrains", LookMode.Value, LookMode.Deep);
            this.terrainDefs = new HashSet<TerrainDef>(this.terrains.Select(t => t.Value.def).Distinct());
        }

        /// <summary>
        /// Updater for terrains
        /// </summary>
        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            foreach (var terr in terrains)
            {
                terr.Value.Update();
            }

            foreach (TerrainDef terrainDef in this.terrainDefs)
            {
                if (terrainDef.modExtensions != null)
                {
                    foreach (DefModExtension extension in terrainDef.modExtensions)
                        if (extension is DefExtensionActive act)
                            act.DoWork(terrainDef);
                }
            }
        }

        /// <summary>
        /// Ticker for terrains
        /// </summary>
        public void TerrainUpdate(long timeBudget)
        {
            if (this.terrains.Count != 0)
            {
                Stopwatch stopwatch = new Stopwatch();
                TerrainInstance[] terrains;

                if (terrainsArray == null || terrainsArray?.Length != this.terrains.Count || dirty)
                {
                    terrains = terrainsArray = this.terrains.Select(p => p.Value).ToArray();
                    index = 0;
                    dirty = false;
                }
                else
                {
                    terrains = terrainsArray;
                }
                int i = index;
                int k = 0;
                while (stopwatch.ElapsedTicks < timeBudget && k < terrains.Length / 6)
                {
                    if (i >= terrains.Length)
                    {
                        i = 0;
                        cycles += 1;
                    }
                    var terr = terrains[i];
                    if (terr.def.tickerType == TickerType.Normal)
                    {
                        terr.Tick();
                    }
                    else if (terr.def.tickerType == TickerType.Rare && cycles % 35 == 0)
                    {
                        terr.TickRare();
                    }
                    else if (terr.def.tickerType == TickerType.Long && cycles % 250 == 0)
                    {
                        terr.TickLong();
                    }
                    i++;
                    k++;
                }
                stopwatch.Stop();
                index = i;
                if (Prefs.DevMode && Prefs.LogVerbose) Log.Message(
                    string.Format("ReGrowther: ticked {0} out of {1} in {2} ms and Cycled to {3}", k, terrains.Length, stopwatch.ElapsedMilliseconds, cycles));
            }
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            RefreshAllCurrentTerrain();
            CallPostLoad();
        }

        public void CallPostLoad()
        {
            foreach (var k in terrains.Keys)
            {
                terrains[k].PostLoad();
            }
        }

        /// <summary>
        /// Registers terrain currently present to terrain list, called on init
        /// </summary>
        public void RefreshAllCurrentTerrain()
        {
            Reset();
            foreach (var cell in map) //Map is IEnumerable...
            {
                TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                if (terrain is ActiveTerrainDef special)
                {
                    RegisterAt(special, cell);
                }
            }
        }

        public void RegisterAt(ActiveTerrainDef special, int i)
        {
            RegisterAt(special, map.cellIndices.IndexToCell(i));
        }

        public void RegisterAt(ActiveTerrainDef special, IntVec3 cell)
        {
            if (!terrains.ContainsKey(cell))
            {
                var newTerr = special.MakeTerrainInstance(map, cell);
                newTerr.Init();
                terrainInstances.Add(newTerr);
                terrains.Add(cell, newTerr);
                this.terrainDefs.Add(special);
                FixAt(terrainInstances.Count);
            }
        }

        public void Notify_RemovedTerrainAt(IntVec3 c)
        {
            var terr = terrains[c];
            var index = FixAt(terrainInstances.IndexOf(terr));
            terrains.Remove(c);
            terrainInstances.Remove(terr);
            terrainsArray = terrainInstances.ToArray();
            terr.PostRemove();

        }

        public int FixAt(int i = -1)
        {
            dirty = true;
            if (i != -1)
            {
                if (i >= this.index)
                    return i;
                this.index = Mathf.Max(i - 1, 0);
            }
            return i;
        }

        public void Reset()
        {
            dirty = true;
            index = 0;
        }
    }
}
