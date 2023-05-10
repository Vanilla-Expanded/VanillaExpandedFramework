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

    public static Verb GetAttackVerb(this Pawn pawn, Thing target, bool allowManualCastWeapons = false)
    {
        var manager = pawn.Manager();
        var job = pawn.CurJob;

        MVCF.LogFormat($"AttackVerb of {pawn} on target {target} with job {job} that has target {job?.targetA} and CurrentVerb {manager.CurrentVerb}",
            LogLevel.Info);

        if (manager.CurrentVerb != null && manager.CurrentVerb.Available() &&
            (target == null || manager.CurrentVerb.CanHitTarget(target)) &&
            (job is not { targetA: { IsValid: true, Cell: var cell } } || cell == pawn.Position || !cell.InBounds(pawn.Map) ||
             manager.CurrentVerb.CanHitTarget(job.targetA)))
            return manager.CurrentVerb;

        var verbs = manager.CurrentlyUseableRangedVerbs;
        if (!allowManualCastWeapons && job != null && job.def == JobDefOf.Wait_Combat)
            verbs = verbs.Where(v => !v.Verb.verbProps.onlyManualCast);

        var verbsToUse = verbs.ToList();
        var usedTarget = target ?? job?.targetA ?? LocalTargetInfo.Invalid;

        MVCF.LogFormat($"Getting best verb for target {target} or {job?.targetA} which is {usedTarget} from {verbsToUse.Count} choices", LogLevel.Info);

        if (!usedTarget.IsValid || !usedTarget.Cell.InBounds(pawn.Map)) return null;
        return verbsToUse.Count switch
        {
            0 => null,
            1 => verbsToUse[0].Verb,
            _ => pawn.BestVerbForTarget(usedTarget, verbsToUse)
        };
    }

    public static bool BrawlerUpsetBy(this ManagedVerb mv) =>
        !mv.Verb.IsMeleeAttack && mv.Props is not { brawlerCaresAbout: false }
                               && !(mv.Source == VerbSource.Equipment && mv.Verb.EquipmentSource.PrefersMelee());
}
