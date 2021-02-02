using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.Harmony
{
    public class VerbUtilityPatches
    {
        public static void DoPatches(HarmonyLib.Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(VerbUtility), "IsEMP"),
                new HarmonyMethod(typeof(VerbUtilityPatches), "IsEMP_Prefix"));
            harm.Patch(AccessTools.Method(typeof(VerbUtility), "IsIncendiary"),
                new HarmonyMethod(typeof(VerbUtilityPatches), "IsIncendiary_Prefix"));
            harm.Patch(AccessTools.Method(typeof(VerbUtility), "UsesExplosiveProjectiles"),
                new HarmonyMethod(typeof(VerbUtilityPatches), "UsesExplosiveProjectiles_Prefix"));
            harm.Patch(AccessTools.Method(typeof(VerbUtility), "ProjectileFliesOverhead"),
                new HarmonyMethod(typeof(VerbUtilityPatches), "ProjectileFliesOverhead_Prefix"));
            harm.Patch(AccessTools.Method(typeof(VerbUtility), "HarmsHealth"),
                new HarmonyMethod(typeof(VerbUtilityPatches), "HarmsHealth_Prefix"));
        }

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