using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace VEF.Planet
{
    [HarmonyPatch(typeof(FactionGenerator), "NewGeneratedFaction", typeof(PlanetLayer), typeof(FactionGeneratorParms))]
    public static class VanillaExpandedFramework_FactionGenerator_NewGeneratedFaction_Patch
    {
        public static void Postfix(PlanetLayer layer, FactionGeneratorParms parms, ref Faction __result)
        {
            if (__result != null && layer.Def == PlanetLayerDefOf.Surface)
            {
                foreach (var movingBaseDef in DefDatabase<MovingBaseDef>.AllDefs)
                {
                    if (movingBaseDef.baseFaction == __result.def && movingBaseDef.initialSpawnCount.min > 0)
                    {
                        var spawnCount = movingBaseDef.initialSpawnCount.RandomInRange;
                        if (movingBaseDef.initialSpawnScalesWithPopulation)
                        {
                            spawnCount = Mathf.RoundToInt(spawnCount * Find.World.info.overallPopulation.GetScaleFactor());
                        }

                        for (var i = 0; i < spawnCount; i++)
                        {
                            var movingBase = (MovingBase)WorldObjectMaker.MakeWorldObject(movingBaseDef);
                            movingBase.Tile = TileFinder.RandomSettlementTileFor(__result);
                            movingBase.SetFaction(__result);
                            Find.WorldObjects.Add(movingBase);
                        }
                    }
                }
            }
        }
    }
}