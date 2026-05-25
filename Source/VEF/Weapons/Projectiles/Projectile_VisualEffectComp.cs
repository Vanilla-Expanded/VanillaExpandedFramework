namespace VEF.Weapons
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    internal class Projectile_VisualEffectComp : ThingComp
    {
        public Projectile_VisualEffectCompProperties Props => (Projectile_VisualEffectCompProperties)this.props;

        public Projectile Projectile => (Projectile)this.parent;

        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);

            // The position is only recalculated on tick interval, so spawning motes on normal tick could end up spawning motes on the same spot over and over.
            Projectile projectile = this.Projectile;

            Vector3 velocityDirection = projectile.ExactRotation * Vector3.forward;
            Vector3 effectPos         = projectile.ExactPosition + velocityDirection;

            // Only draw if on the same map, don't draw out-of-bounds
            if (effectPos.ShouldSpawnMotesAt(parent.Map))
            {
                float angle               = this.Projectile.ExactPosition.AngleToFlat(effectPos) - 90;
                float speed               = 0.01f * this.Projectile.def.projectile.speed;

                if (this.Props.lightningGlow)
                    VefFleckMaker.MakeLightningGlow(parent.Map, effectPos, angle, speed, Rand.Range(0.3f, 0.6f));
                if (this.Props.gaussDistortion)
                    VefFleckMaker.MakeGaussDistortion(parent.Map, effectPos, angle, speed + Rand.Range(-15f, 15f), Rand.Range(0.2f, 0.5f));
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
