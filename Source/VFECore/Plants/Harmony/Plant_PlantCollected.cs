using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

using System;


namespace VanillaPlantsExpanded
{



    [HarmonyPatch(typeof(Plant))]
    [HarmonyPatch("PlantCollected")]
    public static class VanillaPlantsExpanded_Plant_PlantCollected_Patch
    {
        [HarmonyPostfix]
        public static void AddSecondaryOutput(Plant __instance, Pawn by)
        {
            DualCropExtension extension = __instance.def.GetModExtension<DualCropExtension>();


            if (extension!=null)
            {

                float statValue = by.GetStatValue(StatDefOf.PlantHarvestYield);
                if (!(by.RaceProps.Humanlike && !__instance.Blighted && Rand.Value > statValue))
                {

                    int num = extension.outPutAmount;
                    if (statValue > 1f)
                    {
                        num = GenMath.RoundRandom((float)num * statValue);
                    }
                    if (num > 0)
                    {
                        Thing thing = ThingMaker.MakeThing(extension.secondaryOutput);
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











