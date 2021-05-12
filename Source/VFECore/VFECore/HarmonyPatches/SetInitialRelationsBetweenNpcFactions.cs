using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(Faction), "TryMakeInitialRelationsWith")]
    public class SetInitialRelationsBetweenNpcFactions
    {
        // Custom initial relations between NPC factions should be patched last, including after HAR's own faction relation code
        [HarmonyAfter(new string[] { "rimworld.erdelf.alien_race.main" })]
        public static void Postfix(Faction __instance, Faction other)
        {           
            var currentFactionDefExtension = FactionDefExtension.Get(__instance.def);
            var otherFactionDefExtension = FactionDefExtension.Get(other.def);

            // If at least one of the factions references the other via custom values in the FactionDefExtension
            if ((currentFactionDefExtension?.startingGoodwillByFactionDefs.Exists(x => x.factionDef == other.def) ?? false) || (otherFactionDefExtension?.startingGoodwillByFactionDefs.Exists(x => x.factionDef == __instance.def) ?? false))
            {
                // Get the lowest range of goodwill possible between factions
                int? currentToOtherFactionGoodwillMin = currentFactionDefExtension?.startingGoodwillByFactionDefs?.Find(x => x.factionDef == other.def)?.Min ?? null;
                int? currentToOtherFactionGoodwillMax = currentFactionDefExtension?.startingGoodwillByFactionDefs?.Find(x => x.factionDef == other.def)?.Max ?? null;
                int? otherToCurrentFactionGoodwillMin = otherFactionDefExtension?.startingGoodwillByFactionDefs?.Find(x => x.factionDef == __instance.def)?.Min ?? null;
                int? otherToCurrentFactionGoodwillMax = otherFactionDefExtension?.startingGoodwillByFactionDefs?.Find(x => x.factionDef == __instance.def)?.Max ?? null;

                int mutualGoodwillMin = MinOfNullableInts(currentToOtherFactionGoodwillMin, otherToCurrentFactionGoodwillMin);

                int mutualGoodwillMax = MinOfNullableInts(currentToOtherFactionGoodwillMax, otherToCurrentFactionGoodwillMax);

                // Generate a random goodwill value within the range
                int finalMutualGoodWill = Rand.RangeInclusive(mutualGoodwillMin, mutualGoodwillMax);

                // Assign mutual faction relations
                FactionRelationKind kind = (finalMutualGoodWill > -10) ? ((finalMutualGoodWill < 75) ? FactionRelationKind.Neutral : FactionRelationKind.Ally) : FactionRelationKind.Hostile;

                FactionRelation factionRelation = __instance.RelationWith(other, false);
                factionRelation.goodwill = finalMutualGoodWill;
                factionRelation.kind = kind;
                FactionRelation factionRelation2 = other.RelationWith(__instance, false);
                factionRelation2.goodwill = finalMutualGoodWill;
                factionRelation2.kind = kind;
            }
        }

        static int MinOfNullableInts(int? num1, int? num2)
        {
            if (num1.HasValue && num2.HasValue)
            {
                return (num1 < num2) ? (int)num1 : (int)num2;
            }
            if (num1.HasValue && !num2.HasValue)
            {
                return (int)num1;
            }
            if (!num1.HasValue && num2.HasValue)
            {
                return (int)num2;
            }
            return 0;
        }
    }
}
