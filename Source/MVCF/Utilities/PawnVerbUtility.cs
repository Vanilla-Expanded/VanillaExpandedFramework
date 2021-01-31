using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MVCF.Utilities
{
    public static class PawnVerbUtility
    {
        public static VerbManager Manager(this Pawn p, bool createIfMissing = true)
        {
            return Base.Prepatcher
                ? PrepatchedVerbManager(p, createIfMissing)
                : WorldComponent_MVCF.GetComp().GetManagerFor(p, createIfMissing);
        }

        public static VerbManager PrepatchedVerbManager(Pawn p, bool createIfMissing = true)
        {
            if (p.MVCF_VerbManager == null && createIfMissing)
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

        public static Verb BestVerbForTarget(this Pawn p, LocalTargetInfo target, IEnumerable<ManagedVerb> verbs,
            VerbManager man = null)
        {
            var debug = man?.debugOpts != null && man.debugOpts.ScoreLogging;
            if (!target.IsValid || !target.Cell.InBounds(p.Map))
            {
                Log.Error("[MVCF] BestVerbForTarget given invalid target with pawn " + p + " and target " + target);
                if (debug)
                    Log.Error("(Current job is " + p.CurJob + " with verb " + p.CurJob?.verbToUse + " and target " +
                              p.CurJob?.targetA + ")");
                return null;
            }

            Verb bestVerb = null;
            float bestScore = 0;
            foreach (var verb in verbs)
            {
                var score = VerbScore(p, verb.Verb, target, debug);
                if (debug) Log.Message("Score is " + score + " compared to " + bestScore);
                if (score <= bestScore) continue;
                bestScore = score;
                bestVerb = verb.Verb;
            }

            Log.Message("BestVerbForTarget returning " + bestVerb);
            return bestVerb;
        }

        private static float VerbScore(Pawn p, Verb verb, LocalTargetInfo target, bool debug = false)
        {
            if (debug) Log.Message("Getting score of " + verb + " with target " + target);
            var report = ShotReport.HitReportFor(p, verb, target);
            var damage = report.TotalEstimatedHitChance * verb.verbProps.burstShotCount * GetDamage(verb);
            var timeSpent = verb.verbProps.AdjustedCooldownTicks(verb, p) + verb.verbProps.warmupTime.SecondsToTicks();
            if (debug)
            {
                Log.Message("HitChance: " + report.TotalEstimatedHitChance);
                Log.Message("Damage: " + damage);
                Log.Message("timeSpent: " + timeSpent);
                Log.Message("Score of " + verb + " on target " + target + " is " + damage / timeSpent);
            }

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