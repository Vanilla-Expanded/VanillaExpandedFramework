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

       
        public override void CompTick()
        {
            base.CompTick();
            Log.Message("Checking");
            if (Find.TickManager.TicksGame % Props.spreadTimer == 0 && parent.Position.ContainsStaticFire(parent.Map))
            {
                Log.Message("Igniting");
                for (int i = 0; i < 8; i++)
                {
                    IntVec3 tile = this.parent.Position + GenAdj.AdjacentCells[i];

                    if (tile.InBounds(parent.Map) && tile != IntVec3.Invalid)
                    {
                        TerrainDef terrain = tile.GetTerrain(parent.Map);
                        if (this.parent.def == terrain)
                        {
                            FireUtility.TryStartFireIn(tile, parent.Map, 1f, null);
                        }
                    }
                }



                 
                


            }
        }
    }
}
