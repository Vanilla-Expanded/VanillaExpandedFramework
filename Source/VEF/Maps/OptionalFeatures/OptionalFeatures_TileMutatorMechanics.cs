using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using System;

namespace VEF.Maps
{

    public static class OptionalFeatures_TileMutatorMechanics
    {
        public static void ApplyFeature(Harmony harm)
        {

            harm.Patch(AccessTools.Method(typeof(Game), "InitNewGame"),
                transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_Game_InitNewGame_Patch), "TweakMapSizes"));

            harm.Patch(AccessTools.Method(typeof(CompDeepScanner), "DoFind"),
                transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_CompDeepScanner_DoFind_Patch), "ModifyDeepResourceNumbers"));

            //This motherfucker currently causing errors due to a null dict key

            //harm.Patch(AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(StorytellerComp_Disease), "MakeIntervalIncidents")) ,
            //    transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_StorytellerComp_Disease_MakeIntervalIncidents_Patch), "ModifyBiomeDiseaseMTB"));

            harm.Patch(AccessTools.Method(typeof(WITab_Terrain), "ListMiscDetails"),
                transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_WITab_Terrain_ListMiscDetails_Patch), "CorrectlyOutputBiomeDiseaseMTB"));

            harm.Patch(AccessTools.Method(typeof(WorldPathGrid), "CalculatedMovementDifficultyAt"),
               transpiler: new HarmonyMethod(typeof(VanillaExpandedFramework_WorldPathGrid_CalculatedMovementDifficultyAt_Patch), "TweakMovementDifficulty"));

            harm.Patch(AccessTools.Method(typeof(GetOrGenerateMapUtility), "GetOrGenerateMap", new Type[] { typeof(PlanetTile), typeof(IntVec3), typeof(WorldObjectDef), typeof(IEnumerable<GenStepWithParams>), typeof(bool) }),
               prefix: new HarmonyMethod(typeof(VanillaExpandedFramework_GetOrGenerateMapUtility_GetOrGenerateMap_Patch), "TweakMapSizes"));

            harm.Patch(AccessTools.Method(typeof(WildAnimalSpawner), "SpawnRandomWildAnimalAt"),
               postfix: new HarmonyMethod(typeof(VanillaExpandedFramework_WildAnimalSpawner_SpawnRandomWildAnimalAt_Patch), "AddExtraAnimalsByMutator"));
        }
    }
}
