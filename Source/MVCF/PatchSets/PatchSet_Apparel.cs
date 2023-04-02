using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Comps;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.PatchSets;

public class PatchSet_Apparel : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        var type = typeof(Pawn_ApparelTracker);
        yield return Patch.Postfix(AccessTools.Method(type, "Notify_ApparelAdded"),
            AccessTools.Method(GetType(), nameof(ApparelAdded_Postfix)));
        yield return Patch.Prefix(AccessTools.Method(type, "Notify_ApparelRemoved"),
            AccessTools.Method(GetType(), nameof(ApparelRemoved_Prefix)));
    }

    public static void ApparelAdded_Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
    {
        __instance.pawn.Manager(false)?.AddVerbs(apparel);
    }

    public static void ApparelRemoved_Prefix(Apparel apparel, Pawn_ApparelTracker __instance)
    {
        if (MVCF.IsIgnoredMod(apparel?.def?.modContentPack?.Name)) return;
        var comp = apparel.TryGetComp<Comp_VerbGiver>();
        if (comp?.VerbTracker?.AllVerbs == null) return;
        comp.Notify_Unworn();
        var manager = __instance.pawn?.Manager(false);
        if (manager == null) return;
        foreach (var verb in comp.VerbTracker.AllVerbs.Concat(manager.ExtraVerbsFor(apparel))) manager.RemoveVerb(verb);
    }
}
