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
            __instance.pawn.Manager().AddVerbs(apparel);
        }

        public static void ApparelRemoved_Postfix(Apparel apparel, Pawn_ApparelTracker __instance)
        {
            if (Base.IsIgnoredMod(apparel?.def?.modContentPack?.Name)) return;
            var comp = apparel.TryGetComp<Comp_VerbGiver>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
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
            __instance.hediffSet.pawn.Manager().AddVerbs(hediff);
        }

        public static void RemoveHediff_Postfix(Hediff hediff, Pawn_HealthTracker __instance)
        {
            if (Base.IsIgnoredMod(hediff?.def?.modContentPack?.Name)) return;
            var comp = hediff.TryGetComp<HediffComp_VerbGiver>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
            var manager = __instance.hediffSet.pawn?.Manager();
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
            __instance.pawn.Manager()?.AddVerbs(eq);
        }

        public static void EquipmentRemoved_Postfix(ThingWithComps eq, Pawn_EquipmentTracker __instance)
        {
            if (Base.IsIgnoredMod(eq?.def?.modContentPack?.Name)) return;
            var comp = eq.TryGetComp<CompEquippable>();
            if (comp?.VerbTracker?.AllVerbs == null) return;
            var manager = __instance?.pawn?.Manager();
            if (manager == null) return;
            foreach (var verb in comp.VerbTracker.AllVerbs) manager.RemoveVerb(verb);
        }
    }
}