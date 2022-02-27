using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF.Features.PatchSets
{
    public class PatchSet_VerbUtility : PatchSet
    {
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

        public override IEnumerable<Patch> GetPatches()
        {
            yield return Patch.Prefix(AccessTools.Method(typeof(VerbUtility), nameof(VerbUtility.IsEMP)), AccessTools.Method(GetType(), nameof(IsEMP_Prefix)));
            yield return Patch.Prefix(AccessTools.Method(typeof(VerbUtility), nameof(VerbUtility.IsIncendiary)), AccessTools.Method(GetType(), nameof(IsIncendiary_Prefix)));
            yield return Patch.Prefix(AccessTools.Method(typeof(VerbUtility), nameof(VerbUtility.UsesExplosiveProjectiles)),
                AccessTools.Method(GetType(), nameof(UsesExplosiveProjectiles_Prefix)));
            yield return Patch.Prefix(AccessTools.Method(typeof(VerbUtility), nameof(VerbUtility.ProjectileFliesOverhead)),
                AccessTools.Method(GetType(), nameof(ProjectileFliesOverhead_Prefix)));
            yield return Patch.Prefix(AccessTools.Method(typeof(VerbUtility), nameof(VerbUtility.HarmsHealth)), AccessTools.Method(GetType(), nameof(HarmsHealth_Prefix)));
        }
    }
}