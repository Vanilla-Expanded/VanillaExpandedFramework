using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using Verse;

namespace MVCF.PatchSets.Trackers;

public class PatchSet_Hediffs : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Prefix(AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.GetForceMissFactorFor)),
            AccessTools.Method(GetType(), nameof(GetForceMissFactorFor_Prefix)));
        yield return Patch.Postfix(AccessTools.Method(typeof(Pawn_HealthTracker), "AddHediff", new[]
            {
                typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo),
                typeof(DamageWorker.DamageResult)
            }),
            AccessTools.Method(GetType(), nameof(AddHediff_Postfix)));
        yield return Patch.Prefix(AccessTools.Method(typeof(Hediff), "PostRemoved"),
            AccessTools.Method(GetType(), nameof(PostRemoved_Prefix)));
    }

    public static bool GetForceMissFactorFor_Prefix(ref float __result, Thing equipment)
    {
        if (equipment is not null) return true;
        __result = 1f;
        return false;
    }

    public static void AddHediff_Postfix(Hediff hediff, Pawn_HealthTracker __instance)
    {
        __instance.hediffSet.pawn.Manager(false)?.AddVerbs(hediff);
    }

    public static void PostRemoved_Prefix(Hediff __instance)
    {
        if (MVCF.IsIgnoredMod(__instance.def?.modContentPack?.Name)) return;
        var comp = __instance.TryGetComp<HediffComp_VerbGiver>();
        if (comp?.VerbTracker?.AllVerbs == null) return;
        var manager = __instance.pawn.Manager(false);
        if (manager == null) return;
        foreach (var verb in comp.VerbTracker.AllVerbs.Concat(manager.ExtraVerbsFor(__instance))) manager.RemoveVerb(verb);
    }
}
