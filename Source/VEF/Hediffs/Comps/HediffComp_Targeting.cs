using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Hediffs
{
    public class HediffCompProperties_Targeting : HediffCompProperties
    {
        public bool neverMiss;
        public bool neverHit;
        public bool alwaysHit;
        public bool alwaysMiss;
        public ThingDef targetingMote;
        public float initialTargetingMoteScale;
        public bool sizeScalesWithProgress = true;
        public string targetingLineTexPath;
        public Color targetingLineColor = Color.red;
        public float targetingLineWidth = 0.2f;
        public bool maintainLockAfterShot;
        public bool skipTargetLock;
        public HediffCompProperties_Targeting() => compClass = typeof(HediffComp_Targeting);
    }

    public class HediffComp_Targeting : HediffComp
    {
        public Mote mote;
        public Thing lockedTarget;
        public HediffCompProperties_Targeting Props => base.props as HediffCompProperties_Targeting;

        private Material targetingLine;
        public Material TargetingLine
        {
            get
            {
                targetingLine ??= MaterialPool.MatFrom(Props.targetingLineTexPath, ShaderDatabase.Transparent, Props.targetingLineColor);
                return targetingLine;
            }
        }

        public bool LockMaintained => Props.maintainLockAfterShot && lockedTarget is not null;

        public void DrawTargetingEffects(LocalTargetInfo target)
        {
            if (!Props.targetingLineTexPath.NullOrEmpty())
            {
                Vector3 b = target.HasThing is false ? target.Cell.ToVector3Shifted() : target.Thing.TrueCenter();
                var a = Pawn.TrueCenter();
                b.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                a.y = b.y;
                GenDraw.DrawLineBetween(a, b, TargetingLine, Props.targetingLineWidth);
            }
        }

        public void NotifyShotFired(LocalTargetInfo target)
        {
            if (Props.maintainLockAfterShot is false || target.HasThing is false)
            {
                return;
            }
            lockedTarget = target.Thing;
        }

        public void DropLock() => lockedTarget = null;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (LockMaintained && LockInvalid())
            {
                DropLock();
            }
            TargetingMoteTick();
        }

        private bool LockInvalid()
        {
            return lockedTarget.Destroyed || lockedTarget.Spawned is false || lockedTarget.MapHeld != Pawn.MapHeld
                || !Pawn.PositionHeld.InHorDistOf(lockedTarget.PositionHeld, LockRange());
        }

        private void TargetingMoteTick()
        {
            if (Props.targetingMote is null)
            {
                return;
            }
            var stance = Pawn.stances.curStance as Stance_Warmup;
            if (stance?.verb is not null)
            {
                var target = stance.focusTarg;
                EnsureMote(target);
                var lockHeld = LockMaintained && target.HasThing && target.Thing == lockedTarget;
                var progress = WarmupProgress(stance);
                if (mote is MoteAttached_TargetingLockDynamic lockMote)
                {
                    lockMote.lockProgress = lockHeld ? 0f : progress;
                }
                else if (lockHeld)
                {
                    mote.Scale = Mathf.Max(Props.initialTargetingMoteScale, 1f);
                }
                else if (Props.sizeScalesWithProgress)
                {
                    mote.Scale = progress;
                }
                mote.Maintain();
            }
            else if (LockMaintained)
            {
                EnsureMote(new LocalTargetInfo(lockedTarget));
                mote.Scale = Mathf.Max(Props.initialTargetingMoteScale, 1f);
                if (mote is MoteAttached_TargetingLockDynamic lockMote)
                {
                    lockMote.lockProgress = 0f;
                }
                mote.Maintain();
            }
        }

        private void EnsureMote(LocalTargetInfo target)
        {
            if (mote?.Destroyed is false)
            {
                if (mote.link1.Target.Thing == target.Thing && (target.HasThing || mote.Position == target.Cell))
                {
                    return;
                }
                mote.Destroy();
            }
            mote = target.HasThing
                ? MoteMaker.MakeAttachedOverlay(target.Thing, Props.targetingMote, Vector3.zero, Props.initialTargetingMoteScale)
                : MakeStaticMote(target.CenterVector3, Pawn.Map, Props.targetingMote, Props.initialTargetingMoteScale);
            if (mote is MoteAttached_TargetingLockDynamic dynamicLock)
            {
                dynamicLock.lockComp = this;
            }
        }

        private float WarmupProgress(Stance_Warmup stance)
        {
            var statValue = Pawn.GetStatValue(StatDefOf.AimingDelayFactor);
            var ticks = (stance.verb.verbProps.warmupTime * statValue).SecondsToTicks();
            return (float)stance.ticksLeft / (float)ticks;
        }

        private float LockRange()
        {
            var verb = Pawn.equipment?.PrimaryEq?.PrimaryVerb;
            return verb?.verbProps.range ?? 0f;
        }

        public static Mote MakeStaticMote(Vector3 loc, Map map, ThingDef moteDef, float scale = 1f)
        {
            var obj = (Mote)ThingMaker.MakeThing(moteDef);
            obj.exactPosition = loc;
            obj.Scale = scale;
            GenSpawn.Spawn(obj, loc.ToIntVec3(), map);
            return obj;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref mote, "mote");
            Scribe_References.Look(ref lockedTarget, "lockedTarget");
        }
    }
}