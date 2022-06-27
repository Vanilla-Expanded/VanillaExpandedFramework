using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VFECore
{
    public static class ScenPartUtility
    {
        public static List<ScenPart_ForcedFactionGoodwill> goodwillScenParts;
        public static Dictionary<FactionDef, bool>         orginalPermanentEnemyCache;
        public static Dictionary<FactionDef, IntRange>     startingGoodwillRangeCache;

        public static void SetCache()
        {
            orginalPermanentEnemyCache = new Dictionary<FactionDef, bool>();
            startingGoodwillRangeCache = new Dictionary<FactionDef, IntRange>();

            var factions = DefDatabase<FactionDef>.AllDefsListForReading;
            for (var i = 0; i < factions.Count; i++)
            {
                var faction = factions[i];
                orginalPermanentEnemyCache.Add(faction, faction.permanentEnemy);
            }
        }

        public static void FinaliseFactionGoodwillCharacteristics(FactionDef faction)
        {
            if (goodwillScenParts == null)
                goodwillScenParts = Find.Scenario.AllParts.Where(p => p.def == ScenPartDefOf.VFEC_ForcedFactionGoodwill).Cast<ScenPart_ForcedFactionGoodwill>()
                    .ToList();

            // Go through each scenario part that modifies goodwill in reverse order and return an appropriate natural goodwill range
            for (var i = goodwillScenParts.Count - 1; i >= 0; i--)
            {
                var curScenPart = goodwillScenParts[i];
                if (curScenPart.AffectsFaction(faction))
                {
                    faction.permanentEnemy = curScenPart.alwaysHostile;
                    if (curScenPart.affectStartingGoodwill) startingGoodwillRangeCache.Add(faction, curScenPart.startingGoodwillRange);
                    return;
                }
            }

            // Otherwise use cached
            faction.permanentEnemy = orginalPermanentEnemyCache[faction];
        }
    }
}