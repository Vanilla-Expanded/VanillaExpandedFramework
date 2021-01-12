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
        public static bool Prefix(ref Verb __result, Pawn __instance, Thing target, bool allowManualCastWeapons = false)
        {
            var manager = __instance.Manager();
            // Log.Message("Getting attack verb for " + __instance + " with currentVerb " + manager.CurrentVerb?.Label() +
            //             " and target " + target + " and searchVerb " + manager.SearchVerb?.Label());

            if (target == null) manager.CurrentVerb = null;

            if (manager.CurrentVerb != null && manager.CurrentVerb.CanHitTarget(target))
            {
                __result = manager.CurrentVerb;
                return false;
            }

            if (target == null)
            {
                __result = manager.HasVerbs ? manager.SearchVerb : null;
                return false;
            }

            var verbs = manager.ManagedVerbs.Where(v =>
                !v.Verb.IsMeleeAttack && (v.Props == null || !v.Props.canFireIndependently) && v.Enabled);
            if (!allowManualCastWeapons) verbs = verbs.Where(v => !v.Verb.verbProps.onlyManualCast);

            var verbsToUse = verbs.ToList();
            if (verbsToUse.Count == 0) return true;

            var verbToUse = __instance.BestVerbForTarget(target, verbsToUse);

            if (verbToUse == null) return true;

            __result = verbToUse;

            return false;
        }

        // public static void Postfix(ref Verb __result, Pawn __instance, Thing target)
        // {
        //     Log.Message("TryGetAttackVerb returning " + __result?.Label() + " for " + __instance?.LabelShort +
        //                 " and target " + target?.LabelShort);
        // }
    }
}