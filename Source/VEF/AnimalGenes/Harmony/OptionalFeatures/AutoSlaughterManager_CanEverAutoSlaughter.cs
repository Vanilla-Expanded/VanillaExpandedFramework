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

    public class VEF_AnimalGenes_AutoSlaughterManager_CanEverAutoSlaughter_Patch
    {

        public static void ToggleAutoSlaughter(Pawn animal, ref bool __result)
        {
            if (__result)
            {
                CompAnimalGenes comp = animal.TryGetComp<CompAnimalGenes>();
                if (comp != null)
                {
                    __result=!comp.autoslaughterActive;
                }

            }

        }
    }
}