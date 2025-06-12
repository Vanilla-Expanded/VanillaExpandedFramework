using UnityEngine;

namespace VEF.Planet
{
    public class MovingBase_Tweener
    {
        private MovingBase movingBase;

        private Vector3 tweenedPos = Vector3.zero;

        private Vector3 lastTickSpringPos;

        private const float SpringTightness = 0.09f;

        public Vector3 TweenedPos => tweenedPos;

        public Vector3 LastTickTweenedVelocity => TweenedPos - lastTickSpringPos;

        public Vector3 TweenedPosRoot => MovingBaseTweenerUtility.PatherTweenedPosRoot(movingBase)
            + MovingBaseTweenerUtility.MovingBaseCollisionPosOffsetFor(movingBase);

        public MovingBase_Tweener(MovingBase movingBase)
        {
            this.movingBase = movingBase;
        }

        public void TweenerTick()
        {
            lastTickSpringPos = tweenedPos;
            Vector3 vector = TweenedPosRoot - tweenedPos;
            tweenedPos += vector * 0.09f;
        }

        public void ResetTweenedPosToRoot()
        {
            tweenedPos = TweenedPosRoot;
            lastTickSpringPos = tweenedPos;
        }
    }
}