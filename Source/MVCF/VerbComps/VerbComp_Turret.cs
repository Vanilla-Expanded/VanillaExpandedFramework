using System;
using System.Linq;
using MVCF.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MVCF.VerbComps;

public class VerbComp_Turret : VerbComp_Draw
{
    private int cooldownTicksLeft;
    private LocalTargetInfo currentTarget = LocalTargetInfo.Invalid;
    private int warmUpTicksLeft;

    public override bool NeedsTicking => true;
    public override bool NeedsDrawing => !Props.invisible;

    public LocalTargetInfo Target => currentTarget;
    public new VerbCompProperties_Turret Props => props as VerbCompProperties_Turret;

    public override bool Independent => true;

    public override void CompTick()
    {
        base.CompTick();
        if (parent is not { Manager.Pawn.Spawned: true }) return;
        if (parent.Verb.Bursting) return;

        if (currentTarget.IsValid && (currentTarget is { HasThing: true, ThingDestroyed: true }
                                   || (currentTarget is { HasThing: true, Thing: Pawn p } && (p.Downed || p.Dead))
                                   || !parent.Verb.CanHitTarget(currentTarget))) currentTarget = LocalTargetInfo.Invalid;

        if (cooldownTicksLeft > 0) cooldownTicksLeft--;
        if (cooldownTicksLeft > 0) return;

        if (!parent.Enabled || !CanFire())
        {
            if (currentTarget.IsValid) currentTarget = LocalTargetInfo.Invalid;
            if (warmUpTicksLeft > 0) warmUpTicksLeft = 0;
            return;
        }

        if (!currentTarget.IsValid) currentTarget = TryFindNewTarget();

        if (warmUpTicksLeft == 0)
            TryCast();
        else if (warmUpTicksLeft > 0)
            warmUpTicksLeft--;
        else if (currentTarget.IsValid)
            TryStartCast();
    }

    protected virtual void TryStartCast()
    {
        if (currentTarget == null || !currentTarget.IsValid) return;
        if (parent.Verb.verbProps.warmupTime > 0)
            warmUpTicksLeft = (parent.Verb.verbProps.warmupTime * (parent.Manager?.Pawn?.GetStatValue(StatDefOf.AimingDelayFactor) ?? 1f))
               .SecondsToTicks();
        else
            TryCast();
    }

    protected virtual void TryCast()
    {
        warmUpTicksLeft = -1;
        parent.Verb.castCompleteCallback = () =>
            cooldownTicksLeft = parent.Verb.verbProps.AdjustedCooldownTicks(parent.Verb, parent.Manager.Pawn);
        var success = parent.Verb.TryStartCastOn(currentTarget);
        if (success && parent.Verb.verbProps.warmupTime > 0) parent.Verb.WarmupComplete();
    }

    public override LocalTargetInfo PointingTarget(Pawn p) => currentTarget;

    public virtual bool CanFire() =>
        parent is { Manager: { Pawn: var pawn } } && !pawn.Dead && !pawn.Downed &&
        !(!parent.Verb.verbProps.violent || pawn.WorkTagIsDisabled(WorkTags.Violent)) && parent.Verb.Available();

    public override void DrawOnAt(Pawn p, Vector3 drawPos)
    {
        if (Props.invisible) return;
        base.DrawOnAt(p, drawPos);
        if (Find.Selector.IsSelected(p) && Target.IsValid)
        {
            if (warmUpTicksLeft > 0)
                GenDraw.DrawAimPie(p, Target, warmUpTicksLeft, 0.2f);
            if (cooldownTicksLeft > 0)
                GenDraw.DrawCooldownCircle(p.DrawPos, cooldownTicksLeft * 0.002f);
            GenDraw.DrawLineBetween(drawPos, Target.HasThing ? Target.Thing.DrawPos : Target.Cell.ToVector3());
        }
    }

    public override bool SetTarget(LocalTargetInfo target)
    {
        currentTarget = target;
        if (cooldownTicksLeft <= 0 && warmUpTicksLeft <= 0) TryStartCast();
        return false;
    }

    protected virtual LocalTargetInfo TryFindNewTarget()
    {
        if (parent is not { Manager: var man }) return LocalTargetInfo.Invalid;
        return TargetFinder.BestAttackTarget(
                       man.Pawn, parent.Verb,
                       TargetScanFlags.NeedActiveThreat | TargetScanFlags.NeedLOSToAll |
                       TargetScanFlags.NeedAutoTargetable,
                       Props.uniqueTargets
                           ? new Predicate<Thing>(thing =>
                               man.Pawn.mindState.enemyTarget != thing &&
                               man.ManagedVerbs.All(verb =>
                                   verb.Verb.CurrentTarget.Thing != thing &&
                                   verb.TryGetComp<VerbComp_Turret>()?.currentTarget.Thing != thing))
                           : null, parent.Verb.verbProps.minRange, parent.Verb.verbProps.range, canTakeTargetsCloserThanEffectiveMinRange: false)
                 ?.Thing ??
               LocalTargetInfo.Invalid;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_TargetInfo.Look(ref currentTarget, nameof(currentTarget));
        Scribe_Values.Look(ref warmUpTicksLeft, nameof(warmUpTicksLeft));
        Scribe_Values.Look(ref cooldownTicksLeft, nameof(cooldownTicksLeft));
    }
}

public class VerbCompProperties_Turret : VerbCompProperties_Draw
{
    public bool invisible;
    public bool uniqueTargets;
}
