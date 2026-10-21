using UnityEngine;
using Verse;

namespace VEF.Weapons
{
    public class CompProperties_HomingProjectile : CompProperties
    {
        public float homingDistanceFractionPassed;
        public float homingCorrectionTickRate;
        public float initialDispersionFromTarget;
        public float launchAngleDeviationMax;
        public float maxTurnDegreesPerTick;
        public float maxTravelDistance;
        public int maxTravelTicks;
        public int targetLockTicks;
        public SoundDef hitSound;
        public CompProperties_HomingProjectile()
        {
            this.compClass = typeof(CompHomingProjectile);
        }
    }
    public class CompHomingProjectile : ThingComp
    {
        public Vector3 originLaunchCell;

        public bool isOffset;
        public Vector3 smartHeading;
        public int smartTicksAlive;
        public float smartLaunchDist;
        public bool SmartMode => Props.maxTurnDegreesPerTick > 0f;
        public bool HomingEngaged(Vector3 pos) => Vector3.Distance(originLaunchCell.Yto0(), pos) >= smartLaunchDist * Props.homingDistanceFractionPassed;
        public Projectile Projectile => this.parent as Projectile;
        public CompProperties_HomingProjectile Props => base.props as CompProperties_HomingProjectile;
        public Vector3 DispersionOffset => new Vector3(Rand.Range(0f - this.Props.initialDispersionFromTarget,
            this.Props.initialDispersionFromTarget), 0f, Rand.Range(0f - this.Props.initialDispersionFromTarget,
                this.Props.initialDispersionFromTarget));
        public bool CanChangeTrajectory()
        {
            var projectile = Projectile;
            var origCell = originLaunchCell.Yto0();
            var targetCell = projectile.intendedTarget.CenterVector3.Yto0();
            var curPosCell = projectile.ExactPosition.Yto0();
            if (projectile.intendedTarget.Thing is Pawn pawn && pawn.Dead)
            {
                return false;
            }
            var distanceBetweenOrigAndCurPos = Vector3.Distance(origCell, curPosCell);
            var distanceBetweenOrigAndTargetPos = Vector3.Distance(origCell, targetCell);
            var result = (distanceBetweenOrigAndCurPos / distanceBetweenOrigAndTargetPos) >= Props.homingDistanceFractionPassed
                && Find.TickManager.TicksGame % Props.homingCorrectionTickRate == 0;
            //Log.Message(result + " - Find.TickManager.TicksGame: " + Find.TickManager.TicksGame + " - " + this + " - origCell: " + origCell + " - curPosCell: " + curPosCell + " - targetCell: " + targetCell
            //   + " - distanceBetweenOrigAndCurPos: " + distanceBetweenOrigAndCurPos
            //   + " - distanceBetweenOrigAndTargetPos: " + distanceBetweenOrigAndTargetPos + " - (distanceBetweenOrigAndCurPos / distanceBetweenOrigAndTargetPos): " 
            //   + (distanceBetweenOrigAndCurPos / distanceBetweenOrigAndTargetPos)
            //   + " - Find.TickManager.TicksGame % Props.homingCorrectionTickRate: " + (Find.TickManager.TicksGame % Props.homingCorrectionTickRate));
            //if (result)
            //{
            //    Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
            //}
            return result;
        }

        public void InitSmartLaunch()
        {
            var projectile = Projectile;
            var originRef = NonPublicFields.Projectile_origin(projectile);
            originLaunchCell = originRef;
            var dir = (NonPublicFields.Projectile_destination(projectile) - originRef).Yto0();
            smartHeading = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.forward;
            smartHeading = Quaternion.AngleAxis(Rand.Range(0f - Props.launchAngleDeviationMax, Props.launchAngleDeviationMax), Vector3.up) * smartHeading;
            smartTicksAlive = 0;
            smartLaunchDist = Vector3.Distance(originRef.Yto0(), projectile.intendedTarget.CenterVector3.Yto0());
        }

        public void SmartTick()
        {
            var projectile = Projectile;
            var target = projectile.intendedTarget;
            if (target.Thing is Thing thing && (thing.Destroyed || !thing.Spawned))
            {
                projectile.Destroy();
                return;
            }
            smartTicksAlive++;
            var pos = projectile.ExactPosition.Yto0();
            if (smartTicksAlive >= Props.maxTravelTicks || Vector3.Distance(originLaunchCell.Yto0(), pos) >= Props.maxTravelDistance)
            {
                projectile.Destroy();
                return;
            }
            if (target.Thing is Thing targetThing)
            {
                var radius = targetThing is Pawn targetPawn ? Mathf.Clamp(targetPawn.BodySize * 0.5f, 0.55f, 1.4f) : 0.55f;
                if (Vector3.Distance(pos, targetThing.DrawPos.Yto0()) <= radius)
                {
                    NonPublicFields.Projectile_destination(projectile) = target.CenterVector3;
                    NonPublicFields.Projectile_ticksToImpact(projectile) = 1;
                    return;
                }
            }
            var toTarget = target.CenterVector3.Yto0() - pos;
            var angleToTarget = 0f;
            var turn = 0f;
            if (HomingEngaged(pos) && toTarget.sqrMagnitude > 0.0001f)
            {
                angleToTarget = Vector3.SignedAngle(smartHeading, toTarget.normalized, Vector3.up);
                turn = Mathf.Clamp(angleToTarget, 0f - Props.maxTurnDegreesPerTick, Props.maxTurnDegreesPerTick);
                smartHeading = Quaternion.AngleAxis(turn, Vector3.up) * smartHeading;
            }
            var remaining = NonPublicFields.Projectile_ticksToImpact(projectile);
            if (remaining <= 0)
            {
                return;
            }
            NonPublicFields.Projectile_origin(projectile) = pos + Vector3.up * projectile.def.Altitude;
            if (remaining == 1)
            {
                NonPublicFields.Projectile_destination(projectile) = target.CenterVector3;
                return;
            }
            var segment = smartHeading * (projectile.def.projectile.SpeedTilesPerTick * remaining);
            NonPublicFields.Projectile_destination(projectile) = pos + Vector3.up * projectile.def.Altitude + segment;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref originLaunchCell, "originLaunchCell");
            Scribe_Values.Look(ref smartLaunchDist, "smartLaunchDist");
        }
     }
}

