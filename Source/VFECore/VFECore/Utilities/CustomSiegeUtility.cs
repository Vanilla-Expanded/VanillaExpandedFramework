using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFECore
{

    public static class CustomSiegeUtility
    {

        private static SiegeParameterSetDef customParams;

        public static bool AcceptsShell(Building_TurretGun artillery, ThingDef shellDef)
        {
            var changeableProjComp = artillery.gun.TryGetComp<CompChangeableProjectile>();
            if (changeableProjComp == null)
            {
                return false;
            }

            return changeableProjComp.allowedShellsSettings.AllowedToAccept(shellDef);
        }

        public static IEnumerable<Blueprint_Build> PlaceBlueprints(LordToilData_SiegeCustom data, Map map, Faction placeFaction)
        {
            customParams = FactionDefExtension.Get(placeFaction.def).siegeParameterSetDef;
            NonPublicFields.SiegeBlueprintPlacer_center.SetValue(null, data.siegeCenter);
            NonPublicFields.SiegeBlueprintPlacer_faction.SetValue(null, placeFaction);

            // Cover
            if (customParams.coverDef != null)
            {
                var coverBlueprints = PlaceCoverBlueprints(map).ToList();
                for (int i = 0; i < coverBlueprints.Count; i++)
                {
                    yield return coverBlueprints[i];
                }
            }

            // Artillery
            if (!customParams.artilleryBuildingTags.NullOrEmpty())
            {
                var artilleryBlueprints = PlaceArtilleryBlueprints(data, map).ToList();
                for (int i = 0; i < artilleryBlueprints.Count; i++)
                {
                    yield return artilleryBlueprints[i];
                }
            }
        }

        private static IEnumerable<Blueprint_Build> PlaceCoverBlueprints(Map map)
        {
            var centre = (IntVec3)NonPublicFields.SiegeBlueprintPlacer_center.GetValue(null);
            var lengthRange = (IntRange)NonPublicFields.SiegeBlueprintPlacer_CoverLengthRange.GetValue(null);
            var countRange = (IntRange)NonPublicFields.SiegeBlueprintPlacer_NumCoverRange.GetValue(null);
            var placedSandbagLocs = (List<IntVec3>)NonPublicFields.SiegeBlueprintPlacer_placedCoverLocs.GetValue(null);
            placedSandbagLocs.Clear();
            int numSandbags = countRange.RandomInRange;
            var coverStuff = customParams.coverDef.MadeFromStuff ?
                GenStuff.RandomStuffInexpensiveFor(customParams.coverDef, (Faction)NonPublicFields.SiegeBlueprintPlacer_faction.GetValue(null)) : null;
            for (int i = 0; i < numSandbags; i++)
            {
                var bagRoot = FindCoverRoot(map, customParams.coverDef, coverStuff);
                if (!bagRoot.IsValid)
                {
                    yield break;
                }
                Rot4 growDirA;
                if (bagRoot.x > centre.x)
                {
                    growDirA = Rot4.West;
                }
                else
                {
                    growDirA = Rot4.East;
                }
                Rot4 growDirB;
                if (bagRoot.z > centre.z)
                {
                    growDirB = Rot4.South;
                }
                else
                {
                    growDirB = Rot4.North;
                }
                var coverLine = MakeCoverLine(bagRoot, map, growDirA, lengthRange.RandomInRange, customParams.coverDef, coverStuff).ToList();
                for (int j = 0; j < coverLine.Count; j++)
                {
                    yield return coverLine[j];
                }

                bagRoot += growDirB.FacingCell;
                coverLine = MakeCoverLine(bagRoot, map, growDirB, lengthRange.RandomInRange, customParams.coverDef, coverStuff).ToList();
                for (int j = 0; j < coverLine.Count; j++)
                {
                    yield return coverLine[j];
                }
            }
            yield break;
        }

        private static IntVec3 FindCoverRoot(Map map, ThingDef coverDef, ThingDef coverStuff)
        {
            var centre = (IntVec3)NonPublicFields.SiegeBlueprintPlacer_center.GetValue(null);
            var placedCoverLocs = (List<IntVec3>)NonPublicFields.SiegeBlueprintPlacer_placedCoverLocs.GetValue(null);
            var cellRect = CellRect.CenteredOn(centre, 13);
            cellRect.ClipInsideMap(map);
            var cellRect2 = CellRect.CenteredOn(centre, 8);
            cellRect2.ClipInsideMap(map);
            int num = 0;
            for (; ; )
            {
                num++;
                if (num > 200)
                {
                    break;
                }
                var randomCell = cellRect.RandomCell;
                if (!cellRect2.Contains(randomCell))
                {
                    if (map.reachability.CanReach(randomCell, centre, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly))
                    {
                        if (NonPublicMethods.SiegeBlueprintPlacer_CanPlaceBlueprintAt(randomCell, Rot4.North, coverDef, map, coverStuff))
                        {
                            bool flag = false;
                            for (int i = 0; i < placedCoverLocs.Count; i++)
                            {
                                float num2 = (placedCoverLocs[i] - randomCell).LengthHorizontalSquared;
                                if (num2 < 36f)
                                {
                                    flag = true;
                                }
                            }
                            if (!flag)
                            {
                                return randomCell;
                            }
                        }
                    }
                }
            }
            return IntVec3.Invalid;
        }

        private static IEnumerable<Blueprint_Build> MakeCoverLine(IntVec3 root, Map map, Rot4 growDir, int maxLength, ThingDef coverThing, ThingDef coverStuff)
        {
            var placedSandbagLocs = (List<IntVec3>)NonPublicFields.SiegeBlueprintPlacer_placedCoverLocs.GetValue(null);
            var cur = root;
            for (int i = 0; i < maxLength; i++)
            {
                if (!NonPublicMethods.SiegeBlueprintPlacer_CanPlaceBlueprintAt(cur, Rot4.North, coverThing, map, coverStuff))
                {
                    break;
                }
                yield return GenConstruct.PlaceBlueprintForBuild(coverThing, cur, map, Rot4.North, (Faction)NonPublicFields.SiegeBlueprintPlacer_faction.GetValue(null), coverStuff);
                placedSandbagLocs.Add(cur);
                cur += growDir.FacingCell;
            }
            yield break;
        }

        private static IEnumerable<Blueprint_Build> PlaceArtilleryBlueprints(LordToilData_SiegeCustom data, Map map)
        {
            IEnumerable<ThingDef> artyDefs = customParams.artilleryDefs;

            // No tag matches
            if (!artyDefs.Any())
            {
                Log.Error($"Could not find any artillery ThingDefs matching the following tags: {customParams.artilleryBuildingTags.ToStringSafeEnumerable()}");
                yield break;
            }

            float points = data.blueprintPoints;

            // Generate blueprints
            int numArtillery = Mathf.RoundToInt(points / customParams.lowestArtilleryBlueprintPoints);
            numArtillery = Mathf.Clamp(numArtillery, customParams.artilleryCountRange.min, customParams.artilleryCountRange.max);
            int i = 0;
            while (points > 0 && i < numArtillery)
            {
                artyDefs = artyDefs.Where(t => t.GetModExtension<ThingDefExtension>() is ThingDefExtension extension
                && extension.siegeBlueprintPoints <= points);
                if (!artyDefs.Any())
                {
                    yield break;
                }

                var rot = Rot4.Random;
                var artyDef = artyDefs.RandomElementByWeight(t => t.GetModExtension<ThingDefExtension>().siegeBlueprintPoints);
                var artySpot = NonPublicMethods.SiegeBlueprintPlacer_FindArtySpot(artyDef, rot, map);
                if (!artySpot.IsValid)
                {
                    yield break;
                }

                yield return GenConstruct.PlaceBlueprintForBuild(artyDef, artySpot, map, rot, (Faction)NonPublicFields.SiegeBlueprintPlacer_faction.GetValue(null), GenStuff.DefaultStuffFor(artyDef));
                if (data.artilleryCounts.ContainsKey(artyDef))
                {
                    data.artilleryCounts[artyDef]++;
                }
                else
                {
                    data.artilleryCounts.Add(artyDef, 1);
                }

                points -= artyDef.GetModExtension<ThingDefExtension>().siegeBlueprintPoints;
                i++;
            }
            yield break;
        }

    }

}
