using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using Harmony;

namespace VFECore
{

    public static class CustomSiegeUtility
    {

        private static SiegeParameterSetDef customParams;

        public static bool AcceptsShell(Building_TurretGun artillery, ThingDef shellDef)
        {
            var changeableProjComp = artillery.gun.TryGetComp<CompChangeableProjectile>();
            if (changeableProjComp == null)
                return false;
            return changeableProjComp.allowedShellsSettings.AllowedToAccept(shellDef);
        }

        public static IEnumerable<Blueprint_Build> PlaceBlueprints(LordToilData_SiegeCustom data, Map map, Faction placeFaction)
        {
            customParams = FactionDefExtension.Get(placeFaction.def).siegeParameterSetDef;
            NonPublicFields.SiegeBlueprintPlacer_center.SetValue(null, data.siegeCenter);
            NonPublicFields.SiegeBlueprintPlacer_faction.SetValue(null, placeFaction);

            // Cover
            if (customParams.coverDef != null)
                foreach (Blueprint_Build blue in PlaceSandbagBlueprints(map))
                    yield return blue;

            // Artillery
            if (!customParams.artilleryBuildingTags.NullOrEmpty())
                foreach (Blueprint_Build blue2 in PlaceArtilleryBlueprints(data, map))
                    yield return blue2;
        }

        private static IntVec3 FindSandbagRoot(Map map)
        {
            var centre = (IntVec3)NonPublicFields.SiegeBlueprintPlacer_center.GetValue(null);
            var placedSandbagLocs = (List<IntVec3>)NonPublicFields.SiegeBlueprintPlacer_placedSandbagLocs.GetValue(null);
            CellRect cellRect = CellRect.CenteredOn(centre, 13);
            cellRect.ClipInsideMap(map);
            CellRect cellRect2 = CellRect.CenteredOn(centre, 8);
            cellRect2.ClipInsideMap(map);
            int num = 0;
            for (; ; )
            {
                num++;
                if (num > 200)
                {
                    break;
                }
                IntVec3 randomCell = cellRect.RandomCell;
                if (!cellRect2.Contains(randomCell))
                {
                    if (map.reachability.CanReach(randomCell, centre, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly))
                    {
                        if (NonPublicMethods.SiegeBlueprintPlacer_CanPlaceBlueprintAt(randomCell, Rot4.North, customParams.coverDef, map))
                        {
                            bool flag = false;
                            for (int i = 0; i < placedSandbagLocs.Count; i++)
                            {
                                float num2 = (float)(placedSandbagLocs[i] - randomCell).LengthHorizontalSquared;
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

        private static IEnumerable<Blueprint_Build> PlaceSandbagBlueprints(Map map)
        {
            var centre = (IntVec3)NonPublicFields.SiegeBlueprintPlacer_center.GetValue(null);
            var lengthRange = (IntRange)NonPublicFields.SiegeBlueprintPlacer_SandbagLengthRange.GetValue(null);
            var countRange = (IntRange)NonPublicFields.SiegeBlueprintPlacer_NumSandbagRange.GetValue(null);
            var placedSandbagLocs = (List<IntVec3>)NonPublicFields.SiegeBlueprintPlacer_placedSandbagLocs.GetValue(null);
            placedSandbagLocs.Clear();
            int numSandbags = countRange.RandomInRange;
            for (int i = 0; i < numSandbags; i++)
            {
                IntVec3 bagRoot = FindSandbagRoot(map);
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
                foreach (Blueprint_Build bag in MakeSandbagLine(bagRoot, map, growDirA, lengthRange.RandomInRange))
                {
                    yield return bag;
                }
                bagRoot += growDirB.FacingCell;
                foreach (Blueprint_Build bag2 in MakeSandbagLine(bagRoot, map, growDirB, lengthRange.RandomInRange))
                {
                    yield return bag2;
                }
            }
            yield break;
        }

        private static IEnumerable<Blueprint_Build> MakeSandbagLine(IntVec3 root, Map map, Rot4 growDir, int maxLength)
        {
            var placedSandbagLocs = (List<IntVec3>)NonPublicFields.SiegeBlueprintPlacer_placedSandbagLocs.GetValue(null);
            var stuff = customParams.coverDef.MadeFromStuff ? GenStuff.RandomStuffInexpensiveFor(customParams.coverDef, (Faction)NonPublicFields.SiegeBlueprintPlacer_faction.GetValue(null)) : null;
            IntVec3 cur = root;
            for (int i = 0; i < maxLength; i++)
            {
                if (!NonPublicMethods.SiegeBlueprintPlacer_CanPlaceBlueprintAt(cur, Rot4.North, customParams.coverDef, map))
                {
                    break;
                }
                yield return GenConstruct.PlaceBlueprintForBuild(customParams.coverDef, cur, map, Rot4.North, (Faction)NonPublicFields.SiegeBlueprintPlacer_faction.GetValue(null), stuff);
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
                artyDefs = artyDefs.Where(t => ThingDefExtension.Get(t).siegeBlueprintPoints <= points);
                if (!artyDefs.Any())
                    yield break;
                var rot = Rot4.Random;
                var artyDef = artyDefs.RandomElementByWeight(t => ThingDefExtension.Get(t).siegeBlueprintPoints);
                var artySpot = NonPublicMethods.SiegeBlueprintPlacer_FindArtySpot(artyDef, rot, map);
                if (!artySpot.IsValid)
                    yield break;
                yield return GenConstruct.PlaceBlueprintForBuild(artyDef, artySpot, map, rot, (Faction)NonPublicFields.SiegeBlueprintPlacer_faction.GetValue(null), GenStuff.DefaultStuffFor(artyDef));
                if (data.artilleryCounts.ContainsKey(artyDef))
                    data.artilleryCounts[artyDef]++;
                else
                    data.artilleryCounts.Add(artyDef, 1);
                points -= ThingDefExtension.Get(artyDef).siegeBlueprintPoints;
                i++;
            }
            yield break;
        }

    }

}
