using System.Linq;
using MVCF.Comps;
using MVCF.Harmony;
using RimWorld;
using Verse;
using Log = Verse.Log;

namespace MVCF.Utilities
{
    public static class VerbManagerUtility
    {
        public static void AddVerbs(this VerbManager man, ThingWithComps eq)
        {
            if (man == null) return;
            if (Base.IsIgnoredMod(eq?.def?.modContentPack?.Name)) return;
            if (Compat.ShouldIgnore(eq)) return;
            var comp = eq.TryGetComp<CompEquippable>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
            if (!Base.Features.ExtraEquipmentVerbs && !Base.IgnoredFeatures.ExtraEquipmentVerbs &&
                comp.VerbTracker.AllVerbs.Count(v => !v.IsMeleeAttack) > 1)
            {
                Log.ErrorOnce(
                    "[MVCF] Found equipment with more than one ranged attack while that feature is not enabled. Enabling now. This is not recommend. Contact the author of " +
                    eq?.def?.modContentPack?.Name + " and ask them to add a MVCF.ModDef.",
                    eq?.def?.modContentPack?.Name?.GetHashCode() ?? -1);
                Base.Features.ExtraEquipmentVerbs = true;
                Base.ApplyPatches();
            }

            foreach (var verb in comp.VerbTracker.AllVerbs)
                man.AddVerb(verb, VerbSource.Equipment, comp.props is CompProperties_VerbProps props
                    ? props.PropsFor(verb)
                    : eq.TryGetComp<Comp_VerbProps>()?.Props?.PropsFor(verb));
        }

        public static void AddVerbs(this VerbManager man, Apparel apparel)
        {
            if (man == null) return;
            if (Base.IsIgnoredMod(apparel?.def?.modContentPack?.Name)) return;
            if (Compat.ShouldIgnore(apparel)) return;
            var comp = apparel.TryGetComp<Comp_VerbGiver>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
            if (!Base.Features.ApparelVerbs && !Base.IgnoredFeatures.ApparelVerbs)
            {
                Log.ErrorOnce(
                    "[MVCF] Found apparel with a verb while that feature is not enabled. Enabling now. This is not recommend. Contact the author of " +
                    apparel?.def?.modContentPack?.Name + " and ask them to add a MVCF.ModDef.",
                    apparel?.def?.modContentPack?.Name?.GetHashCode() ?? -1);
                Base.Features.ApparelVerbs = true;
                Base.ApplyPatches();
            }

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
            if (!Base.Features.HediffVerbs && !Base.IgnoredFeatures.HediffVerbs &&
                comp.VerbTracker.AllVerbs.Any(v => !v.IsMeleeAttack))
            {
                Log.ErrorOnce(
                    "[MVCF] Found a hediff with a ranged verb while that feature is not enabled. Enabling now. This is not recommend. Contant the author of " +
                    hediff?.def?.modContentPack?.Name + " and ask them to add a MVCF.ModDef.",
                    hediff?.def?.modContentPack?.Name?.GetHashCode() ?? -1);
                Base.Features.HediffVerbs = true;
                Base.ApplyPatches();
            }

            var extComp = comp as HediffComp_ExtendedVerbGiver;
            foreach (var verb in comp.VerbTracker.AllVerbs)
                man.AddVerb(verb, VerbSource.Hediff, extComp?.PropsFor(verb));
        }
    }
}