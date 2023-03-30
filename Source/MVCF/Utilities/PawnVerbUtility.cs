using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Prepatcher;
using RimWorld;
using Verse;

namespace MVCF.Utilities;

public static class PawnVerbUtility
{
    private static readonly ConditionalWeakTable<Pawn, StrongBox<VerbManager>> managers = new();

    [PrepatcherField]
    private static ref VerbManager VerbManager(this Pawn pawn)
    {
        if (!managers.TryGetValue(pawn, out var box))
        {
            box = new StrongBox<VerbManager>();
            managers.Add(pawn, box);
        }

        return ref box.Value;
    }

    public static VerbManager Manager(this Pawn p, bool createIfMissing = true)
    {
        if (p == null) return null;
        ref var manager = ref p.VerbManager();
        if (manager is not null) return manager;
        if (!createIfMissing) return null;
        manager = new VerbManager();
        manager.Initialize(p);
        return manager;
    }

    public static void SaveManager(this Pawn pawn)
    {
        ref var manager = ref pawn.VerbManager();
        Scribe_Deep.Look(ref manager, "MVCF_VerbManager");
        if (manager is null) return;
        if (Scribe.mode == LoadSaveMode.PostLoadInit) manager.Initialize(pawn);
    }

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
