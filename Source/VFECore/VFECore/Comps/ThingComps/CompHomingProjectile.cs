using UnityEngine;
using Verse;

namespace VFECore
{
    public class CompProperties_HomingProjectile : CompProperties
    {
        public float homingDistanceFractionPassed;
        public float homingCorrectionTickRate;
        public float initialDispersionFromTarget;
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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref originLaunchCell, "originLaunchCell");
        }
    }
}

