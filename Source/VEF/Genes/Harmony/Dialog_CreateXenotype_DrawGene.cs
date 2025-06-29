﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace VEF.Genes
{
    [HarmonyPatch(typeof(Dialog_CreateXenotype), "DrawGene")]
    public static class VanillaExpandedFramework__Dialog_CreateXenotype_DrawGene_Patch
    {
        
        public static bool Prefix(GeneDef geneDef, ref bool __result)
        {
            
            if(StaticCollectionsClass.hidden_genes.Contains(geneDef))
            {
                __result = false;
                return false;
            }
            return true;
           
        }

        
    }
}
