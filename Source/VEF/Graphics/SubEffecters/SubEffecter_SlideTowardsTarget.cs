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

            if (Rand.Value < this.def.chancePerTick)
                this.MakeMote(A, B, -1);
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
                Pawn_EquipmentTracker equipmentTracker = (A.Thing as Pawn)?.equipment;
                Vector3 endpointRelative = this.Def.endPointZOverrideByWeapon ?
                                               this.Def.endPoint with { z = (equipmentTracker?.Primary?.DrawSize.x ?? this.Def.endPoint.z) * this.Def.endPointFactor.RandomInRange } :
                                               this.Def.endPoint;

                float num = (B.CenterCell - A.CenterCell).AngleFlat;
                


                float   progress = this.Def.ticksToEndOverrideByWeaponWarmup ? 
                                       equipmentTracker?.PrimaryEq?.PrimaryVerb?.WarmupProgress ?? this.SlideProgress : this.SlideProgress;

                if (progress >= 1)
                    return;

                progress = Mathf.Max(this.Def.minimumProgress.RandomInRange, progress);

                Vector3 endPointRelativeRotated = endpointRelative.RotatedBy(num);

                Vector3 vector         = Vector3.Lerp(A.CenterVector3 + endPointRelativeRotated * this.Def.startPointFactor.RandomInRange, 
                                                      A.CenterVector3 + endPointRelativeRotated, 
                                                      progress);

                if (vector.ShouldSpawnMotesAt(map, this.def.fleckDef.drawOffscreen))
                {
                    float           velocityAngle = (this.def.fleckUsesAngleForVelocity ? (this.def.angle.RandomInRange + num) : 0f);
                    FleckAttachLink link          = FleckAttachLink.Invalid;

                    if (this.def.fleckDef.useAttachLink && base.EffectiveSpawnLocType == MoteSpawnLocType.OnSource && A.IsValid)
                        link = new FleckAttachLink(A);

                    if (this.def.fleckDef.useAttachLink && base.EffectiveSpawnLocType == MoteSpawnLocType.OnTarget && B.IsValid)
                        link = new FleckAttachLink(B);

                    Vector3 num2 = new Vector3(Mathf.Lerp(this.Def.scaleXByStart.RandomInRange, this.Def.scaleXByEnd.RandomInRange, progress),
                                                1f,
                                                Mathf.Lerp(this.Def.scaleYByStart.RandomInRange, this.Def.scaleYByEnd.RandomInRange, progress)) * 
                                   (this.parent?.scale ?? 1f);


                    map.flecks.CreateFleck(new FleckCreationData
                                           {
                                               def               = this.def.fleckDef,
                                               exactScale        = num2,
                                               spawnPosition     = vector,
                                               rotationRate      = this.def.rotationRate.RandomInRange,
                                               rotation          = this.def.rotation.RandomInRange + num - 90f,
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

        public FloatRange startPointFactor = new(0f);
        public FloatRange endPointFactor = new(1f);

        public FloatRange scaleXByStart = new(0.8f, 1.2f);
        public FloatRange scaleYByStart = new(0.8f, 1.2f);

        public FloatRange scaleXByEnd = new(0.8f, 1.2f);
        public FloatRange scaleYByEnd = new(0.8f, 1.2f);

        public FloatRange minimumProgress = new(0f);


        public SubEffecterDef_SlideTowardsTarget() => 
            this.subEffecterClass = typeof(SubEffecter_SlideTowardsTarget);
    }

    public class FleckSystemCustom(FleckManager parent) : FleckSystemBase<FleckCustom>(parent);

    public struct FleckCustom : IFleck
    {
        public FleckDef def;

        public FleckDrawPosition position;

        public float exactRotation;

        public Vector3 originalScale;

        public Vector3 exactScale;

        public Color instanceColor;

        public float solidTimeOverride;

        public float ageSecs;

        public int ageTicks;

        public int setupTick;

        public Vector3 spawnPosition;

        public float SolidTime
        {
            get
            {
                if (!(this.solidTimeOverride < 0f))
                {
                    return this.solidTimeOverride;
                }
                return this.def.solidTime;
            }
        }

        public Vector3 DrawPos => this.position.ExactPosition;

        public float Lifespan => this.def.fadeInTime + this.SolidTime + this.def.fadeOutTime;

        public bool EndOfLife => this.ageSecs >= this.Lifespan;

        public float Alpha
        {
            get
            {
                float num = this.ageSecs;
                if (num <= this.def.fadeInTime)
                {
                    if (this.def.fadeInTime > 0f)
                    {
                        return num / this.def.fadeInTime;
                    }
                    return 1f;
                }
                if (num <= this.def.fadeInTime + this.SolidTime)
                {
                    return 1f;
                }
                if (this.def.fadeOutTime > 0f)
                {
                    return 1f - Mathf.InverseLerp(this.def.fadeInTime + this.SolidTime, this.def.fadeInTime + this.SolidTime + this.def.fadeOutTime, num);
                }
                return 1f;
            }
        }

        public Vector3 ExactScale => this.exactScale;

        public Vector3 AddedScale => this.ExactScale - this.originalScale;

        public void Setup(FleckCreationData creationData)
        {
            this.def               = creationData.def;
            this.exactScale        = Vector3.one;
            this.instanceColor     = creationData.instanceColor     ?? Color.white;
            this.solidTimeOverride = creationData.solidTimeOverride ?? (-1f);
            this.ageSecs           = 0f;
            this.exactScale        = creationData.exactScale ?? new Vector3(creationData.scale, 1f, creationData.scale);

            this.originalScale = this.ExactScale;
            this.position      = new FleckDrawPosition(creationData.spawnPosition, 0f, Vector3.zero, this.def.unattachedDrawOffset);
            this.spawnPosition = creationData.spawnPosition;
            this.exactRotation = creationData.rotation;
            this.setupTick     = Find.TickManager.TicksGame;
            if (creationData.ageTicksOverride != -1)
            {
                this.ForceSpawnTick(creationData.ageTicksOverride);
            }
        }

        public bool TimeInterval(float deltaTime, Map map)
        {
            if (this.EndOfLife)
                return true;

            this.ageSecs += deltaTime;
            this.ageTicks++;
            if (this.def.growthRate != 0f)
            {
                float num  = Mathf.Sign(this.exactScale.x);
                float num2 = Mathf.Sign(this.exactScale.z);
                this.exactScale   = new Vector3(this.exactScale.x + num * (this.def.growthRate * deltaTime), this.exactScale.y, this.exactScale.z + num2 * (this.def.growthRate * deltaTime));
                this.exactScale.x = ((num  > 0f) ? Mathf.Max(this.exactScale.x, 0.0001f) : Mathf.Min(this.exactScale.x, -0.0001f));
                this.exactScale.z = ((num2 > 0f) ? Mathf.Max(this.exactScale.z, 0.0001f) : Mathf.Min(this.exactScale.z,    -0.0001f));
            }
            if (this.def.scalers != null)
                this.def.scalers.ScaleAtTime(this.ageSecs);
            return false;
        }

        public void Draw(DrawBatch batch)
        {
            this.Draw(this.def.altitudeLayer.AltitudeFor(this.def.altitudeLayerIncOffset), batch);
        }

        public void Draw(float altitude, DrawBatch batch)
        {
            this.position.worldPosition.y = altitude;
            int num = this.setupTick + this.spawnPosition.GetHashCode();
            ((Graphic_Fleck)this.def.GetGraphicData(num).Graphic).DrawFleck(new FleckDrawData
                                                                            {
                                                                                alpha     = this.Alpha,
                                                                                color     = this.instanceColor,
                                                                                drawLayer = 0,
                                                                                pos       = this.DrawPos,
                                                                                rotation  = this.exactRotation,
                                                                                scale     = this.ExactScale,
                                                                                ageSecs   = this.ageSecs,
                                                                                id        = num
                                                                            }, batch);
        }

        public void ForceSpawnTick(int tick)
        {
            this.ageTicks = Find.TickManager.TicksGame - tick;
            this.ageSecs  = this.ageTicks.TicksToSeconds();
        }

        public Vector3 GetPosition() => 
            this.position.worldPosition;
    }
}