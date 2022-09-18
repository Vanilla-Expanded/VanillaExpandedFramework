using System.Collections.Generic;
using System.Linq;
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

        public static Verb BestVerbForTarget(this Pawn p, LocalTargetInfo target, IEnumerable<ManagedVerb> verbs) => p.Manager().ChooseVerb(target, verbs.ToList())?.Verb;

        public static int GetDamage(this Verb verb)
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