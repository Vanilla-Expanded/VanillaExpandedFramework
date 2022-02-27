using MVCF.Comps;
using RimWorld;
using Verse;

namespace MVCF.Utilities
{
    public static class VerbManagerUtility
    {
        public static void AddVerbs(this VerbManager man, ThingWithComps eq)
        {
            if (man == null) return;
            if (Base.IsIgnoredMod(eq?.def?.modContentPack?.Name)) return;
            if (Base.ShouldIgnore(eq)) return;
            var comp = eq.TryGetComp<CompEquippable>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
            foreach (var verb in comp.VerbTracker.AllVerbs)
                man.AddVerb(verb, VerbSource.Equipment, comp.props is CompProperties_VerbProps props
                    ? props.PropsFor(verb)
                    : eq.TryGetComp<Comp_VerbProps>()?.Props?.PropsFor(verb));
        }

        public static void AddVerbs(this VerbManager man, Apparel apparel)
        {
            if (man == null) return;
            if (Base.IsIgnoredMod(apparel?.def?.modContentPack?.Name)) return;
            if (Base.ShouldIgnore(apparel)) return;
            var comp = apparel.TryGetComp<Comp_VerbGiver>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
            comp.Notify_Worn(man.Pawn);
            foreach (var verb in comp.VerbTracker.AllVerbs)
                man.AddVerb(verb, VerbSource.Apparel, comp.PropsFor(verb));
        }

        public static void AddVerbs(this VerbManager man, Hediff hediff)
        {
            if (man == null) return;
            if (Base.IsIgnoredMod(hediff?.def?.modContentPack?.Name)) return;
            var comp = hediff.TryGetComp<HediffComp_VerbGiver>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
            var extComp = comp as HediffComp_ExtendedVerbGiver;
            foreach (var verb in comp.VerbTracker.AllVerbs)
                man.AddVerb(verb, VerbSource.Hediff, extComp?.PropsFor(verb));
        }
    }
}