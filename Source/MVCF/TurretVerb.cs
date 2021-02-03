// TurretVerb.cs by Joshua Bennett
// 
// Created 2021-02-02

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
        private readonly DummyCaster dummyCaster;
        private int cooldownTicksLeft;
        private LocalTargetInfo currentTarget = LocalTargetInfo.Invalid;
        private int warmUpTicksLeft;


        public TurretVerb(Verb verb, VerbSource source, AdditionalVerbProps props, VerbManager man) : base(verb, source,
            props, man)
        {
            dummyCaster = new DummyCaster(man.Pawn, this);
            dummyCaster.Tick();
            dummyCaster.SpawnSetup(man.Pawn.Map, false);
            verb.caster = dummyCaster;
            verb.castCompleteCallback = () => cooldownTicksLeft = Verb.verbProps.AdjustedCooldownTicks(Verb, man.Pawn);
        }

        public LocalTargetInfo Target => currentTarget;

        public virtual void Tick()
        {
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
                currentTarget.HasThing && currentTarget.Thing is Pawn p && (p.Downed || p.Dead))
            {
                man.OverrideVerb = Verb;
                currentTarget = TryFindNewTarget();
                man.OverrideVerb = null;
                TryStartCast();
            }
            else if (warmUpTicksLeft == 0)
            {
                TryCast();
            }
            else if (warmUpTicksLeft > 0)
            {
                warmUpTicksLeft--;
            }
            else
            {
                TryStartCast();
            }
        }

        protected virtual void TryStartCast()
        {
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
            if (success) Verb.WarmupComplete();
        }

        public override LocalTargetInfo PointingTarget(Pawn p)
        {
            return currentTarget;
        }

        public virtual bool CanFire()
        {
            return true;
        }

        public override void DrawOn(Pawn p, Vector3 drawPos)
        {
            base.DrawOn(p, drawPos);
            if (Find.Selector.IsSelected(p) && Target.IsValid)
            {
                GenDraw.DrawAimPie(p, Target, warmUpTicksLeft, 0.2f);
                GenDraw.DrawLineBetween(drawPos, Target.HasThing ? Target.Thing.DrawPos : Target.Cell.ToVector3());
            }
        }

        public void SetTarget(LocalTargetInfo target)
        {
            currentTarget = target;
            TryStartCast();
        }

        protected virtual LocalTargetInfo TryFindNewTarget()
        {
            return AttackTargetFinder.BestShootTargetFromCurrentPosition(
                man.Pawn,
                TargetScanFlags.NeedActiveThreat | TargetScanFlags.NeedLOSToAll |
                TargetScanFlags.NeedAutoTargetable)?.Thing ?? LocalTargetInfo.Invalid;
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

        public Thing RealCaster()
        {
            return pawn;
        }

        public override void Tick()
        {
            if (pawn == null) return;

            Position = pawn.Position;
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