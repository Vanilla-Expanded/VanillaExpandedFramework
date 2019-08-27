using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFECore
{

    public static class CustomSiegeUtility
    {

        public static Dictionary<FactionDef, List<ThingDef>> cachedArtilleryBuildings;
        public static Dictionary<FactionDef, float> lowestArtilleryBlueprintPoints;

        public static void SetCache()
        {
            cachedArtilleryBuildings = new Dictionary<FactionDef, List<ThingDef>>();
            lowestArtilleryBlueprintPoints = new Dictionary<FactionDef, float>();
            foreach (var faction in DefDatabase<FactionDef>.AllDefsListForReading)
            {
                // Set cached artillery buildings
                cachedArtilleryBuildings.Add(faction, new List<ThingDef>());
                var factionDefExtension = FactionDefExtension.Get(faction);
                foreach (var thing in DefDatabase<ThingDef>.AllDefsListForReading)
                    if (thing.building != null && thing.building.buildingTags != null && thing.building.buildingTags.Any(t => factionDefExtension.artilleryBuildingTags.Contains(t)))
                            cachedArtilleryBuildings[faction].Add(thing);

                // Set lowest artillery building points
                lowestArtilleryBlueprintPoints.Add(faction, int.MaxValue);
                foreach (var artillery in cachedArtilleryBuildings[faction])
                {
                    var thingDefExtension = artillery.GetModExtension<ThingDefExtension>();
                    float blueprintPoints = thingDefExtension?.siegeBlueprintPoints ?? SiegeBlueprintPlacer.ArtyCost;
                    if (blueprintPoints < lowestArtilleryBlueprintPoints[faction])
                        lowestArtilleryBlueprintPoints[faction] = blueprintPoints;
                }
            }


        }

        public static IEnumerable<Blueprint_Build> PlaceArtilleryBlueprints(List<string> tags, float points, Map map)
        {
            var artyDefs = DefDatabase<ThingDef>.AllDefs.Where(t => t.building != null && t.building.buildingTags.Any(t2 => tags.Contains(t2)));

            // No tag matches
            if (!artyDefs.Any())
            {
                Log.Error($"Could not find any artillery ThingDefs matching the following tags: {tags.ToStringSafeEnumerable()}... using defaults...");
                yield break;
            }

            // Generate blueprints
            int numArtillery = Mathf.RoundToInt(points / 60f);
            numArtillery = Mathf.Clamp(numArtillery, 1, 2);
            for (int i = 0; i < numArtillery; i++)
            {
                var rot = Rot4.Random;
                var artyDef = artyDefs.RandomElement();
                var artySpot = NonPublicMethods.SiegeBlueprintPlacer_FindArtySpot(artyDef, rot, map);
                if (!artySpot.IsValid)
                    yield break;
                yield return GenConstruct.PlaceBlueprintForBuild(artyDef, artySpot, map, rot, (Faction)NonPublicFields.SiegeBlueprintPlacer_faction.GetValue(null), GenStuff.DefaultStuffFor(artyDef));
                points -= 60f;
            }
            yield break;
        }

    }

}
