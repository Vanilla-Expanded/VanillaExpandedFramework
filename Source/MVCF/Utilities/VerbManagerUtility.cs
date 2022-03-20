using System.Linq;
using MVCF.Comps;
using MVCF.Features;
using MVCF.ModCompat;
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
            if (DualWield_Interop.Active && DualWield_Interop.IsOffHand(eq)) return;
            var comp = eq?.TryGetComp<CompEquippable>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
            if (Base.GetFeature<Feature_ExtraEquipmentVerbs>().Enabled)
                foreach (var verb in comp.VerbTracker.AllVerbs.Concat(man.ExtraVerbsFor(eq)))
                    man.AddVerb(verb, VerbSource.Equipment);
            else if (eq is {def: {equipmentType: EquipmentType.Primary}})
                man.AddVerb(comp.PrimaryVerb, VerbSource.Equipment);
        }

        public static void AddVerbs(this VerbManager man, Apparel apparel)
        {
            if (man == null) return;
            if (Base.IsIgnoredMod(apparel?.def?.modContentPack?.Name)) return;
            if (Base.ShouldIgnore(apparel)) return;
            var comp = apparel?.TryGetComp<Comp_VerbGiver>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
            comp.Notify_Worn(man.Pawn);
            foreach (var verb in comp.VerbTracker.AllVerbs.Concat(man.ExtraVerbsFor(apparel)))
                man.AddVerb(verb, VerbSource.Apparel);
        }

        public static void AddVerbs(this VerbManager man, Hediff hediff)
        {
            if (man == null) return;
            if (Base.IsIgnoredMod(hediff?.def?.modContentPack?.Name)) return;
            var comp = hediff.TryGetComp<HediffComp_VerbGiver>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
            foreach (var verb in comp.VerbTracker.AllVerbs.Concat(man.ExtraVerbsFor(hediff)))
                man.AddVerb(verb, VerbSource.Hediff);
        }

        public static void AddVerbs(this VerbManager man, Thing item)
        {
            if (man == null) return;
            if (Base.IsIgnoredMod(item?.def?.modContentPack?.Name)) return;
            if (Base.ShouldIgnore(item)) return;
            var comp = item?.TryGetComp<CompVerbsFromInventory>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
            comp.Notify_PickedUp(man.Pawn);
            foreach (var verb in comp.VerbTracker.AllVerbs.Concat(man.ExtraVerbsFor(item)))
                man.AddVerb(verb, VerbSource.Inventory);
        }
    }
}