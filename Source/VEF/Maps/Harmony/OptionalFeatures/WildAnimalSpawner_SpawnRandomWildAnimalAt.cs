using HarmonyLib;
using RimWorld;
using System;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using Verse.Noise;
using VEF;

namespace VEF.Maps
{
    // This Harmony patch will only be patched if TileMutatorMechanics is added via XML to a mod using OptionalFeatures
  
    public static class VanillaExpandedFramework_WildAnimalSpawner_SpawnRandomWildAnimalAt_Patch
    {
       
        public static void AddExtraAnimalsByMutator(WildAnimalSpawner __instance, Map ___map, bool __result)

        {
            if (__result)
            {
                foreach (TileMutatorDef mutator in ___map.TileInfo.Mutators)
                {
                    if (mutator.Worker != null && mutator.Worker is TileMutatorWorker_ExtraAnimal)
                    {
                        TileMutatorExtension extension = mutator.GetModExtension<TileMutatorExtension>();
                        if (extension != null)
                        {
                            PawnKindDefAndChance kindDefAndChance = extension.forcedPawnKindDefs.RandomElement();

                            if (Rand.Chance(kindDefAndChance.forcedPawnKindDefChance))
                            {
                                if (RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 newLoc, ___map, CellFinder.EdgeRoadChance_Animal, allowFogged: true, (IntVec3 cell) => !___map.areaManager.Home[cell] && ___map.reachability.CanReachMapEdge(cell, TraverseParms.For(TraverseMode.NoPassClosedDoors).WithFenceblocked(forceFenceblocked: true))))
                                {
                                    SpawnAnimal(kindDefAndChance.forcedPawnKindDef, newLoc, ___map);
                                }


                            }
                        }

                    }
                }
            }

        }

        public static void SpawnAnimal(PawnKindDef animalKind, IntVec3 loc, Map map)
        {

            int numToSpawn = animalKind.wildGroupSize.RandomInRange;
            int spawnRadius = Mathf.CeilToInt(Mathf.Sqrt(animalKind.wildGroupSize.max));
            for (int i = 0; i < numToSpawn; i++)
            {
                IntVec3 spawnLoc = CellFinder.RandomClosewalkCellNear(loc, map, spawnRadius * 10);
                Pawn animal = PawnGenerator.GeneratePawn(animalKind);
                if (Rand.Chance(map.BiomeAt(loc).wildAnimalScariaChance))
                {
                    animal.health.AddHediff(HediffDefOf.Scaria);
                }

                GenSpawn.Spawn(animal, spawnLoc, map);

            }

        }

    }


}