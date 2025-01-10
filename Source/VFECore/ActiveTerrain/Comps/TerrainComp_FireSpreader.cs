using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace VFECore
{
    /// <summary>
    /// This terrain comp will push heat endlessly with no regard for ambient temperature.
    /// </summary>
    public class TerrainComp_FireSpreader : TerrainComp
    {
        public TerrainCompProperties_FireSpreader Props { get { return (TerrainCompProperties_FireSpreader)props; } }

        private int hashOffset;
        private bool hasFire;
        private int[] checkTimers = new int[8];
        private int checkCounter = 0;

        private int overrideCooldown = 0;

        private int tickCounter;
        private int warmupTicks = 1;

        public override void PlaceSetup()
        {
            base.PlaceSetup();
            this.hashOffset = this.parent.Map.cellIndices.CellToIndex(this.parent.Position);
        }



        public override void CompTick()
        {
            base.CompTick();
            this.tickCounter++;

            if (this.overrideCooldown > 0)
            {
                this.overrideCooldown--;
            }
            else
            {
                int ticksOffset = (this.tickCounter + this.hashOffset) % this.Props.spreadTimer;

                if (ticksOffset < 8)
                {
                    if (ticksOffset == 0)
                    {
                        if (this.checkCounter >= 8)
                        {
                            this.hasFire = false;
                            this.overrideCooldown = GenDate.TicksPerHour * Props.spreadTimer * 2;
                        }
                        else
                        {
                            this.hasFire = parent.Position.ContainsStaticFire(parent.Map);
                        }
                        this.checkCounter = 0;
                    }

                    if (this.hasFire)
                    {
                        if (this.warmupTicks > 0)
                        {
                            this.warmupTicks--;
                        }
                        else
                        {
                            if (this.checkTimers[ticksOffset] > 0)
                            {
                                this.checkTimers[ticksOffset]--;
                                this.checkCounter++;
                            }
                            else
                            {
                                IntVec3 tile = this.parent.Position + GenAdj.AdjacentCells[ticksOffset];

                                if (tile != IntVec3.Invalid && tile.InBounds(parent.Map))
                                {
                                    TerrainDef terrain = tile.GetTerrain(parent.Map);
                                    if (this.parent.def == terrain)
                                        FireUtility.TryStartFireIn(tile, parent.Map, 1f, null);
                                    this.checkTimers[ticksOffset] = GenDate.TicksPerHour * Props.spreadTimer;
                                } else
                                {
                                    this.checkTimers[ticksOffset] = int.MaxValue;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref this.warmupTicks, nameof(this.warmupTicks), 1);

            if(Scribe.mode == LoadSaveMode.PostLoadInit)
                this.hashOffset = this.parent.Map.cellIndices.CellToIndex(this.parent.Position);
        }
    }
}
