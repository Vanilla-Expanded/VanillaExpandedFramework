using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Hediffs
{
    [HotSwappableAttribute]
    public class MoteAttached_TargetingLock : MoteAttached
    {
        public float lockProgress = -1f;
        public Color? colorOverride;
        private Graphic coloredGraphic;

        private Graphic LockGraphic => colorOverride is { } color
            ? coloredGraphic ??= Graphic.GetColoredVersion(Graphic.Shader, color, Graphic.ColorTwo)
            : Graphic;

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
                LockGraphic.Draw(drawPos, Rotation, this, exactRotation);
            }
            this.exactRotation = oldRot;
            this.exactPosition = oldPos;
        }

        protected override void Tick()
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
        public Thing shooter;
        public HediffCompProperties_Targeting targetingProps;

        public float aimProgress = -1f;

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (lockProgress >= 0f)
            {
                DrawTargetingLock(lockProgress);

                if (aimProgress >= 0f && shooter?.Spawned is true)
                {
                    var targetPos = link1.LastDrawPos;
                    var from = shooter is Pawn pawn ? HediffComp_Targeting.WeaponDrawPos(pawn, targetPos) : shooter.DrawPos;
                    targetingProps.DrawLockEffects(from, targetPos, aimProgress, shooter.HostileTo(Faction.OfPlayer));
                }
            }
        }
    }
}