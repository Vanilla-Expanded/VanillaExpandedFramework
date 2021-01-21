using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using Verse;

// ReSharper disable InconsistentNaming

namespace MVCF.Harmony
{
    [HarmonyPatch(typeof(Pawn), "TryGetAttackVerb")]
    public class Pawn_TryGetAttackVerb
    {
        public static bool Prefix(ref Verb __result, Pawn __instance, Thing target,
            out bool __state, bool allowManualCastWeapons = false)
        {
            var manager = __instance.Manager();

            var job = __instance.CurJob;
            if (target == null && (job == null || !job.targetA.HasThing && job.targetA.Cell == __instance.Position))
                manager.CurrentVerb = null;

            if (manager.CurrentVerb != null && manager.CurrentVerb.Available() &&
                (target == null || manager.CurrentVerb.CanHitTarget(target)) &&
                (job == null || !job.targetA.HasThing || job.targetA.Cell == __instance.Position ||
                 manager.CurrentVerb.CanHitTarget(job.targetA)))
            {
                __result = manager.CurrentVerb;
                __state = false;
                return false;
            }

            if (target == null)
            {
                __result = manager.HasVerbs ? manager.SearchVerb : null;
                __state = __result == null || !__result.Available();
                return __state;
            }

            var verbs = manager.ManagedVerbs.Where(v =>
                !v.Verb.IsMeleeAttack && (v.Props == null || !v.Props.canFireIndependently) && v.Enabled &&
                v.Verb.Available() && v.Verb.CanHitTarget(target));
            if (!allowManualCastWeapons) verbs = verbs.Where(v => !v.Verb.verbProps.onlyManualCast);

            var verbsToUse = verbs.ToList();
            if (verbsToUse.Count == 0) return __state = true;

            var verbToUse = __instance.BestVerbForTarget(target, verbsToUse);

            if (verbToUse == null) return __state = true;

            __result = verbToUse;
            __state = false;

            return false;
        }

        public static void Postfix(ref Verb __result, bool __state)
        {
            if (__result == null) return;
            if (__result.verbProps.label == Base.SearchLabel && __state) __result = null;
        }
    }
}