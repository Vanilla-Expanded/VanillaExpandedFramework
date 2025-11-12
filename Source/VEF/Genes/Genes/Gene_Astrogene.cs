using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VEF.Genes
{
    public class Gene_Astrogene : Gene
    {
        
        public override bool Active
        {
            get
            {
                if(pawn is null) return false;
                if (pawn?.Position != IntVec3.Invalid && pawn.Map?.BiomeAt(pawn.Position)?.inVacuum == false)
                {
                   
                    return false;
                }               
                if (pawn?.ageTracker != null && (float)pawn.ageTracker.AgeBiologicalYears < def.minAgeActive)
                {
                   
                    return false;
                }
                if (pawn?.mutant != null && pawn.mutant.Def.disablesGenes.Contains(def))
                {
                   
                    return false;
                }
                
                return true;
            }
        }

       

    }
}