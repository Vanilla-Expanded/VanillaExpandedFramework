using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using RimWorld.Planet;
using System.Reflection;

namespace VFECore
{
    [HarmonyPatch]
    public static class Patch_RandomSettlementTileFor
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return typeof(TileFinder).GetNestedTypes(AccessTools.all)
                .SelectMany(x => x.GetMethods(AccessTools.all))
                .FirstOrDefault(x => x.Name.Contains("<RandomSettlementTileFor>") && x.ReturnType == typeof(float));
        }

        public static void Postfix(ref float __result, int x)
        {
            if (RandomSettlementTileFor_Patch.factionToCheck != null && __result > 0)
            {
                var options = RandomSettlementTileFor_Patch.factionToCheck.def.GetModExtension<FactionDefExtension>();
                if (options != null)
                {
                    Tile tile = Find.WorldGrid[x];
                    if ((options.disallowedBiomes?.Any() ?? false) && options.disallowedBiomes.Contains(tile.biome))
                    {
                        //Log.Message(RandomSettlementTileFor_Patch.factionToCheck.def + " can't settle in " + tile.biome + ", disallowed biomes: " + String.Join(", ", options.disallowedBiomes));
                        __result = 0f;
                    }
                    else if ((options.allowedBiomes?.Any() ?? false) && !options.allowedBiomes.Contains(tile.biome))
                    {
                        //Log.Message(RandomSettlementTileFor_Patch.factionToCheck.def + " can't settle in " + tile.biome + ", allowed biomes: " + String.Join(", ", options.allowedBiomes));
                        __result = 0f;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TileFinder), nameof(TileFinder.RandomSettlementTileFor))]
        public static class RandomSettlementTileFor_Patch
        {
            public static Faction factionToCheck;
            public static void Prefix(Faction faction)
            {
                factionToCheck = faction;
            }
        }
    }
}