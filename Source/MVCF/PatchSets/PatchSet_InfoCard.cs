using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.PatchSets;

public class PatchSet_InfoCard : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Postfix(AccessTools.Method(typeof(ThingDef), nameof(ThingDef.SpecialDisplayStats)),
            AccessTools.Method(GetType(), nameof(ReplaceVerbStatDisplay)));
    }

    public static IEnumerable<StatDrawEntry> ReplaceVerbStatDisplay(IEnumerable<StatDrawEntry> entries, StatRequest req, ThingDef __instance,
        List<VerbProperties> ___verbs)
    {
        var category = __instance.category == ThingCategory.Pawn ? StatCategoryDefOf.PawnCombat : StatCategoryDefOf.Weapon_Ranged;
        return ___verbs is not { Count: > 1 }
            ? entries
            : entries.Where(entry => entry.category != category).Concat(VerbStatsUtility.DisplayStatsForVerbs(___verbs, category, req));
    }
}
