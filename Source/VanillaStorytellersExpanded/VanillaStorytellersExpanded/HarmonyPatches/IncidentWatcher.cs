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
                    float alliesCount = Find.FactionManager.GetFactions().Where(x => x.PlayerRelationKind == FactionRelationKind.Ally).Count();
                    float enemiesCount = Find.FactionManager.GetFactions().Where(x => x.PlayerRelationKind == FactionRelationKind.Hostile).Count();

                    // we put a 90% cap here...
                    if (alliesCount > 9f) alliesCount = 9f;
                    if (enemiesCount > 9f) enemiesCount = 9f;

                    if (alliesCount == 0f) alliesCount = 0.1f;
                    if (enemiesCount == 0f) enemiesCount = 0.1f;

                    if (incidentOptions.alliesIncreaseGoodIncidents && IsGoodIncident(def, incidentOptions))
                    {
                        __result *= alliesCount;
                    }
                    else if (incidentOptions.alliesReduceThreats && IsBadIncident(def, incidentOptions))
                    {
                        __result /= alliesCount;
                    }
                    if (incidentOptions.enemiesIncreaseGoodIncidents && IsGoodIncident(def, incidentOptions))
                    {
                        __result *= enemiesCount;
                    }
                    else if (incidentOptions.enemiesReduceThreats && IsBadIncident(def, incidentOptions))
                    {
                        __result /= enemiesCount;
                    }
                }
            }
        }

        private static bool IsGoodIncident(IncidentDef def, IncidentSpawnOptions incidentOptions)
        {
            return def.letterDef == LetterDefOf.PositiveEvent
                || incidentOptions.goodIncidents.Contains(def.defName);
        }

        private static bool IsBadIncident(IncidentDef def, IncidentSpawnOptions incidentOptions)
        {
            return def.category == IncidentCategoryDefOf.ThreatBig
                || def.category == IncidentCategoryDefOf.ThreatSmall
                || def.letterDef == LetterDefOf.ThreatBig
                || def.letterDef == LetterDefOf.ThreatSmall
                || incidentOptions.negativeIncidents.Contains(def.defName);
        }
    }
}
