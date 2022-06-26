using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using Verse;

namespace KCSG
{
    internal class WorldGenStep_SpawnWorldObjects : WorldGenStep
    {
        public override int SeedPart => 1616161616;

        public override void GenerateFresh(string seed)
        {
            var worldobjects = DefDatabase<WorldObjectDef>.AllDefsListForReading.FindAll(wo => wo.HasModExtension<SpawnAtWorldGen>());
            foreach (var worldObject in worldobjects)
            {
                SpawnAtWorldGen ext = worldObject.GetModExtension<SpawnAtWorldGen>();
                for (int i = 0; i < ext.spawnCount; i++)
                {
                    Site wo = (Site)WorldObjectMaker.MakeWorldObject(worldObject);
                    // Set faction
                    if (ext.spawnPartOfFaction != null)
                        wo.SetFaction(Find.FactionManager.FirstFactionOfDef(ext.spawnPartOfFaction));
                    // Find tile
                    wo.Tile = RandomTileFor(wo, ext, 500);
                    // Parts
                    SitePartParams parms = new SitePartParams
                    {
                        points = 500f
                    };

                    foreach (var part in ext.parts)
                    {
                        SitePart sitePart = new SitePart(wo, part, parms);
                        wo.AddPart(sitePart);
                    }

                    Find.WorldObjects.Add(wo);
                }
            }
        }

        internal int RandomTileFor(WorldObject wo, SpawnAtWorldGen ext, int maxTries, Predicate<int> extraValidator = null)
        {
            for (int i = 0; i < maxTries; i++)
            {
                if (Enumerable.Range(0, 100).Select(_ => Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(x =>
                {
                    Tile tile = Find.WorldGrid[x];
                    if (!tile.biome.canBuildBase || !tile.biome.implemented || tile.hilliness == Hilliness.Impassable)
                    {
                        return 0f;
                    }
                    else if (Find.WorldObjects.AnyWorldObjectAt(x) || Find.WorldObjects.AnySettlementBaseAtOrAdjacent(x))
                    {
                        return 0f;
                    }
                    else if (ext.allowedBiomes?.Count > 0 && !ext.allowedBiomes.Contains(tile.biome))
                    {
                        return 0f;
                    }
                    else if (ext.disallowedBiomes?.Count > 0 && ext.disallowedBiomes.Contains(tile.biome))
                    {
                        return 0f;
                    }
                    else if (extraValidator != null && !extraValidator(x))
                    {
                        return 0f;
                    }
                    return tile.biome.settlementSelectionWeight;

                }, out int result))
                {
                    return result;
                }
            }
            Debug.Message($"Failed to find world tile for {wo.def.defName}");
            return 0;
        }
    }
}