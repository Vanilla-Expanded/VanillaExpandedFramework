namespace VEF.Graphics
{
    using UnityEngine;
    using Verse;

    public class SubEffecter_SlideTowardsTarget : SubEffecter
    {
        public SubEffecterDef_SlideTowardsTarget Def => (SubEffecterDef_SlideTowardsTarget) this.def;

        public int tick = 0;

        private float SlideProgress => (float)this.tick / this.Def.ticksToEnd;


        public SubEffecter_SlideTowardsTarget(SubEffecterDef subDef, Effecter parent) : base(subDef, parent)
        {
        }

        public override void SubEffectTick(TargetInfo A, TargetInfo B)
        {
            base.SubEffectTick(A, B);
            this.tick++;
            MakeMote(A, B, -1);
        }

        public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
        {
            Log.Message("SubTrigger called for SubEffecter_SlideTowardsTarget");
            base.SubTrigger(A, B, overrideSpawnTick, force);

            this.MakeMote(A, B, overrideSpawnTick);
        }

        private void MakeMote(TargetInfo A, TargetInfo B, int overrideSpawnTick)
        {
            Map map = A.Map ?? B.Map;
            if (map == null)
                return;

            if (this.def.fleckDef != null)
            {
                float num = this.def.absoluteAngle ?
                                0f :
                                this.def.useTargetAInitialRotation && A.HasThing ?
                                    A.Thing.Rotation.AsAngle :
                                    !this.def.useTargetBInitialRotation || !B.HasThing ?
                                        (B.Cell - A.Cell).AngleFlat :
                                        B.Thing.Rotation.AsAngle;

                Pawn_EquipmentTracker equipmentTracker          = (A.Thing as Pawn)?.equipment;
                Vector3               endpointRelative          = this.Def.endPointZOverrideByWeapon ? 
                                                                      this.Def.endPoint with { z = equipmentTracker?.Primary?.DrawSize.x ?? this.Def.endPoint.x } : 
                                                                      this.Def.endPoint;


                float   progress = this.Def.ticksToEndOverrideByWeaponWarmup ? 
                                       equipmentTracker?.PrimaryEq?.PrimaryVerb?.WarmupProgress ?? this.SlideProgress : this.SlideProgress;

                Vector3 vector = Vector3.Lerp(A.CenterVector3, A.CenterVector3 + endpointRelative.RotatedBy(num), progress);

                if (vector.ShouldSpawnMotesAt(map, this.def.fleckDef.drawOffscreen))
                {
                    float           velocityAngle = (this.def.fleckUsesAngleForVelocity ? (this.def.angle.RandomInRange + num) : 0f);
                    FleckAttachLink link          = FleckAttachLink.Invalid;

                    if (this.def.fleckDef.useAttachLink && base.EffectiveSpawnLocType == MoteSpawnLocType.OnSource && A.IsValid)
                        link = new FleckAttachLink(A);

                    if (this.def.fleckDef.useAttachLink && base.EffectiveSpawnLocType == MoteSpawnLocType.OnTarget && B.IsValid)
                        link = new FleckAttachLink(B);

                    float num2 = Mathf.Lerp(this.Def.scale.RandomInRange, this.Def.scaleByEnd.RandomInRange, progress) * (this.parent?.scale ?? 1f);


                    map.flecks.CreateFleck(new FleckCreationData
                                           {
                                               def               = this.def.fleckDef,
                                               scale             = this.def.scale.RandomInRange * num2,
                                               spawnPosition     = vector,
                                               rotationRate      = this.def.rotationRate.RandomInRange,
                                               rotation          = this.def.rotation.RandomInRange + num,
                                               instanceColor     = base.EffectiveColor,
                                               velocitySpeed     = this.def.speed.RandomInRange,
                                               velocityAngle     = velocityAngle,
                                               ageTicksOverride  = overrideSpawnTick,
                                               orbitSpeed        = (this.def.orbitOrigin ? this.def.orbitSpeed.RandomInRange : 0f),
                                               orbitSnapStrength = this.def.orbitSnapStrength,
                                               link              = link
                                           });
                }
            }
        }
    }

    public class SubEffecterDef_SlideTowardsTarget : SubEffecterDef
    {
        public int  ticksToEnd                       = 120;
        public bool ticksToEndOverrideByWeaponWarmup = true;

        public Vector3 endPoint                  = Vector3.zero with { z = 5 };
        public bool    endPointZOverrideByWeapon = true;

        public FloatRange scaleByEnd = new(0.8f, 1.2f);


        public SubEffecterDef_SlideTowardsTarget() => 
            this.subEffecterClass = typeof(SubEffecter_SlideTowardsTarget);
    }
}