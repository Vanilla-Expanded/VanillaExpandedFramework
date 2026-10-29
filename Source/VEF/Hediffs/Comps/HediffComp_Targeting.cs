using System.Linq;
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

        public GraphicData aimingGraphicData;

        public float hostileColorFactor = 1f;

        private Material targetingLine;
        private Material hostileTargetingLine;
        private MaterialPropertyBlock aimingPropertyBlock;

        public HediffCompProperties_Targeting() => compClass = typeof(HediffComp_Targeting);

        public Color HostileColor(Color color) => new Color(color.r * hostileColorFactor, color.g * hostileColorFactor, color.b * hostileColorFactor, color.a);

        public Material TargetingLine(bool hostile)
        {
            if (hostile)
            {
                hostileTargetingLine ??= MaterialPool.MatFrom(targetingLineTexPath, ShaderDatabase.Transparent, HostileColor(targetingLineColor));
                return hostileTargetingLine;
            }
            targetingLine ??= MaterialPool.MatFrom(targetingLineTexPath, ShaderDatabase.Transparent, targetingLineColor);
            return targetingLine;
        }

        public void DrawLockEffects(Vector3 from, Vector3 to, float aimProgress, bool hostile)
        {
            from.y = to.y = AltitudeLayer.MetaOverlays.AltitudeFor();
            if (!targetingLineTexPath.NullOrEmpty())
            {
                GenDraw.DrawLineBetween(from, to, TargetingLine(hostile), targetingLineWidth);
            }
            if (aimingGraphicData is not null)
            {
                DrawAiming(from, to, aimProgress, hostile);
            }
        }

        private void DrawAiming(Vector3 from, Vector3 to, float aimProgress, bool hostile)
        {
            var length = (to - from).MagnitudeHorizontal();
            if (length < 0.01f)
            {
                return;
            }
            var pos = (from + to) * 0.5f;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            var rotation = Quaternion.AngleAxis(from.AngleToFlat(to) + 90f, Vector3.up);
            var matrix = Matrix4x4.TRS(pos, rotation, new Vector3(aimingGraphicData.drawSize.x, 1f, length));
            aimingPropertyBlock ??= new MaterialPropertyBlock();
            aimingPropertyBlock.SetColor(ShaderPropertyIDs.Color, hostile ? HostileColor(aimingGraphicData.color) : aimingGraphicData.color);
            aimingPropertyBlock.SetFloat(ShaderPropertyIDs.AgeSecs, aimProgress);
            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, aimingGraphicData.Graphic.MatSingle, 0, null, 0, aimingPropertyBlock);
        }
    }

    public class HediffComp_Targeting : HediffComp
    {
        public Mote mote;
        public Thing lockedTarget;
        public HediffCompProperties_Targeting Props => base.props as HediffCompProperties_Targeting;

        public Material TargetingLine => Props.TargetingLine(Hostile);

        public bool LockMaintained => Props.maintainLockAfterShot && lockedTarget is not null;

        public bool Hostile => Pawn.HostileTo(Faction.OfPlayer);

        public static bool SkipsTargetLock(Pawn target) => target.health.hediffSet.hediffs.Any(hediff => hediff.TryGetComp<HediffComp_Targeting>()?.Props.skipTargetLock is true);

        public static Vector3 WeaponDrawPos(Pawn pawn, Vector3 targetPos)
        {
            var weapon = pawn.equipment?.Primary;
            if (weapon == null)
            {
                return pawn.DrawPos;
            }
            var diff = targetPos - pawn.DrawPos;
            var angle = diff.MagnitudeHorizontalSquared() > 0.001f ? diff.AngleFlat() : 0f;
            return pawn.DrawPos + new Vector3(0f, 0f, 0.4f + weapon.def.equippedDistanceOffset).RotatedBy(angle)
                * pawn.ageTracker.CurLifeStage.equipmentDrawDistanceFactor;
        }

        public void DrawTargetingEffects(LocalTargetInfo target)
        {
            if (target.HasThing && mote is MoteAttached_TargetingLockDynamic lockMote && lockMote.Destroyed is false && lockMote.link1.Target.Thing == target.Thing)
            {
                return;
            }
            var targetPos = target.HasThing is false ? target.Cell.ToVector3Shifted() : target.Thing.TrueCenter();
            var aimProgress = Pawn.stances.curStance is Stance_Warmup stance ? AimProgress(stance) : 1f;
            DrawLockEffects(targetPos, aimProgress);
        }

        public void DrawLockEffects(Vector3 targetPos, float aimProgress)
        {
            Props.DrawLockEffects(WeaponDrawPos(Pawn, targetPos), targetPos, aimProgress, Hostile);
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
            return Pawn.Drafted is false || lockedTarget.Destroyed || lockedTarget.Spawned is false
                || lockedTarget.MapHeld != Pawn.MapHeld
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
                    lockMote.aimProgress = AimProgress(stance);
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
                    lockMote.aimProgress = 1f;
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
            if (mote is MoteAttached_TargetingLock targetingLock && Hostile)
            {
                targetingLock.colorOverride = Props.HostileColor(Props.targetingMote.graphicData.color);
            }
            if (mote is MoteAttached_TargetingLockDynamic dynamicLock)
            {
                dynamicLock.shooter = Pawn;
                dynamicLock.targetingProps = Props;
            }
        }

        private float WarmupProgress(Stance_Warmup stance)
        {
            var statValue = Pawn.GetStatValue(StatDefOf.AimingDelayFactor);
            var ticks = (stance.verb.verbProps.warmupTime * statValue).SecondsToTicks();
            return (float)stance.ticksLeft / (float)ticks;
        }

        private float AimProgress(Stance_Warmup stance)
        {
            if (LockMaintained && stance.focusTarg.Thing == lockedTarget)
            {
                return 1f;
            }
            var elapsed = Find.TickManager.TicksGame - stance.startedTick;
            var total = elapsed + stance.ticksLeft;
            return total > 0 ? (float)elapsed / total : 1f;
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
            Scribe_References.Look(ref lockedTarget, "lockedTarget");
        }
    }
}
