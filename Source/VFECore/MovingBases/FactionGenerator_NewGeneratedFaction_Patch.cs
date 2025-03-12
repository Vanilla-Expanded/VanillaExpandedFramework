using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(FactionGenerator), "NewGeneratedFaction", new Type[] { typeof(FactionGeneratorParms) })]
    public static class FactionGenerator_NewGeneratedFaction_Patch
    {
        public static void Postfix(FactionGeneratorParms parms, ref Faction __result)
        {
            if (__result != null)
            {
                foreach (var movingBaseDef in DefDatabase<MovingBaseDef>.AllDefs)
                {
                    if (movingBaseDef.baseFaction == __result.def && movingBaseDef.initialSpawnCount > 0)
                    {
                        var spawnCount = movingBaseDef.initialSpawnCount;
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