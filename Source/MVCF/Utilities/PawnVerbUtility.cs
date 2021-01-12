using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MVCF.Utilities
{
    public static class PawnVerbUtility
    {
        public static VerbManager Manager(this Pawn p)
        {
            return Base.Prepatcher ? PrepatchedVerbManager(p) : WorldComponent_MVCF.GetStorage().GetManagerFor(p);
        }

        public static VerbManager PrepatchedVerbManager(Pawn p)
        {
            if (p.MVCF_VerbManager == null)
            {
                p.MVCF_VerbManager = new VerbManager();
                p.MVCF_VerbManager.Initialize(p);
            }

            return p.MVCF_VerbManager;
        }

        public static IEnumerable<Verb> AllRangedVerbsPawn(this Pawn p)
        {
            return p.Manager().AllRangedVerbs;
        }

        public static IEnumerable<Verb> AllRangedVerbsPawnNoEquipment(this Pawn p)
        {
            return p.Manager().AllRangedVerbsNoEquipment;
        }

        public static IEnumerable<Verb> AllRangedVerbsPawnNoEquipmentNoApparel(this Pawn p)
        {
            return p.Manager().AllRangedVerbsNoEquipmentNoApparel;
        }

        public static Verb BestVerbForTarget(this Pawn p, LocalTargetInfo target, IEnumerable<ManagedVerb> verbs)
        {
//            Log.Message("BestVerbForTarget: " + p + ", " + target);
            Verb bestVerb = null;
            float bestScore = 0;
            foreach (var verb in verbs)
            {
                if (!(verb.Verb.CanHitTarget(target) && verb.Enabled)) continue;
                var score = VerbScore(p, verb.Verb, target);
//                Log.Message("    Verb " + verb.Label() + " has score " + score);
                if (score <= bestScore) continue;
                bestScore = score;
                bestVerb = verb.Verb;
            }

//            Log.Message("    Best verb is " + bestVerb?.Label());

            return bestVerb;
        }

        private static float VerbScore(Pawn p, Verb verb, LocalTargetInfo target)
        {
            var report = ShotReport.HitReportFor(p, verb, target);
            var damage = report.TotalEstimatedHitChance * verb.verbProps.burstShotCount * GetDamage(verb);
            var timeSpent = verb.verbProps.AdjustedCooldownTicks(verb, p) + verb.verbProps.warmupTime.SecondsToTicks();
            return damage / timeSpent;
        }

        private static int GetDamage(Verb verb)
        {
            switch (verb)
            {
                case Verb_LaunchProjectile launch:
                    return launch.Projectile.projectile.GetDamageAmount(1f);
                case Verb_Bombardment _:
                case Verb_PowerBeam _:
                case Verb_MechCluster _:
                    return int.MaxValue;
                case Verb_CastAbility cast:
                    return cast.ability.EffectComps.Count * 100;
                default:
                    return 1;
            }
        }
    }
}