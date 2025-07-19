using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld.Planet;
using HarmonyLib;

namespace VEF.Maps
{
    [HarmonyPatch(typeof(MapGenerator), "GenerateMap")]
    public static class VanillaExpandedFramework_MapGenerator_GenerateMap_Patch
    {
        public static void Postfix(Map __result)
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                try
                {
                    DoMapSpawns(__result);
                }
                catch (Exception ex)
                {
                    Log.Error("[VEF] Error in MapGenerator_GenerateMap_Patch: " + ex.ToString());
                }
            });
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

            if (c.GetThingList(map).Any(x => x is not Filth))
            {
                return false;
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
            foreach (ObjectSpawnsDef element in DefDatabase<ObjectSpawnsDef>.AllDefs.Where(element => CanSpawnAt(map, element)))
            {
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
                        var faction = element.factionDef != null ? Find.FactionManager.FirstFactionOfDef(element.factionDef) 
                            : null;
                        Thing thing;
                        if (element.pawnKindDef != null)
                        {
                            thing = PawnGenerator.GeneratePawn(element.pawnKindDef, faction);
                        }
                        else
                        {
                            var thingDef = GetThingDefToSpawn(element);
                            if (thingDef is null)
                            {
                                break;
                            }
                            try
                            {
                                thing = ThingMaker.MakeThing(thingDef, GenStuff.RandomStuffFor(thingDef));
                            }
                            catch (Exception)
                            {
                                Log.Error("Error making: " + thingDef);
                                break;
                            }
                        }
                        if (faction != null && !(thing is Pawn) && thing.def.CanHaveFaction)
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
                                if (thing.def.stackLimit > 1)
                                {
                                    thing.stackCount = Mathf.Min(Rand.RangeInclusive(1, thing.def.stackLimit), spawnCounter);
                                }
                                spawnCounter -= thing.stackCount;

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
                                catch (Exception ex)
                                {
                                    Log.Error("Exception spawning thing " + thing + " - " + ex.ToString());
                                }
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

        private static ThingDef GetThingDefToSpawn(ObjectSpawnsDef element)
        {
            if (element.thingDef != null)
            {
                return element.thingDef;
            }
            else if (element.thingDefs.NullOrEmpty() is false)
            {
                return element.thingDefs.RandomElementByWeight(x => x.weight).thingDef;
            }
            else if (element.category != null)
            {
                return element.category.childThingDefs.RandomElement();
            }
            return null;
        }

        private static bool CanSpawnAt(Map map, ObjectSpawnsDef element)
        {
            if (element.allowedPlanetLayers is null && map.Tile.LayerDef != PlanetLayerDefOf.Surface)
            {
                return false;
            }
            if (element.allowedPlanetLayers != null && !element.allowedPlanetLayers.Contains(map.Tile.LayerDef))
            {
                return false;
            }
            if (element.spawnOnlyInPlayerMaps && !map.IsPlayerHome)
            {
                return false;
            }
            if (element.allowSpawningOnPocketMaps is false && map.IsPocketMap)
            {
                return false;
            }
            if (element.allowedBiomes != null && element.allowedBiomes.Contains(map.Biome) is false)
            {
                return false;
            }
            var tile = Find.WorldGrid[map.Tile.tileId];
            if (element.allowedRoads != null && (tile.Roads is null
                || element.allowedRoads.Any(road => tile.Roads.Any(x => x.road == road)) is false))
            {
                return false;
            }

            return true;
        }

        public static bool OutOfCenter(IntVec3 c, Map map, int centerDist)
        {
            IntVec3 CenterPoint = map.Center;
            return c.x < CenterPoint.x - centerDist || c.z < CenterPoint.z - centerDist || c.x >= CenterPoint.x + centerDist || c.z >= CenterPoint.z + centerDist;
        }
    }
}