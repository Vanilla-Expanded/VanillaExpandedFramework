using System.Collections.Generic;
using System.Linq;
using MVCF.Comps;
using MVCF.Reloading.Comps;
using Verse;

namespace MVCF.Utilities;

public static class ReloadingUtility
{
    public static IEnumerable<VerbComp_Reloadable> AllReloadComps(this Pawn p) =>
        p.Manager().ManagedVerbs.SelectMany(mv => mv.GetComps()).OfType<VerbComp_Reloadable>();

    public static IEnumerable<VerbComp_Reloadable> AllReloadComps(this Thing t)
    {
        if (t is not ThingWithComps twc) yield break;
        foreach (var comp in twc.AllComps)
            switch (comp)
            {
                case CompEquippable eq:
                    foreach (var verbComp in eq.AllVerbs.SelectMany(verb => verb.Managed().GetComps()).OfType<VerbComp_Reloadable>())
                        yield return verbComp;
                    break;
                case Comp_VerbGiver giver:
                    foreach (var reloadable in giver.VerbTracker.AllVerbs.SelectMany(verb => verb.Managed().GetComps()).OfType<VerbComp_Reloadable>())
                        yield return reloadable;
                    break;
            }
    }

    public static VerbComp_Reloadable GetReloadable(this Verb verb) => verb.Managed().TryGetComp<VerbComp_Reloadable>();
}
