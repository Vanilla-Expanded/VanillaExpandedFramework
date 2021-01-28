using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
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
            if (verb.verbProps.label.NullOrEmpty()) return true;
            if (verb.verbProps.label != Base.SearchLabel) return true;
            if (!(verb.caster is Pawn p)) return true;
            var man = p.Manager();
            if (man == null) return true;
            __result = man.ManagedVerbs.Any(v =>
                v.Enabled && verb.GetDamageDef() == null && verb.GetDamageDef() == DamageDefOf.EMP);
            return false;
        }

        [HarmonyPatch("IsIncendiary")]
        [HarmonyPrefix]
        public static bool IsIncendiary_Prefix(Verb verb, ref bool __result)
        {
            if (verb.verbProps.label.NullOrEmpty()) return true;
            if (verb.verbProps.label != Base.SearchLabel) return true;
            if (!(verb.caster is Pawn p)) return true;
            var man = p.Manager();
            if (man == null) return true;
            __result = man.ManagedVerbs.Any(v =>
                v.Enabled && verb.GetProjectile() is ThingDef proj && proj.projectile.ai_IsIncendiary);
            return false;
        }

        [HarmonyPatch("UsesExplosiveProjectiles")]
        [HarmonyPrefix]
        public static bool UsesExplosiveProjectiles_Prefix(Verb verb, ref bool __result)
        {
            if (verb.verbProps.label.NullOrEmpty()) return true;
            if (verb.verbProps.label != Base.SearchLabel) return true;
            if (!(verb.caster is Pawn p)) return true;
            var man = p.Manager();
            if (man == null) return true;
            __result = man.ManagedVerbs.Any(v =>
                v.Enabled && verb.GetProjectile() is ThingDef proj && proj.projectile.explosionRadius > 0f);
            return false;
        }

        [HarmonyPatch("ProjectileFliesOverhead")]
        [HarmonyPrefix]
        public static bool ProjectileFliesOverhead_Prefix(Verb verb, ref bool __result)
        {
            if (verb.verbProps.label.NullOrEmpty()) return true;
            if (verb.verbProps.label != Base.SearchLabel) return true;
            if (!(verb.caster is Pawn p)) return true;
            var man = p.Manager();
            if (man == null) return true;
            __result = man.ManagedVerbs.Any(v =>
                v.Enabled && verb.GetProjectile() is ThingDef proj && proj.projectile.flyOverhead);
            return false;
        }

        [HarmonyPatch("HarmsHealth")]
        [HarmonyPrefix]
        public static bool HarmsHealth_Prefix(Verb verb, ref bool __result)
        {
            if (verb.verbProps.label.NullOrEmpty()) return true;
            if (verb.verbProps.label != Base.SearchLabel) return true;
            if (!(verb.caster is Pawn p)) return true;
            var man = p.Manager();
            if (man == null) return true;
            __result = man.ManagedVerbs.Any(v =>
                v.Enabled && verb.GetDamageDef() == null && verb.GetDamageDef().harmsHealth);
            return false;
        }
    }
}