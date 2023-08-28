using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MVCF.Comps;

public class CompRandomVerbs : VerbManagerComp
{
    public override bool ChooseVerb(Dictionary<ManagedVerb, LocalTargetInfo> options, out ManagedVerb verb)
    {
        Rand.PushState(Manager.Pawn.HashOffset() ^ (GenTicks.TicksGame / GenDate.TicksPerHour));
        try
        {
            if (Rand.Chance((props as CompProperties_RandomVerbs)?.meleeChance ?? 0f))
            {
                verb = null;
                return true;
            }

            if (options.TryRandomElementByWeight(kv => kv.Key.Verb.verbProps.commonality, out var result))
            {
                verb = result.Key;
                return true;
            }

            verb = null;
            return false;
        }
        finally { Rand.PopState(); }
    }
}

public class CompProperties_RandomVerbs : CompProperties
{
    public float meleeChance;
    public CompProperties_RandomVerbs() => compClass = typeof(CompRandomVerbs);
}
