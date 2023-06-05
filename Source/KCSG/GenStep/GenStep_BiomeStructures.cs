using System.Collections.Generic;
using System.Security.Policy;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace KCSG
{
    public class GenStep_BiomeStructures : GenStep
    {
        public override int SeedPart => 919504193;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (map.Biome.modExtensions.NullOrEmpty())
            {
                return;
            }

            for (int p = 0; p < map.Biome.modExtensions.Count; p++)
            {
                var extension = map.Biome.modExtensions[p];
                if (extension is BiomeStructGenExtension ext)
                {
                    if (ext.onlyOnPlayerMap && map.ParentFaction != Faction.OfPlayer)
                    {
                        return;
                    }

                    int spawnCount = ext.countScaleHiliness ? ext.scalingOptions.GetScalingFor(map, ext.spawnCount) : ext.spawnCount;
                    for (int i = 0; i < spawnCount; i++)
                    {
                        StructureLayoutDef layout = ext.structures.RandomElementByWeight(l => l.commonality).layout;

                        IntVec3 spawnPos = CellFinderLoose.RandomCellWith((c) =>
                        {
                            CellRect rect = CellRect.CenteredOn(c, layout.sizes.x + ext.clearCellRadiusAround, layout.sizes.z + ext.clearCellRadiusAround);

                            if (!rect.InBounds(map))
                            {
                                return false;
                            }

                            foreach (IntVec3 cell in rect.Cells)
                            {
                                if (!ext.canSpawnInMontains)
                                {
                                    if (!cell.Walkable(map))
                                    {
                                        return false;
                                    }
                                }

                                if (!ext.canSpawnOnWaterTerrain)
                                {
                                    if (cell.GetTerrain(map).HasTag("Water"))
                                        return false;
                                }
                            }

                            return true;
                        }, map);


                        var spawnRect = CellRect.CenteredOn(spawnPos, layout.sizes.x, layout.sizes.z);
                        GenOption.GetAllMineableIn(spawnRect, map);
                        layout.Generate(spawnRect, map);
                    }

                    if (ext.postGenerateOre)
                    {
                        GenStep_ScatterLumpsMineable gen = new GenStep_ScatterLumpsMineable
                        {
                            maxValue = ext.maxMineableValue
                        };

                        float count = 0f;
                        switch (Find.WorldGrid[map.Tile].hilliness)
                        {
                            case Hilliness.Flat:
                                count = 4f;
                                break;
                            case Hilliness.SmallHills:
                                count = 8f;
                                break;
                            case Hilliness.LargeHills:
                                count = 11f;
                                break;
                            case Hilliness.Mountainous:
                                count = 15f;
                                break;
                        }
                        gen.countPer10kCellsRange = new FloatRange(count, count);
                        gen.Generate(map, parms);
                    }
                }
            }
        }
    }
}