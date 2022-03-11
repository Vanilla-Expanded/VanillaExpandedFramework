using System;
using System.Collections.Generic;
using System.Linq;
using MVCF.Comps;
using MVCF.Utilities;
using MVCF.VerbComps;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MVCF
{
    public class TurretVerb : DrawnVerb
    {
        private int cooldownTicksLeft;
        private LocalTargetInfo currentTarget = LocalTargetInfo.Invalid;
        private DummyCaster dummyCaster;
        private int warmUpTicksLeft;

        public override bool NeedsTicking => true;

        public LocalTargetInfo Target => currentTarget;

        public override void Initialize(Verb verb, AdditionalVerbProps props, IEnumerable<VerbCompProperties> additionalComps)
        {
            base.Initialize(verb, props, additionalComps);
            if (Manager is not null) verb.castCompleteCallback = () => cooldownTicksLeft = Verb.verbProps.AdjustedCooldownTicks(Verb, Manager.Pawn);
        }

        public void CreateCaster()
        {
            if (Manager is not {Pawn: var pawn}) return;
            dummyCaster = new DummyCaster(pawn, this);
            dummyCaster.Tick();
            dummyCaster.SpawnSetup(pawn.Map, false);
            Verb.caster = dummyCaster;
        }

        public void DestroyCaster()
        {
            if (dummyCaster != null)
            {
                if (!dummyCaster.Destroyed) dummyCaster.Destroy();
                dummyCaster = null;
            }

            if (Manager is {Pawn: var pawn})
                Verb.caster = pawn;
        }

        public override void Tick()
        {
            if (Manager is not {Pawn: {Spawned: true}}) return;
            if (Verb.Bursting) return;

            if (cooldownTicksLeft > 0) cooldownTicksLeft--;
            if (cooldownTicksLeft > 0) return;

            if (!Enabled || !CanFire())
            {
                if (currentTarget.IsValid) currentTarget = LocalTargetInfo.Invalid;
                if (warmUpTicksLeft > 0) warmUpTicksLeft = 0;
                return;
            }

            if (!currentTarget.IsValid || currentTarget.HasThing && currentTarget.ThingDestroyed ||
                currentTarget.HasThing && currentTarget.Thing is Pawn p && (p.Downed || p.Dead) ||
                !Verb.CanHitTarget(currentTarget))
            {
                currentTarget = TryFindNewTarget();
                TryStartCast();
            }
            else if (warmUpTicksLeft == 0)
                TryCast();
            else if (warmUpTicksLeft > 0)
                warmUpTicksLeft--;
            else
                TryStartCast();
        }

        protected virtual void TryStartCast()
        {
            if (currentTarget == null || !currentTarget.IsValid) return;
            if (Verb.verbProps.warmupTime > 0)
                warmUpTicksLeft = (Verb.verbProps.warmupTime * (Manager?.Pawn?.GetStatValue(StatDefOf.AimingDelayFactor) ?? 1f))
                    .SecondsToTicks();
            else
                TryCast();
        }

        protected virtual void TryCast()
        {
            warmUpTicksLeft = -1;
            var success = Verb.TryStartCastOn(currentTarget);
            if (success && Verb.verbProps.warmupTime > 0) Verb.WarmupComplete();
        }

        public override LocalTargetInfo PointingTarget(Pawn p) => currentTarget;

        public virtual bool CanFire() => Manager is {Pawn: var pawn} && !pawn.Dead && !pawn.Downed && !(!Verb.verbProps.violent || pawn.WorkTagIsDisabled(WorkTags.Violent));

        public override void DrawOn(Pawn p, Vector3 drawPos)
        {
            base.DrawOn(p, drawPos);
            if (Find.Selector.IsSelected(p) && Target.IsValid)
            {
                if (warmUpTicksLeft > 0)
                    GenDraw.DrawAimPie(p, Target, warmUpTicksLeft, 0.2f);
                if (cooldownTicksLeft > 0)
                    GenDraw.DrawCooldownCircle(p.DrawPos, cooldownTicksLeft * 0.002f);
                GenDraw.DrawLineBetween(drawPos, Target.HasThing ? Target.Thing.DrawPos : Target.Cell.ToVector3());
            }
        }

        public void SetTarget(LocalTargetInfo target)
        {
            currentTarget = target;
            if (cooldownTicksLeft <= 0) TryStartCast();
        }

        protected virtual LocalTargetInfo TryFindNewTarget()
        {
            if (Manager is not { } man) return LocalTargetInfo.Invalid;
            return TargetFinder.BestAttackTarget(
                man.Pawn, Verb,
                TargetScanFlags.NeedActiveThreat | TargetScanFlags.NeedLOSToAll |
                TargetScanFlags.NeedAutoTargetable,
                Props.uniqueTargets
                    ? new Predicate<Thing>(thing =>
                        man.Pawn.mindState.enemyTarget != thing &&
                        man.ManagedVerbs.All(verb =>
                            verb.Verb.CurrentTarget.Thing != thing &&
                            (verb as TurretVerb)?.currentTarget.Thing != thing))
                    : null, Verb.verbProps.minRange, Verb.verbProps.range, canTakeTargetsCloserThanEffectiveMinRange: false)?.Thing ?? LocalTargetInfo.Invalid;
        }
    }


    public class DummyCaster : Thing, IFakeCaster
    {
        private readonly Pawn pawn;
        private readonly TurretVerb verb;

        public DummyCaster(Pawn pawn, TurretVerb verb)
        {
            this.pawn = pawn;
            this.verb = verb;
            def = ThingDef.Named("MVCF_Dummy");
        }

        public DummyCaster()
        {
        }

        public override Vector3 DrawPos => verb.DrawPos(verb.Target, pawn, pawn.DrawPos);

        public Thing RealCaster() => pawn;

        public override void Tick()
        {
            if (pawn == null) return;

            Position = pawn.Position;
            if (pawn.Spawned && Spawned && Map.Index != pawn.Map.Index)
            {
                DeSpawn();
                SpawnSetup(pawn.Map, false);
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad) Destroy();
        }
    }
}