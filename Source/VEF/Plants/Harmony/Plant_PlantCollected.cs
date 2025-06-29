﻿using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

using System;


namespace VEF.Plants
{



    [HarmonyPatch(typeof(Plant))]
    [HarmonyPatch("PlantCollected")]
    public static class VanillaExpandedFramework_Plant_PlantCollected_Patch
    {
        [HarmonyPrefix]
        public static void AddSecondaryOutput(Plant __instance, Pawn by)
        {
            DualCropExtension extension = __instance.def.GetModExtension<DualCropExtension>();


            if (extension!=null)
            {
                if (__instance.CanYieldNow()) {

                    float statValue = by.GetStatValue(StatDefOf.PlantHarvestYield);

                    if ((by.RaceProps.Humanlike||by.RaceProps.IsMechanoid) && !__instance.Blighted && Rand.Value < statValue)
                    {
                     
                        int num = (int)(extension.outPutAmount * __instance.Growth);
                        if (statValue > 1f)
                        {
                            num = GenMath.RoundRandom((float)num * statValue);
                        }
                        if (num > 0)
                        {
                            Thing thing = null;
                            if (extension.randomOutput && !extension.randomSecondaryOutput.NullOrEmpty())
                            {
                                thing = ThingMaker.MakeThing(extension.randomSecondaryOutput.RandomElement());
                            }
                            else if (extension.secondaryOutput!=null)
                            {
                                thing = ThingMaker.MakeThing(extension.secondaryOutput);
                            }
                            if (thing != null)
                            {
                                thing.stackCount = num;
                                if (by.Faction != Faction.OfPlayer)
                                {
                                    thing.SetForbidden(value: true);
                                }
                                GenPlace.TryPlaceThing(thing, by.Position, by.Map, ThingPlaceMode.Near);
                            }
                           
                        }
                    }

                   


                }

               

            }
            

        }


    }


}











