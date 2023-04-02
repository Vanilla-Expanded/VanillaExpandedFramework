using System.Collections.Generic;
using MVCF.Comps;
using Verse;

namespace MVCF.Utilities;

public static class MeleeVerbUtility
{
    private static readonly Dictionary<ThingWithComps, bool> preferMeleeCache =
        new();

    public static bool PrefersMelee(this ThingWithComps eq)
    {
        if (eq == null) return false;
        if (preferMeleeCache.TryGetValue(eq, out var res)) return res;

        res = (eq.TryGetComp<CompEquippable>()?.props as CompProperties_VerbProps ??
               eq.TryGetComp<Comp_VerbProps>()?.Props)?.ConsiderMelee ?? false;
        preferMeleeCache.Add(eq, res);
        return res;
    }
}
