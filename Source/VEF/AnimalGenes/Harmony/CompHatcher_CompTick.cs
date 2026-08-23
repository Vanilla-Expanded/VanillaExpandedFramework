using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using VEF.AnimalGenes;


namespace VEF.AnimalGenes
{

    [HarmonyPatch(typeof(CompHatcher))]
    [HarmonyPatch("CompTick")]

    public class VEF_AnimalGenes_CompHatcher_CompTick_Patch
    {
        [HarmonyPostfix]
        public static void RemoveIfRuined(CompHatcher __instance)
        {
            if (__instance.TemperatureDamaged)
            {
                WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.Remove(__instance.parent);
            }

        }
    }
}