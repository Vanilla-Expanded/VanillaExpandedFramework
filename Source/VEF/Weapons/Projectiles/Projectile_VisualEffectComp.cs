namespace VEF.Weapons
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    internal class Projectile_VisualEffectComp : ThingComp
    {
        public Projectile_VisualEffectCompProperties Props => (Projectile_VisualEffectCompProperties)this.props;

        public Projectile Projectile => (Projectile)this.parent;

        public override void PostDraw()
        {
            base.PostDraw();

            if (!Find.TickManager.Paused)
            {
                Projectile projectile = this.Projectile;

                Vector3 velocityDirection = projectile.ExactRotation * Vector3.forward;
                Vector3 effectPos         = projectile.ExactPosition + velocityDirection;

                if (this.Props.lightningGlow)
                    this.parent.Map.flecks.CreateFleck(new FleckCreationData
                                                       {
                                                           def               = FleckDefOf.LightningGlow,
                                                           spawnPosition     = effectPos,
                                                           scale             = Rand.Range(0.1f, 0.2f) * 3,
                                                           ageTicksOverride  = -1,
                                                           rotationRate      = 0,
                                                           velocityAngle     = this.Projectile.ExactPosition.AngleToFlat(effectPos) - 90,
                                                           velocitySpeed     = 0.01f * this.Projectile.def.projectile.speed,
                                                           solidTimeOverride = 0f
                                                       });

                if (this.Props.gaussDistortion)
                {
                    FleckCreationData data = FleckMaker.GetDataStatic(effectPos, projectile.Map, VEFDefOf.VEF_GaussDistortion, Rand.Range(0.1f, 0.25f) * 2);
                    data.rotationRate  = 90f;
                    data.velocityAngle = this.Projectile.ExactPosition.AngleToFlat(effectPos) - 90 + Rand.Range(-15, 15);
                    data.velocitySpeed = this.Projectile.def.projectile.speed;
                    projectile.Map.flecks.CreateFleck(data);
                }
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            if (!Find.TickManager.Paused)
            {
                Projectile projectile = this.Projectile;

                Vector3 velocityDirection = projectile.ExactRotation * Vector3.forward;
                Vector3 effectPos         = drawLoc + velocityDirection;

                if (this.Props.lightningGlow)
                    this.parent.Map.flecks.CreateFleck(new FleckCreationData
                                                       {
                                                           def               = FleckDefOf.LightningGlow,
                                                           spawnPosition     = effectPos,
                                                           scale             = Rand.Range(0.1f, 0.2f) * 3,
                                                           ageTicksOverride  = -1,
                                                           rotationRate      = 0,
                                                           velocityAngle     = this.Projectile.ExactPosition.AngleToFlat(effectPos) - 90,
                                                           velocitySpeed     = 0.01f * this.Projectile.def.projectile.speed,
                                                           solidTimeOverride = 0f
                                                       });

                if (this.Props.gaussDistortion)
                {
                    FleckCreationData data = FleckMaker.GetDataStatic(effectPos, projectile.Map, VEFDefOf.VEF_GaussDistortion, Rand.Range(0.1f, 0.25f) * 2);
                    data.rotationRate  = 90f;
                    data.velocityAngle = this.Projectile.ExactPosition.AngleToFlat(effectPos) - 90 + Rand.Range(-15, 15);
                    data.velocitySpeed = this.Projectile.def.projectile.speed;
                    projectile.Map.flecks.CreateFleck(data);
                }
            }
        }
    }

    internal class Projectile_VisualEffectCompProperties : CompProperties
    {
        public bool gaussDistortion;
        public bool lightningGlow;

        public Projectile_VisualEffectCompProperties() => 
            this.compClass = typeof(Projectile_VisualEffectComp);
    }
}
