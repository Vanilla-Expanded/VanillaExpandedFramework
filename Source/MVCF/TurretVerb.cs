// TurretVerb.cs by Joshua Bennett
// 
// Created 2021-02-02

using System;
using System.Linq;
using MVCF.Comps;
using MVCF.Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MVCF
{
    public class TurretVerb : ManagedVerb
    {
        private int cooldownTicksLeft;
        private LocalTargetInfo currentTarget = LocalTargetInfo.Invalid;
        private DummyCaster dummyCaster;
        private int warmUpTicksLeft;

        public TurretVerb(Verb verb, VerbSource source, AdditionalVerbProps props, VerbManager man) : base(verb, source,
            props, man)
        {
            verb.castCompleteCallback = () => cooldownTicksLeft = Verb.verbProps.AdjustedCooldownTicks(Verb, man.Pawn);
        }

        public override bool NeedsTicking => true;

        public LocalTargetInfo Target => currentTarget;

        public void CreateCaster()
        {
            dummyCaster = new DummyCaster(man.Pawn, this);
            dummyCaster.Tick();
            dummyCaster.SpawnSetup(man.Pawn.Map, false);
            Verb.caster = dummyCaster;
        }

        public void DestroyCaster()
        {
            if (dummyCaster != null)
            {
                if (!dummyCaster.Destroyed) dummyCaster.Destroy();
                dummyCaster = null;
            }

            Verb.caster = man.Pawn;
        }

        public override void Tick()
        {
            if (!man.Pawn.Spawned) return;
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
                man.OverrideVerb = Verb;
                currentTarget = TryFindNewTarget();
                man.OverrideVerb = null;
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
                warmUpTicksLeft = (Verb.verbProps.warmupTime * man.Pawn.GetStatValue(StatDefOf.AimingDelayFactor))
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

        public virtual bool CanFire() =>
            !man.Pawn.Dead && !man.Pawn.Downed && !(!Verb.verbProps.violent ||
                                                    man.Pawn.WorkTagIsDisabled(WorkTags.Violent));

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
            return AttackTargetFinder.BestShootTargetFromCurrentPosition(
                man.Pawn,
                TargetScanFlags.NeedActiveThreat | TargetScanFlags.NeedLOSToAll |
                TargetScanFlags.NeedAutoTargetable,
                Props.uniqueTargets
                    ? new Predicate<Thing>(thing =>
                        man.Pawn.mindState.enemyTarget != thing &&
                        man.ManagedVerbs.All(verb =>
                            verb.Verb.CurrentTarget.Thing != thing &&
                            (verb as TurretVerb)?.currentTarget.Thing != thing))
                    : null)?.Thing ?? LocalTargetInfo.Invalid;
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