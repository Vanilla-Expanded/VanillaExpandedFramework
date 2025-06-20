using System.Collections.Generic;
using MVCF.Comps;
using Verse;

namespace MVCF.Utilities;

public static class MeleeVerbUtility
{

    public static bool BrawlerUpsetBy(this ManagedVerb mv) =>
        !mv.Verb.IsMeleeAttack && mv.Props is not { brawlerCaresAbout: false };

    public static void AddAdditionalMeleeVerbs(this Pawn pawn, List<Verb> verbs)
    {
        verbs.AddRange(pawn.Manager().AdditionalMeleeVerbs);
        GenDebug.LogList(verbs);
    }
}
