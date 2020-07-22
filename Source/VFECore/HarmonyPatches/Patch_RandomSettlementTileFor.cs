using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using RimWorld.Planet;

namespace VFECore
{
    public static class Patch_RandomSettlementTileFor
    {
        [HarmonyPatch(typeof(TileFinder), nameof(TileFinder.RandomSettlementTileFor))]
        public static class RandomSettlementTileFor_Patch
        {
            public static bool Prefix(ref int __result, Faction faction, bool mustBeAutoChoosable = false, Predicate<int> extraValidator = null)
            {
                if (faction?.def != null && faction.def.HasModExtension<FactionDefExtension>())
                {
                    var options = faction.def.GetModExtension<FactionDefExtension>();
                    if (options.allowedBiomes != null)
                    {
                        __result = RandomSettlementTileFor(options, faction, mustBeAutoChoosable, extraValidator);
                        return false;
                    }
                    if (options.disallowedBiomes != null)
                    {
                        __result = RandomSettlementTileFor(options, faction, mustBeAutoChoosable, extraValidator);
                        return false;
                    }
                }
                return true;
            }

            public static int RandomSettlementTileFor(FactionDefExtension options, Faction faction, bool mustBeAutoChoosable = false, Predicate<int> extraValidator = null)
            {
                for (int i = 0; i < 500; i++)
                {
                    if ((from _ in Enumerable.Range(0, 100)
                         select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate (int x)
                         {
                             Tile tile = Find.WorldGrid[x];
                             if (options.disallowedBiomes != null && options.disallowedBiomes.Contains(tile.biome))
                             {
                                 return 0f;
                             }
                             if (options.allowedBiomes != null && !options.allowedBiomes.Contains(tile.biome))
                             {
                                 return 0f;
                             }
                             if (!tile.biome.canBuildBase || !tile.biome.implemented || tile.hilliness == Hilliness.Impassable)
                             {
                                 return 0f;
                             }
                             if (mustBeAutoChoosable && !tile.biome.canAutoChoose)
                             {
                                 return 0f;
                             }
                             return (extraValidator != null && !extraValidator(x)) ? 0f : tile.biome.settlementSelectionWeight;
                         }, out int result) && TileFinder.IsValidTileForNewSettlement(result))
                    {
                        return result;
                    }
                }
                Log.Error("Failed to find faction base tile for " + faction);
                return 0;
            }
        }
    }
}