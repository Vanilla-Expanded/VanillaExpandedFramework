using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(World), nameof(World.NaturalRockTypesIn))]
    public class World_NaturalRockTypesIn_Patch
    {
        [HarmonyPostfix]
        public static IEnumerable<ThingDef> Postfix(IEnumerable<ThingDef> rocks, int tile, World __instance)
        {
            var tileInfo = __instance.grid[tile];
            if (tileInfo is null) return rocks;
            var biome = tileInfo.biome;
            var list  = rocks.ToList();
            Rand.PushState(tile);
            var num   = Rand.RangeInclusive(2, 3);
            var force = biome.ForcedRocksFor(out var removeOthers);
            if (force != null)
            {
                // GenDebug.LogList(force);
                if (removeOthers) list = force;
                else
                {
                    while (list.Count + force.Count > num && list.Count > 0) list.Remove(list.RandomElement());
                    list.AddRange(force);
                }
            }

            var disallowed          = biome.DisallowedRocksFor();
            if (disallowed != null && disallowed.Count > 0)
                for (var i = 0; i < list.Count; i++)
                    if (disallowed.Contains(list[i]))
                        list[i] = biome.PossibleRockTypesFor(__instance).Except(list).RandomElement();
            Rand.PopState();
            return list;
        }
    }
}
