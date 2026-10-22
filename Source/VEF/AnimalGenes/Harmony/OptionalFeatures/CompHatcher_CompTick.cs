using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using VEF.AnimalGenes;
using Verse;


namespace VEF.AnimalGenes
{

    // This Harmony patch will only be patched if AnimalGenesMechanics is added via XML to a mod using OptionalFeatures

    public class VEF_AnimalGenes_CompHatcher_CompTick_Patch
    {
      
        public static void RemoveIfRuined(CompHatcher __instance)
        {
            if (__instance.TemperatureDamaged)
            {
                CompAnimalGenes comp = __instance.parent.TryGetComp<CompAnimalGenes>();
                if (comp != null) { 
                    comp.active = false;
                }
              
            }

        }
    }
}