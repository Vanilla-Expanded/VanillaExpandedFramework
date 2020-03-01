using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class ScenPartUtility
    {

        public static List<ScenPart_ForcedFactionGoodwill> goodwillScenParts;
        public static Dictionary<FactionDef, bool> cachedFactionPermanentEnemyFlags;
        public static Dictionary<FactionDef, IntRange> cachedFactionNaturalGoodwillRanges;

        public static void SetCache()
        {
            cachedFactionPermanentEnemyFlags = new Dictionary<FactionDef, bool>();
            cachedFactionNaturalGoodwillRanges = new Dictionary<FactionDef, IntRange>();
            foreach (var faction in DefDatabase<FactionDef>.AllDefsListForReading)
            {
                cachedFactionPermanentEnemyFlags.Add(faction, faction.permanentEnemy);
                cachedFactionNaturalGoodwillRanges.Add(faction, faction.naturalColonyGoodwill);
            }
                
        }

        public static void FinaliseFactionGoodwillCharacteristics(FactionDef faction)
        {
            if (goodwillScenParts == null)
                goodwillScenParts = Find.Scenario.AllParts.Where(p => p.def == ScenPartDefOf.VFEC_ForcedFactionGoodwill).Cast<ScenPart_ForcedFactionGoodwill>().ToList();

            // Go through each scenario part that modifies goodwill in reverse order and return an appropriate natural goodwill range
            for (int i = goodwillScenParts.Count - 1; i >= 0; i--)
            {
                var curScenPart = goodwillScenParts[i];
                if (curScenPart.AffectsFaction(faction))
                {
                    faction.permanentEnemy = curScenPart.alwaysHostile;
                    if (curScenPart.affectNaturalGoodwill)
                        faction.naturalColonyGoodwill = curScenPart.naturalGoodwillRange;
                    return;
                }
            }

            // Otherwise use cached
            faction.permanentEnemy = cachedFactionPermanentEnemyFlags[faction];
            faction.naturalColonyGoodwill = cachedFactionNaturalGoodwillRanges[faction];
        }

    }

}
