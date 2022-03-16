using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MVCF.Comps
{
    public class CompRandomVerbs : VerbManagerComp
    {
        public override bool ChooseVerb(LocalTargetInfo target, List<ManagedVerb> verbs, out ManagedVerb verb)
        {
            Rand.PushState(Manager.Pawn.HashOffset() ^ (GenTicks.TicksGame / GenDate.TicksPerHour));
            verb = null;
            var result = Rand.Chance((props as CompProperties_RandomVerbs)?.meleeChance ?? 0f) || verbs.TryRandomElementByWeight(v => v.Verb.verbProps.commonality, out verb);
            Rand.PopState();
            return result;
        }
    }

    public class CompProperties_RandomVerbs : CompProperties
    {
        public float meleeChance;
        public CompProperties_RandomVerbs() => compClass = typeof(CompRandomVerbs);
    }
}