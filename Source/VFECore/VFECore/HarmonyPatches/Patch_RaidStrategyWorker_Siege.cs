using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_RaidStrategyWorker_Siege
    {

        [HarmonyPatch(typeof(RaidStrategyWorker_Siege), "MakeLordJob")]
        public static class MakeLordJob
        {

            public static bool Prefix(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed, ref LordJob __result)
            {
                // Conditionally detour the method
                var faction = parms.faction;
                var factionDefExtension = FactionDefExtension.Get(faction.def);
                if (factionDefExtension.siegeParameterSetDef != null)
                {
                    var entrySpot = (!parms.spawnCenter.IsValid) ? pawns[0].PositionHeld : parms.spawnCenter;
                    var siegeSpot = RCellFinder.FindSiegePositionFrom(entrySpot, map);
                    float blueprintPoints = Mathf.Max(parms.points * Rand.Range(0.2f, 0.3f), factionDefExtension.siegeParameterSetDef.lowestArtilleryBlueprintPoints);
                    __result = new LordJob_SiegeCustom(faction, siegeSpot, blueprintPoints);
                    return false;
                }

                return true;
            }

        }

    }

}
