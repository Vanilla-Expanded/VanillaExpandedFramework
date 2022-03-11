using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MVCF.Utilities
{
    public static class PawnVerbUtility
    {
        public static VerbManager Manager(this Pawn p, bool createIfMissing = true)
        {
            if (p == null) return null;
            return Base.Prepatcher
                ? PrepatchedVerbManager(p, createIfMissing)
                : WorldComponent_MVCF.Instance.GetManagerFor(p, createIfMissing);
        }

        private static VerbManager PrepatchedVerbManager(Pawn p, bool createIfMissing = true)
        {
            if (p.MVCF_VerbManager == null && createIfMissing)
            {
                p.MVCF_VerbManager = new VerbManager();
                p.MVCF_VerbManager.Initialize(p);
            }

            return p.MVCF_VerbManager;
        }

        public static void SaveManager(this Pawn p)
        {
            if (Base.Prepatcher) PrepatchedSaveManager(p);
            else WorldComponent_MVCF.Instance.SaveManager(p);
        }


        private static void PrepatchedSaveManager(Pawn p)
        {
            Scribe_Deep.Look(ref p.MVCF_VerbManager, "MVCF_VerbManager");
            if (Scribe.mode == LoadSaveMode.PostLoadInit) p.MVCF_VerbManager?.Initialize(p);
        }

        public static Verb BestVerbForTarget(this Pawn p, LocalTargetInfo target, IEnumerable<ManagedVerb> verbs,
            VerbManager man = null)
        {
            var debug = man?.debugOpts != null && man.debugOpts.ScoreLogging;
            if (!target.IsValid || p.Map != null && !target.Cell.InBounds(p.Map))
            {
                Log.Error("[MVCF] BestVerbForTarget given invalid target with pawn " + p + " and target " + target);
                if (debug)
                    Log.Error("(Current job is " + p.CurJob + " with verb " + p.CurJob?.verbToUse + " and target " +
                              p.CurJob?.targetA + ")");
                return null;
            }

            Verb bestVerb = null;
            var bestScore = 0f;
            foreach (var verb in verbs)
            {
                if (verb.Verb is IVerbScore verbScore && verbScore.ForceUse(p, target)) return verb.Verb;
                var score = VerbScore(p, verb.Verb, target, debug);
                if (debug) Log.Message("Score is " + score + " compared to " + bestScore);
                if (score <= bestScore) continue;
                bestScore = score;
                bestVerb = verb.Verb;
            }

            if (debug) Log.Message("BestVerbForTarget returning " + bestVerb);
            return bestVerb;
        }

        public static float VerbScore(Pawn p, Verb verb, LocalTargetInfo target, bool debug = false)
        {
            if (debug) Log.Message("Getting score of " + verb + " with target " + target);
            if (verb is IVerbScore score) return score.GetScore(p, target);
            var accuracy = 0f;
            if (p.Map != null)
                accuracy = ShotReport.HitReportFor(p, verb, target).TotalEstimatedHitChance;
            else if (verb.TryFindShootLineFromTo(p.Position, target, out var line))
                accuracy = verb.verbProps.GetHitChanceFactor(verb.EquipmentSource,
                    p.Position.DistanceTo(target.Cell));

            var damage = accuracy * verb.verbProps.burstShotCount * GetDamage(verb);
            var timeSpent = verb.verbProps.AdjustedCooldownTicks(verb, p) + verb.verbProps.warmupTime.SecondsToTicks();
            if (debug)
            {
                Log.Message("Accuracy: " + accuracy);
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

    public interface IVerbScore
    {
        float GetScore(Pawn pawn, LocalTargetInfo target);
        bool ForceUse(Pawn pawn, LocalTargetInfo target);
    }
}