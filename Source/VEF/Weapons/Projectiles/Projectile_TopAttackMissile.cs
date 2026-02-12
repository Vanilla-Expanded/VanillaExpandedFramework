using UnityEngine;
using RimWorld;
using Verse;

namespace VEF.Weapons
{
    [HotSwappable]
    public class Projectile_TopAttackMissile : Projectile_Explosive
    {
        private const float JAVELIN_HEIGHT = 12f;
        private const float JAVELIN_POWER = 3f;
        private const float ROTATION_INTENSITY = 10f;

        private Vector3 LookTowards
        {
            get
            {
                float dx = this.destination.x - this.origin.x;
                float dz = this.destination.z - this.origin.z;
                float t = this.DistanceCoveredFraction;
                float direction = (t < 0.5f) ? 1f : -1f;
                float slopeCurve = Mathf.Pow(Mathf.Abs(2f * t - 1f), 2f);
                float zOffset = JAVELIN_HEIGHT * ROTATION_INTENSITY * direction * slopeCurve;
                return new Vector3(dx, this.def.Altitude, dz + zOffset);
            }
        }

        public override Quaternion ExactRotation => Quaternion.LookRotation(this.LookTowards);

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            ThrowDustPuffThick(this.DrawPos, Map, 3f);
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            float t = this.DistanceCoveredFraction;
            float shape = 1f - Mathf.Pow(Mathf.Abs(2f * t - 1f), JAVELIN_POWER);
            float currentHeight = JAVELIN_HEIGHT * shape;
            Vector3 visualPos = drawLoc + new Vector3(0f, 0f, currentHeight);
            UnityEngine.Graphics.DrawMesh(MeshPool.GridPlane(this.def.graphicData.drawSize), visualPos, this.ExactRotation, this.DrawMat, 0);
            DrawShadow(drawLoc, currentHeight);
            Comps_PostDraw();
        }

        protected override void Tick()
        {
            base.Tick();
            if (this.Map != null)
            {
                float t = this.DistanceCoveredFraction;
                float shape = 1f - Mathf.Pow(Mathf.Abs(2f * t - 1f), JAVELIN_POWER);
                float currentHeight = JAVELIN_HEIGHT * shape;

                Vector3 groundPos = base.ExactPosition;
                Vector3 airPos = groundPos + new Vector3(0f, 0f, currentHeight);

                if (t > 0.5f)
                {
                    float angle = Vector3.Angle(origin, airPos);

                    if (Rand.Chance(0.5f))
                    {
                        ThrowSmokeTrail(airPos, base.Map, angle, 1.2f);
                    }
                    ThrowRocketExhaust(airPos, base.Map, angle, 0.8f);
                }
            }
        }

        private void DrawShadow(Vector3 drawLoc, float height)
        {
            if (def.projectile.shadowSize > 0f)
            {
                float scale = Mathf.Lerp(1f, 0.5f, height / JAVELIN_HEIGHT);
                Vector3 s = new Vector3(def.projectile.shadowSize * scale, 1f, def.projectile.shadowSize * scale);

                Matrix4x4 matrix = default(Matrix4x4);
                Vector3 shadowPos = drawLoc;
                shadowPos.y = AltitudeLayer.Shadows.AltitudeFor();

                matrix.SetTRS(shadowPos, Quaternion.identity, s);
                UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, MaterialPool.MatFrom("Things/Skyfaller/SkyfallerShadowCircle", ShaderDatabase.Transparent), 0);
            }
        }

        public void ThrowSmokeTrail(Vector3 loc, Map map, float angle, float size)
        {
            FleckCreationData data = FleckMaker.GetDataStatic(loc, map, FleckDefOf.Smoke, size);
            data.rotationRate = Rand.Range(-30f, 30f);
            data.velocityAngle = Rand.Range(0, 360);
            data.velocitySpeed = Rand.Range(0.008f, 0.012f);
            map.flecks.CreateFleck(data);
        }

        public void ThrowRocketExhaust(Vector3 loc, Map map, float angle, float size)
        {
            FleckCreationData data = FleckMaker.GetDataStatic(loc, map, FleckDefOf.ShotFlash, size);
            data.velocityAngle = angle;
            data.solidTimeOverride = 0.15f;
            data.velocitySpeed = 0.01f;
            map.flecks.CreateFleck(data);
        }

        public void ThrowDustPuffThick(Vector3 loc, Map map, float scale)
        {
            FleckCreationData data = FleckMaker.GetDataStatic(loc, map, FleckDefOf.DustPuffThick, scale);
            data.rotationRate = Rand.Range(-60, 60);
            data.velocityAngle = Rand.Range(0, 360);
            data.velocitySpeed = Rand.Range(0.6f, 0.75f);
            map.flecks.CreateFleck(data);
        }
    }
}