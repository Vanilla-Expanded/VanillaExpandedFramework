using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace MVCF.Utilities;

public static class PawnVerbUtility
{
    private static readonly ConditionalWeakTable<Pawn, VerbManager> managers = new();

    public static VerbManager Manager(this Pawn p, bool createIfMissing = true)
    {
        if (p == null) return null;
        if (managers.TryGetValue(p, out var manager) && manager is not null) return manager;
        if (!createIfMissing) return null;
        manager = new VerbManager();
        manager.Initialize(p);
        managers.Add(p, manager);
        return manager;
    }

    public static void SaveManager(this Pawn pawn)
    {
        if (managers.TryGetValue(pawn, out var man)) managers.Remove(pawn);
        else man = null;
        Scribe_Deep.Look(ref man, "MVCF_VerbManager");
        if (man is null) return;
        managers.Add(pawn, man);
        if (Scribe.mode == LoadSaveMode.PostLoadInit) man.Initialize(pawn);
    }


    // private static void PrepatchedSaveManager(Pawn p)
    // {
    //     Scribe_Deep.Look(ref p.MVCF_VerbManager, "MVCF_VerbManager");
    //     if (Scribe.mode == LoadSaveMode.PostLoadInit) p.MVCF_VerbManager?.Initialize(p);
    // }

    public static Verb BestVerbForTarget(this Pawn p, LocalTargetInfo target, IEnumerable<ManagedVerb> verbs) =>
        p.Manager().ChooseVerb(target, verbs.ToList())?.Verb;

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
