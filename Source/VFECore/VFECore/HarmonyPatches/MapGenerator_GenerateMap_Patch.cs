using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld.Planet;
using HarmonyLib;

// Copyright Sarg - Alpha Biomes 2020 & Taranchuck

namespace VFECore
{
    [HarmonyPatch(typeof(MapGenerator), nameof(MapGenerator.GenerateMap))]
    public static class MapGenerator_GenerateMap_Patch
    {
        public static void Postfix(Map __result)
        {
            DoMapSpawns(__result);
        }
        public static bool CanSpawnAt(IntVec3 c, Map map, ObjectSpawnsDef element)
        {
            if (!element.allowOnChunks)
            {
                foreach (var item in c.GetThingList(map))
                {
                    if (item?.def?.thingCategories != null)
                    {
                        foreach (var category in item.def.thingCategories)
                        {
                            if (category == ThingCategoryDefOf.Chunks || category == ThingCategoryDefOf.StoneChunks)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            TerrainDef terrain = c.GetTerrain(map);

            bool flagAllowed = true;

            if (element.allowedTerrains != null)
            {
                foreach (string allowed in element.allowedTerrains)
                {
                    if (terrain.defName == allowed)
                    {
                        break;
                    }
                    else flagAllowed = false;
                }
            }

            if (!flagAllowed) return false;

            if (element.disallowedTerrainTags != null)
            {
                foreach (string notAllowed in element.disallowedTerrainTags)
                {
                    if (terrain.HasTag(notAllowed))
                    {
                        return false;
                    }
                }
            }


            if (!element.allowOnWater && terrain.IsWater)
            {
                return false;
            }

            if (element.findCellsOutsideColony)
            {
                if (!OutOfCenter(c, map, 60))
                {
                    return false;
                }
            }

            return true;
        }
        public static void DoMapSpawns(Map map)
        {
            if (map is null)
            {
                Log.Error("[VEF] Map was null, MapGenerator_GenerateMap_Patch won't properly.");
                return;
            }
            int spawnCounter = 0;
            foreach (ObjectSpawnsDef element in DefDatabase<ObjectSpawnsDef>.AllDefs.Where(element => element.allowedBiomes.Contains(map.Biome)))
            {
                if (element.thingDef is null && element.pawnKindDef is null)
                {
                    Log.Error("[VEF] ObjectSpawnsDef " + element.defName + " contain null thing / pawnkind def. It will not work.");
                    continue;
                }
                if (element.spawnOnlyInPlayerMaps && !map.IsPlayerHome)
                {
                    continue;
                }
                IEnumerable<IntVec3> tmpTerrain = map.AllCells.InRandomOrder();
                if (spawnCounter == 0)
                {
                    spawnCounter = element.numberToSpawn.RandomInRange;
                }
                foreach (IntVec3 c in tmpTerrain)
                {
                    bool canSpawn = CanSpawnAt(c, map, element);
                    if (canSpawn)
                    {
                        var faction = element.factionDef != null ? Find.FactionManager.FirstFactionOfDef(element.factionDef) : null;
                        Thing thing = element.thingDef != null ? ThingMaker.MakeThing(element.thingDef, null) : PawnGenerator.GeneratePawn(element.pawnKindDef, faction);
                        if (faction != null && !(thing is Pawn))
                        {
                            thing.SetFaction(faction);
                        }
                        CellRect occupiedRect = GenAdj.OccupiedRect(c, thing.Rotation, thing.def.Size);
                        if (occupiedRect.InBounds(map))
                        {
                            canSpawn = true;
                            foreach (IntVec3 c2 in occupiedRect)
                            {
                                if (!CanSpawnAt(c2, map, element))
                                {
                                    canSpawn = false;
                                    break;
                                }
                            }
                            if (canSpawn)
                            {
                                try
                                {
                                    if (element.randomRotation)
                                    {
                                        GenPlace.TryPlaceThing(thing, c, map, ThingPlaceMode.Direct, null, null, Rot4.Random);
                                    }
                                    else
                                    {
                                        GenSpawn.Spawn(thing, c, map);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Log.Error("Exception spawning thing " + thing);
                                }
                                spawnCounter--;
                            }
                        }
                    }

                    if (canSpawn && spawnCounter <= 0)
                    {
                        spawnCounter = 0;
                        break;
                    }
                }
            }
        }

        public static bool OutOfCenter(IntVec3 c, Map map, int centerDist)
        {
            IntVec3 CenterPoint = map.Center;
            return c.x < CenterPoint.x - centerDist || c.z < CenterPoint.z - centerDist || c.x >= CenterPoint.x + centerDist || c.z >= CenterPoint.z + centerDist;
        }
    }
}