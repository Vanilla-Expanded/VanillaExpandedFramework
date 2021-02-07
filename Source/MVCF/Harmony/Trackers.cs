using System.Linq;
using HarmonyLib;
using MVCF.Comps;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.Harmony
{
    public class Trackers
    {
        public static void Apparel(HarmonyLib.Harmony harm)
        {
            var type = typeof(Pawn_ApparelTracker);
            harm.Patch(AccessTools.Method(type, "Notify_ApparelAdded"),
                postfix: new HarmonyMethod(typeof(Trackers), "ApparelAdded_Postfix"));
            harm.Patch(AccessTools.Method(type, "Notify_ApparelRemoved"),
                postfix: new HarmonyMethod(typeof(Trackers), "ApparelRemoved_Postfix"));
        }

        public static void ApparelAdded_Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            if (Base.IgnoredMods.Contains(apparel.def.modContentPack.Name)) return;
            var comp = apparel.TryGetComp<Comp_VerbGiver>();
            if (comp == null) return;
            if (!Base.Features.ApparelVerbs && !Base.IgnoredFeatures.ApparelVerbs &&
                !Base.IgnoredMods.Contains(apparel.def.modContentPack.Name))
            {
                Log.ErrorOnce(
                    "[MVCF] Found apparel with a verb while that feature is not enabled. Enabling now. This is not recommend. Contact the author of " +
                    apparel.def.modContentPack.Name + " and ask them to add a MVCF.ModDef.",
                    apparel.def.modContentPack.Name.GetHashCode());
                Base.Features.ApparelVerbs = true;
                Base.ApplyPatches();
            }

            comp.Notify_Worn(__instance.pawn);
            var manager = __instance.pawn?.Manager();
            if (manager == null) return;
            foreach (var verb in comp.VerbTracker.AllVerbs)
                manager.AddVerb(verb, VerbSource.Apparel, comp.PropsFor(verb));
        }

        public static void ApparelRemoved_Postfix(Apparel apparel, Pawn_ApparelTracker __instance)
        {
            if (Base.IgnoredMods.Contains(apparel.def.modContentPack.Name)) return;
            var comp = apparel.TryGetComp<Comp_VerbGiver>();
            if (comp == null) return;
            comp.Notify_Unworn();
            var manager = __instance.pawn?.Manager();
            if (manager == null) return;
            foreach (var verb in comp.VerbTracker.AllVerbs) manager.RemoveVerb(verb);
        }

        public static void Hediffs(HarmonyLib.Harmony harm)
        {
            var type = typeof(Pawn_HealthTracker);
            harm.Patch(AccessTools.Method(type, "AddHediff", new[]
                {
                    typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo),
                    typeof(DamageWorker.DamageResult)
                }),
                postfix: new HarmonyMethod(typeof(Trackers), "AddHediff_Postfix"));
            harm.Patch(AccessTools.Method(type, "RemoveHediff"),
                postfix: new HarmonyMethod(typeof(Trackers), "RemoveHediff_Postfix"));
        }

        public static void AddHediff_Postfix(Hediff hediff, Pawn_HealthTracker __instance)
        {
            if (Base.IgnoredMods.Contains(hediff.def.modContentPack.Name)) return;
            var comp = hediff.TryGetComp<HediffComp_VerbGiver>();
            if (comp == null) return;
            if (!Base.Features.HediffVerbs && !Base.IgnoredFeatures.HediffVerbs &&
                comp.VerbTracker.AllVerbs.Any(v => !v.IsMeleeAttack) &&
                !Base.IgnoredMods.Contains(hediff.def.modContentPack.Name))
            {
                Log.ErrorOnce(
                    "[MVCF] Found a hediff with a ranged verb while that feature is not enabled. Enabling now. This is not recommend. Contant the author of " +
                    hediff.def.modContentPack.Name + " and ask them to add a MVCF.ModDef.",
                    hediff.def.modContentPack.Name.GetHashCode());
                Base.Features.HediffVerbs = true;
                Base.ApplyPatches();
            }

            var pawn = __instance.hediffSet.pawn;
            var manager = pawn?.Manager();
            if (manager == null) return;
            var extComp = comp as HediffComp_ExtendedVerbGiver;
            foreach (var verb in comp.VerbTracker.AllVerbs)
                manager.AddVerb(verb, VerbSource.Hediff, extComp?.PropsFor(verb));
        }

        public static void RemoveHediff_Postfix(Hediff hediff, Pawn_HealthTracker __instance)
        {
            if (Base.IgnoredMods.Contains(hediff.def.modContentPack.Name)) return;
            var comp = hediff.TryGetComp<HediffComp_VerbGiver>();
            if (comp == null) return;
            var pawn = __instance.hediffSet.pawn;
            var manager = pawn?.Manager();
            if (manager == null) return;
            foreach (var verb in comp.VerbTracker.AllVerbs) manager.RemoveVerb(verb);
        }

        public static void Equipment(HarmonyLib.Harmony harm)
        {
            var type = typeof(Pawn_EquipmentTracker);
            harm.Patch(AccessTools.Method(type, "Notify_EquipmentAdded"),
                postfix: new HarmonyMethod(typeof(Trackers), "EquipmentAdded_Postfix"));
            harm.Patch(AccessTools.Method(type, "Notify_EquipmentRemoved"),
                postfix: new HarmonyMethod(typeof(Trackers), "EquipmentRemoved_Postfix"));
        }

        public static void EquipmentAdded_Postfix(ThingWithComps eq, Pawn_EquipmentTracker __instance)
        {
            if (Base.IgnoredMods.Contains(eq.def.modContentPack.Name)) return;
            var comp = eq.TryGetComp<CompEquippable>();
            if (comp == null) return;
            var manager = __instance.pawn?.Manager();
            if (manager == null) return;
            if (!Base.Features.ExtraEquipmentVerbs && !Base.IgnoredFeatures.ExtraEquipmentVerbs &&
                comp.VerbTracker.AllVerbs.Count(v => !v.IsMeleeAttack) > 1 &&
                !Base.IgnoredMods.Contains(eq.def.modContentPack.Name))
            {
                Log.ErrorOnce(
                    "[MVCF] Found equipment with more than one ranged attack while that feature is not enabled. Enabling now. This is not recommend. Contact the author of " +
                    eq.def.modContentPack.Name + " and ask them to add a MVCF.ModDef.",
                    eq.def.modContentPack.Name.GetHashCode());
                Base.Features.ExtraEquipmentVerbs = true;
                Base.ApplyPatches();
            }

            foreach (var verb in comp.VerbTracker.AllVerbs)
                manager.AddVerb(verb, VerbSource.Equipment, (comp.props as CompProperties_VerbProps)?.PropsFor(verb));
        }

        public static void EquipmentRemoved_Postfix(ThingWithComps eq, Pawn_EquipmentTracker __instance)
        {
            if (Base.IgnoredMods.Contains(eq.def.modContentPack.Name)) return;
            var comp = eq.TryGetComp<CompEquippable>();
            if (comp == null) return;
            var manager = __instance.pawn?.Manager();
            if (manager == null) return;
            foreach (var verb in comp.VerbTracker.AllVerbs) manager.RemoveVerb(verb);
        }
    }
}