using System.Collections.Generic;
using MVCF.Comps;
using Verse;

namespace MVCF.Utilities;

public static class MeleeVerbUtility
{
    private static readonly Dictionary<ThingWithComps, bool> preferMeleeCache =
        new();

    public static bool BrawlerUpsetBy(this ManagedVerb mv) =>
        !mv.Verb.IsMeleeAttack && mv.Props is not { brawlerCaresAbout: false }
                               && !(mv.Source == VerbSource.Equipment && mv.Verb.EquipmentSource.PrefersMelee());

    public static bool PrefersMelee(this ThingWithComps eq)
    {
        if (eq == null) return false;
        if (preferMeleeCache.TryGetValue(eq, out var res)) return res;

        res = (eq.TryGetComp<CompEquippable>()?.props as CompProperties_VerbProps ??
               eq.TryGetComp<Comp_VerbProps>()?.Props)?.ConsiderMelee ?? false;
        preferMeleeCache.Add(eq, res);
        return res;
    }

    public static void AddAdditionalMeleeVerbs(this Pawn pawn, List<Verb> verbs)
    {
        verbs.AddRange(pawn.Manager().AdditionalMeleeVerbs);
        GenDebug.LogList(verbs);
    }
}
