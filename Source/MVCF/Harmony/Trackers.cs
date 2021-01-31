using HarmonyLib;
using MVCF.Comps;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.Harmony
{
    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
    public class Pawn_ApparelTracker_Notify_ApparelAdded
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            var comp = apparel.TryGetComp<Comp_VerbGiver>();
            if (comp == null) return;
            comp.Notify_Worn(__instance.pawn);
            var manager = __instance.pawn?.Manager();
            if (manager == null) return;
            foreach (var verb in comp.VerbTracker.AllVerbs)
                manager.AddVerb(verb, VerbSource.Apparel, comp.PropsFor(verb));
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
    public class Pawn_ApparelTracker_Notify_ApparelRemoved
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(Apparel apparel, Pawn_ApparelTracker __instance)
        {
            var comp = apparel.TryGetComp<Comp_VerbGiver>();
            if (comp == null) return;
            comp.Notify_Unworn();
            var manager = __instance.pawn?.Manager();
            if (manager == null) return;
            foreach (var verb in comp.VerbTracker.AllVerbs) manager.RemoveVerb(verb);
        }
    }

    public class TrackerPatches
    {
        public static void Apparel(HarmonyLib.Harmony harm)
        {
            var type = typeof(Pawn_ApparelTracker);
            harm.Patch(AccessTools.Method(type, "Notify_ApparelAdded"),
                postfix: new HarmonyMethod(typeof(Pawn_ApparelTracker_Notify_ApparelAdded), "Postfix"));
            harm.Patch(AccessTools.Method(type, "Notify_ApparelRemoved"),
                postfix: new HarmonyMethod(typeof(Pawn_ApparelTracker_Notify_ApparelRemoved), "Postfix"));
        }

        public static void Hediffs(HarmonyLib.Harmony harm)
        {
            var type = typeof(Pawn_HealthTracker);
            harm.Patch(AccessTools.Method(type, "AddHediff", new[]
                {
                    typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo),
                    typeof(DamageWorker.DamageResult)
                }),
                postfix: new HarmonyMethod(typeof(Pawn_HealthTracker_AddHediff), "Postfix"));
            harm.Patch(AccessTools.Method(type, "RemoveHediff"),
                postfix: new HarmonyMethod(typeof(Pawn_HealthTracker_RemoveHediff), "Postfix"));
        }

        public static void Equipment(HarmonyLib.Harmony harm)
        {
            var type = typeof(Pawn_EquipmentTracker);
            harm.Patch(AccessTools.Method(type, "Notify_EquipmentAdded"),
                postfix: new HarmonyMethod(typeof(Pawn_EquipmentTracker_Notify_EquipmentAdded), "Postfix"));
            harm.Patch(AccessTools.Method(type, "Notify_EquipmentRemoved"),
                postfix: new HarmonyMethod(typeof(Pawn_EquipmentTracker_Notify_EquipmentRemoved), "Postfix"));
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo),
        typeof(DamageWorker.DamageResult))]
    public class Pawn_HealthTracker_AddHediff
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(Hediff hediff, Pawn_HealthTracker __instance)
        {
            var comp = hediff.TryGetComp<HediffComp_VerbGiver>();
            if (comp == null) return;
            var pawn = __instance.hediffSet.pawn;
            var manager = pawn?.Manager();
            if (manager == null) return;
            var extComp = comp as HediffComp_ExtendedVerbGiver;
            foreach (var verb in comp.VerbTracker.AllVerbs)
                manager.AddVerb(verb, VerbSource.Hediff, extComp?.PropsFor(verb));
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "RemoveHediff")]
    public class Pawn_HealthTracker_RemoveHediff
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(Hediff hediff, Pawn_HealthTracker __instance)
        {
            var comp = hediff.TryGetComp<HediffComp_VerbGiver>();
            if (comp == null) return;
            var pawn = __instance.hediffSet.pawn;
            var manager = pawn?.Manager();
            if (manager == null) return;
            foreach (var verb in comp.VerbTracker.AllVerbs) manager.RemoveVerb(verb);
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentAdded")]
    public class Pawn_EquipmentTracker_Notify_EquipmentAdded
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(ThingWithComps eq, Pawn_EquipmentTracker __instance)
        {
            var comp = eq.TryGetComp<CompEquippable>();
            if (comp == null)
            {
                var extComp = eq.TryGetComp<Comp_VerbGiver>();
                if (extComp == null) return;
                var manager = __instance.pawn?.Manager();
                if (manager == null) return;
                foreach (var verb in extComp.VerbTracker.AllVerbs)
                    manager.AddVerb(verb, VerbSource.Equipment, extComp.PropsFor(verb));
            }
            else
            {
                var manager = __instance.pawn?.Manager();
                if (manager == null) return;
                foreach (var verb in comp.VerbTracker.AllVerbs) manager.AddVerb(verb, VerbSource.Equipment, null);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentRemoved")]
    public class Pawn_EquipmentTracker_Notify_EquipmentRemoved
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(ThingWithComps eq, Pawn_EquipmentTracker __instance)
        {
            var comp = eq.TryGetComp<CompEquippable>();
            if (comp == null) return;
            var manager = __instance.pawn?.Manager();
            if (manager == null) return;
            foreach (var verb in comp.VerbTracker.AllVerbs) manager.RemoveVerb(verb);
        }
    }
}