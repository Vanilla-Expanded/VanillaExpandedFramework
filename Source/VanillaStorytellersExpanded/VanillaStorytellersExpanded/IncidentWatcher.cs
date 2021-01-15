using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using static Verse.DamageWorker;

namespace VanillaStorytellersExpanded
{

    [HarmonyPatch(typeof(StorytellerComp), "IncidentChanceFinal")]
    public static class Patch_IncidentChanceFinal
    {
        public static void Postfix(ref float __result, IncidentDef def)
        {
            if (__result > 0)
            {
                var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
                if (options != null && options.incidentSpawnOptions != null)
                {
                    var incidentOptions = options.incidentSpawnOptions;
                    float alliesCount = Find.FactionManager.GetFactions_NewTemp().Where(x => x.PlayerRelationKind == FactionRelationKind.Ally).Count();
                    float enemiesCount = Find.FactionManager.GetFactions_NewTemp().Where(x => x.PlayerRelationKind == FactionRelationKind.Hostile).Count();
                    Log.Message(Find.Storyteller.def.defName + " - Incident spawn: " + def.defName + " - vanilla spawn chance: " + __result + " - Allies count: " + alliesCount + " - enemiesCount: " + enemiesCount);

                    // we put a 90% cap here...
                    if (alliesCount > 9f) alliesCount = 9f;
                    if (enemiesCount > 9f) enemiesCount = 9f;

                    if (alliesCount == 0f) alliesCount = 0.1f;
                    if (enemiesCount == 0f) enemiesCount = 0.1f;

                    if (incidentOptions.alliesIncreaseGoodIncidents && incidentOptions.goodIncidents.Contains(def.defName))
                    {
                        __result *= alliesCount;
                    }
                    else if (incidentOptions.alliesReduceThreats && (def.category == IncidentCategoryDefOf.ThreatBig || def.category == IncidentCategoryDefOf.ThreatSmall
                        || incidentOptions.negativeIncidents.Contains(def.defName)))
                    {
                        __result /= alliesCount;
                    }
                    if (incidentOptions.enemiesIncreaseGoodIncidents && incidentOptions.goodIncidents.Contains(def.defName))
                    {
                        __result *= enemiesCount;
                    }
                    else if (incidentOptions.enemiesReduceThreats && (def.category == IncidentCategoryDefOf.ThreatBig || def.category == IncidentCategoryDefOf.ThreatSmall
                        || incidentOptions.negativeIncidents.Contains(def.defName)))
                    {
                        __result /= enemiesCount;
                    }
                    Log.Message(Find.Storyteller.def.defName + " - New Incident spawn: " + def.defName + " - new incident spawn chance: " + __result + " - Allies count: " + alliesCount + " - enemiesCount: " + enemiesCount);
                }
            }
        }
    }
}
