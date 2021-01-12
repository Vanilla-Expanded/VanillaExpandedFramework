using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using Verse;

namespace MVCF.Harmony
{
    [HarmonyPatch(typeof(VerbUtility))]
    public class VerbUtilityPatches
    {
        [HarmonyPatch("IsEMP")]
        [HarmonyPrefix]
        public static bool IsEMP_Prefix(Verb verb, ref bool __result)
        {
            if (verb.verbProps.label == null) return true;
            if (verb.verbProps.label != Base.SearchLabel) return true;
            if (!(verb.caster is Pawn p)) return true;
            var man = p.Manager();
            __result = man.ManagedVerbs.Any(v => v.Enabled && v.Verb.IsEMP());
            return false;
        }

        [HarmonyPatch("IsIncendiary")]
        [HarmonyPrefix]
        public static bool IsIncendiary_Prefix(Verb verb, ref bool __result)
        {
            if (verb.verbProps.label == null) return true;
            if (verb.verbProps.label != Base.SearchLabel) return true;
            if (!(verb.caster is Pawn p)) return true;
            var man = p.Manager();
            __result = man.ManagedVerbs.Any(v => v.Enabled && v.Verb.IsIncendiary());
            return false;
        }

        [HarmonyPatch("UsesExplosiveProjectiles")]
        [HarmonyPrefix]
        public static bool UsesExplosiveProjectiles_Prefix(Verb verb, ref bool __result)
        {
            if (verb.verbProps.label == null) return true;
            if (verb.verbProps.label != Base.SearchLabel) return true;
            if (!(verb.caster is Pawn p)) return true;
            var man = p.Manager();
            __result = man.ManagedVerbs.Any(v => v.Enabled && v.Verb.UsesExplosiveProjectiles());
            return false;
        }

        [HarmonyPatch("ProjectileFliesOverhead")]
        [HarmonyPrefix]
        public static bool ProjectileFliesOverhead_Prefix(Verb verb, ref bool __result)
        {
            if (verb.verbProps.label == null) return true;
            if (verb.verbProps.label != Base.SearchLabel) return true;
            if (!(verb.caster is Pawn p)) return true;
            var man = p.Manager();
            __result = man.ManagedVerbs.Any(v => v.Enabled && v.Verb.ProjectileFliesOverhead());
            return false;
        }

        [HarmonyPatch("HarmsHealth")]
        [HarmonyPrefix]
        public static bool HarmsHealth_Prefix(Verb verb, ref bool __result)
        {
            if (verb.verbProps.label == null) return true;
            if (verb.verbProps.label != Base.SearchLabel) return true;
            if (!(verb.caster is Pawn p)) return true;
            var man = p.Manager();
            __result = man.ManagedVerbs.Any(v => v.Enabled && v.Verb.HarmsHealth());
            return false;
        }
    }
}