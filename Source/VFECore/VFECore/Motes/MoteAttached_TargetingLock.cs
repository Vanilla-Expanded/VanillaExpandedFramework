using System;
using UnityEngine;
using Verse;

namespace VFECore
{
    [HotSwappableAttribute]
    public class MoteAttached_TargetingLock : MoteAttached
    {
		// gets called by a harmony patch
		public void DrawTargetingLock(float progress)
		{
            var oldPos = this.exactPosition;
            var oldRot = this.exactRotation;
            for (var i = 0; i < 4; i++)
            {
                this.exactRotation = i * 90;
                var offset = (Quaternion.AngleAxis(this.exactRotation, Vector3.up) * (Vector3.forward * Mathf.Max(0.3f, (progress + 0.3f))));
                var drawPos = oldPos + offset;
                this.exactPosition = drawPos;
				this.exactPosition.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                Graphic.Draw(drawPos, Rotation, this);
            }
            this.exactRotation = oldRot;
            this.exactPosition = oldPos;
        }

        public override void Tick()
        {
            base.Tick();
            if (!link1.Linked || link1.Target.ThingDestroyed)
            {
                this.Destroy();
            }
        }
    }

    public class MoteAttached_TargetingLockFixed : MoteAttached_TargetingLock
    {
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            DrawTargetingLock(0.2f);
        }
    }

    public class MoteAttached_TargetingLockDynamic : MoteAttached_TargetingLock
    {
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            // we skip it, DrawTargetingLock gets called by a harmony patch
        }
    }
}