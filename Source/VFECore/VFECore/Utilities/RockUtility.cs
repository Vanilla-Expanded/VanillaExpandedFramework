using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public static class RockUtility
    {
        private static readonly HashSet<ThingDef>                 rocksWithBiomeUnique = new HashSet<ThingDef>();

        private static readonly AccessTools.FieldRef<World, List<ThingDef>> allNaturalRockDefs =
            AccessTools.FieldRefAccess<World, List<ThingDef>>("allNaturalRockDefs");

        static RockUtility()
        {
            foreach (var def in DefDatabase<BiomeDef>.AllDefs)
            {
                var ext = def.GetModExtension<BiomeExtension>();
                if (ext?.uniqueRockTypes != null) rocksWithBiomeUnique.AddRange(ext.uniqueRockTypes);
            }
        }

        public static List<ThingDef> ForcedRocksFor(this BiomeDef biome, out bool removeOthers)
        {
            var ext = biome.GetModExtension<BiomeExtension>();
            if (ext == null)
            {
                removeOthers = false;
                return null;
            }

            var list = new List<ThingDef>();
            if (ext.uniqueRockTypes != null && ext.forceUniqueRockTypes) list.AddRange(ext.uniqueRockTypes);
            if (ext.forceRockTypes  != null) list.AddRange(ext.forceRockTypes);
            removeOthers = ext.onlyAllowForcedRockTypes;
            return list.Count == 0 ? null : list;
        }

        public static HashSet<ThingDef> DisallowedRocksFor(this BiomeDef biome)
        {
            var ext = biome.GetModExtension<BiomeExtension>();
            if (ext == null) return rocksWithBiomeUnique;
            var set = new HashSet<ThingDef>();
            set.UnionWith(ext.uniqueRockTypes == null ? rocksWithBiomeUnique : rocksWithBiomeUnique.Except(ext.uniqueRockTypes));
            if (ext.disallowRockTypes != null) set.UnionWith(ext.disallowRockTypes);
            return set;
        }

        public static IEnumerable<T> Exclude<T>(this IEnumerable<T> source, HashSet<T> toExclude)
        {
            return source.Where(item => !toExclude.Contains(item));
        }

        public static IEnumerable<ThingDef> PossibleRockTypesFor(this BiomeDef biome, World world)
        {
            allNaturalRockDefs(world) ??= DefDatabase<ThingDef>.AllDefs.Where(d => d.IsNonResourceNaturalRock).ToList();
            return allNaturalRockDefs(world).Exclude(biome.DisallowedRocksFor());
        }
    }
}
