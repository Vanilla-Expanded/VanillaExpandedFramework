using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace VEF.Genes
{
    public class MapComponent_AstrogeneOverrides : MapComponent
    {



        public MapComponent_AstrogeneOverrides(Map map) : base(map)
        {

        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (Find.TickManager.TicksGame % 1000 == 0)
            {
                List<Pawn> pawnsWithAstrogenes = map.mapPawns.AllHumanlike.Where(x => x.genes?.GenesListForReading.ContainsAny(x => x.def.geneClass == typeof(Gene_Astrogene))==true)?.ToList();
                if (!pawnsWithAstrogenes.NullOrEmpty()) {
                    foreach (Pawn pawn in pawnsWithAstrogenes) {
                        ReflectionCache.checkForOverrides(pawn.genes);
                    }
                }
            
            }

        }





    }


}

