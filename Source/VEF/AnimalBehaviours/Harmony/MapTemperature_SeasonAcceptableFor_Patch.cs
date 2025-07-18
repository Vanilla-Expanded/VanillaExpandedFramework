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
using RimWorld.Planet;

namespace VEF.AnimalBehaviours
{

    [HarmonyPatch(typeof(MapTemperature))]
    [HarmonyPatch("SeasonAcceptableFor")]
    public static class VanillaExpandedFramework_MapTemperature_SeasonAcceptableFor_Patch
    {
        [HarmonyPostfix]
        public static void AllowAnimalSpawns(ThingDef animalRace, ref bool __result, Map ___map)

        {          

            if (VanillaAnimalsExpanded_Mod.settings.pawnSpawnStates != null && VanillaAnimalsExpanded_Mod.settings.pawnSpawnStates.Keys.Contains(animalRace.defName))
            {
                if (VanillaAnimalsExpanded_Mod.settings.pawnSpawnStates[animalRace.defName])
                {
                    __result = false;
                }

            }

            if (animalRace != null && StaticCollectionsClass.riverAnimals.Contains(animalRace))
            {
                SurfaceTile surfaceTile = ___map.Tile.Tile as SurfaceTile;
                if (surfaceTile.Rivers == null)
                {
                    __result = false;
                }

            }


        }
    }





}
