using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using System.Reflection.Emit;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Verse.AI;

namespace AnimalBehaviours
{

    [HarmonyPatch(typeof(MapTemperature))]
    [HarmonyPatch("SeasonAcceptableFor")]
    public static class VanillaExpandedFramework_MapTemperature_SeasonAcceptableFor_Patch
    {
        [HarmonyPostfix]
        public static void AllowAnimalSpawns(ThingDef animalRace, ref bool __result)

        {

           

            if (VanillaAnimalsExpanded_Mod.settings.pawnSpawnStates != null && VanillaAnimalsExpanded_Mod.settings.pawnSpawnStates.Keys.Contains(animalRace.defName))
            {
                if (VanillaAnimalsExpanded_Mod.settings.pawnSpawnStates[animalRace.defName])
                {

                    __result = false;
                }

            }


        }
    }





}
