using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using System.Reflection.Emit;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Verse.AI;
using RimWorld.Planet;



namespace AnimalBehaviours
{


    /*This Harmony Postfix changes terrain calculation for floating creatures*/

    [HarmonyPatch(typeof(Verse.AI.Pawn_PathFollower))]
    [HarmonyPatch("CostToMoveIntoCell")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(IntVec3) })]
    public static class VanillaExpandedFramework_Pawn_PathFollower_CostToMoveIntoCell_Patch
    {

        [HarmonyPostfix]
        public static void DisablePathCostForFloatingCreatures(Pawn pawn, IntVec3 c, ref float __result)

        {

            if ((pawn.Map != null) && AnimalBehaviours_Settings.flagHovering)
            {

                if (AnimalCollectionClass.floating_animals.Contains(pawn))
                {
                    TerrainDef terrainDef = pawn.Map.terrainGrid.TerrainAt(c);
                    float num;
                    if (c.x == pawn.Position.x || c.z == pawn.Position.z)
                    {
                        num = pawn.TicksPerMoveCardinal;
                    }
                    else
                    {
                        num = pawn.TicksPerMoveDiagonal;
                    }
                    if (terrainDef == null)
                    {
                        num = 10000;
                    }
                    else if ((terrainDef.passability == Traversability.Impassable) && !terrainDef.IsWater)
                    {
                        num = 10000;
                    }
                    List<Thing> list = pawn.Map.thingGrid.ThingsListAt(c);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Thing thing = list[i];
                        if (thing.def.passability == Traversability.Impassable)
                        {
                            num = 10000;
                        }
                    }
                    __result = num;
                }
                if (AnimalCollectionClass.waterstriding_pawns.Contains(pawn))
                {
                    TerrainDef terrainDef = pawn.Map.terrainGrid.TerrainAt(c);
                    if (terrainDef.IsWater) {
                        float num;
                        if (c.x == pawn.Position.x || c.z == pawn.Position.z)
                        {
                            num = pawn.TicksPerMoveCardinal;
                        }
                        else
                        {
                            num = pawn.TicksPerMoveDiagonal;
                        }

                        __result = num;
                    }
                    
                }

            }


        }
    }



}
