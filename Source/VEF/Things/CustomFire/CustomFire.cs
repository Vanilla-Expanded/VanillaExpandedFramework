using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;

namespace VEF.Things;

public class CustomFire : AttachableThing, ISizeReporter
{
    // A lot of stuff check for fire, should handle those?
    // For example, invisible pawns are forced visible while on fire.
    // Shamblers on fire have a chance to extinguish fire each tick.
    // Pawns cannot interact with a lot of stuff that's on fire, like:
    // - Pawns can't eat burning food
    // - Pawns can't sleep in burning beds
    // - NPCs raiders can't attempt to vandalize burning buildings
    // - Pawn's cant harvest burning plants
    // - Children can't play with burning toys
    // - Breastfeeding pawns can't sit on burning chairs
    // - etc.

    protected int ticksSinceSpawn;
    public float fireSize = 0.1f;
    protected int ticksSinceSpread;
    protected float flammabilityMax = 0.5f;
    public Thing instigator;
    protected int ticksUntilSmoke;
    protected Sustainer sustainer;

    protected static readonly List<Thing> FlammableList = [];
    protected static readonly CustomFireDefMap<int> FireCount = new();
    protected static readonly CustomFireDefMap<int> LastFireCountUpdateTick = new();

    public const float MinFireSize = 0.1f;
    public const int ComplexCalcsInterval = 150;
    public const float HeatFactorWhenDoorPresent = 0.15f;
    public const int FireCountParticlesOff = 15;

    public int TicksSinceSpawn => ticksSinceSpawn;

    public CustomFireDef Def => (CustomFireDef)def;

    public override string Label
    {
        get
        {
            if (parent != null)
                return Def.translationKeyFireOn.Translate(parent.LabelCap, parent);

            return def.label;
        }
    }

    protected virtual float VacuumExtinguishDamage => Def.vacuumExtinguishDamage;

    protected virtual float RainExtinguishDamage => Def.rainExtinguishDamage;

    public override string InspectStringAddon => $"{Def.translationKeyBurning.Translate()} ({Def.translationKeyFireSizeLower.Translate((fireSize * 100f).ToString("F0"))})";

    protected virtual float SpreadInterval
    {
        get
        {
            var interval = Def.spreadIntervalTicksBase - (fireSize - 1f) * Def.spreadIntervalTicksReductionPerFireSize;
            if (interval < Def.minTicksBetweenFireSpread)
                interval = Def.minTicksBetweenFireSpread;

            return interval;
        }
    }

    public override int UpdateRateTicks => 15;

    public override bool ShouldBeLitNow() => true;

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref ticksSinceSpawn, nameof(ticksSinceSpawn));
        Scribe_Values.Look(ref fireSize, nameof(fireSize));
        Scribe_References.Look(ref instigator, nameof(instigator));
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        RecalcPathsOnAndAroundMe(map);
        LessonAutoActivator.TeachOpportunity(ConceptDefOf.HomeArea, this, OpportunityType.Important);
        ticksSinceSpread = (int)(SpreadInterval * Rand.Value);
    }

    public virtual float CurrentSize() => fireSize;

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        if (sustainer != null)
        {
            sustainer.externalParams.sizeAggregator ??= new SoundSizeAggregator();
            sustainer.externalParams.sizeAggregator.RemoveReporter(this);
        }

        var prevMap = Map;
        base.DeSpawn(mode);
        RecalcPathsOnAndAroundMe(prevMap);
    }

    protected virtual void RecalcPathsOnAndAroundMe(Map map)
    {
        var adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;

        for (var i = 0; i < adjacentCellsAndInside.Length; i++)
        {
            var cell = Position + adjacentCellsAndInside[i];
            if (cell.InBounds(map))
                map.pathing.RecalculatePerceivedPathCostAt(cell);
        }
    }

    public override void AttachTo(Thing newParent)
    {
        base.AttachTo(newParent);
        if (newParent is Pawn pawn)
            TaleRecorder.RecordTale(TaleDefOf.WasOnFire, pawn);
    }

    protected override void Tick()
    {
        if (sustainer is { Ended: false })
            sustainer.Maintain();
    }

    protected override void TickInterval(int delta)
    {
        ticksSinceSpawn += delta;
        if (LastFireCountUpdateTick[Def] != Find.TickManager.TicksGame)
        {
            FireCount[Def] = Map.listerThings.ThingsOfDef(def).Count;
            LastFireCountUpdateTick[Def] = Find.TickManager.TicksGame;
        }

        if ((sustainer == null || sustainer.Ended) && !Position.Fogged(Map) && Def.burningSoundDef != null)
        {
            var info = SoundInfo.InMap(new TargetInfo(Position, Map), MaintenanceType.PerTick);
            sustainer = SustainerAggregatorUtility.AggregateOrSpawnSustainerFor(this, Def.burningSoundDef, info);
        }

        ticksUntilSmoke -= delta;
        if (ticksUntilSmoke <= 0)
            SpawnSmokeParticles();

        if (Def.fireSizeForMicroSparks >= 0f && FireCount[Def] < FireCountParticlesOff && fireSize > Def.fireSizeForMicroSparks && Rand.Chance(fireSize * Def.microSparksChancePerFireSize))
            FleckMaker.ThrowMicroSparks(DrawPos, Map);

        if (fireSize > Def.minSizeForFireSpread)
        {
            ticksSinceSpread += delta;
            if (ticksSinceSpread >= SpreadInterval)
            {
                TrySpread();
                ticksSinceSpread = 0;
            }
        }

        if (this.IsHashIntervalTick(ComplexCalcsInterval, delta))
            DoComplexCalcs();

        if (ticksSinceSpawn >= Def.ticksToBurnFloor && Def.ticksToBurnFloor >= 0)
            TryBurnFloor();
    }

    protected virtual void SpawnSmokeParticles()
    {
        if (Def.throwSmoke && FireCount[Def] < 15)
            FleckMaker.ThrowSmoke(DrawPos, Map, fireSize);

        if (Def.throwFireGlow && fireSize > 0.5f && parent == null)
            FleckMaker.ThrowFireGlow(Position.ToVector3Shifted(), Map, fireSize);

        var lerpFactor = fireSize / 2f;
        if (lerpFactor > 1f)
            lerpFactor = 1f;

        lerpFactor = 1f - lerpFactor;
        ticksUntilSmoke = Def.smokeIntervalRange.Lerped(lerpFactor) + (int)(Def.smokeIntervalRandomAddition * Rand.Value);
    }

    protected virtual void DoComplexCalcs()
    {
        var inDoorways = false;

        FlammableList.Clear();
        flammabilityMax = 0f;

        if (!Def.canExtinguishInWater || !Position.GetTerrain(Map).extinguishesFire)
        {
            Thing fireToDestroy = null;
            if (parent == null)
            {
                if (Position.TerrainFlammableNow(Map))
                    flammabilityMax = Def.Worker.TerrainFlammability(Position, Map);

                var list = Map.thingGrid.ThingsListAt(Position);
                for (var i = 0; i < list.Count; i++)
                {
                    var thing = list[i];
                    if (thing is Building_Door)
                        inDoorways = true;

                    var flammability = thing.GetStatValue(Def.flammabilityStatDef);
                    if (flammability >= 0.01f)
                    {
                        FlammableList.Add(list[i]);
                        if (flammability > flammabilityMax)
                            flammabilityMax = flammability;

                        // Consider making attached size configurable?
                        if (parent == null && fireSize > Def.minSizeToIgnitePawns && list[i].def.category == ThingCategory.Pawn && Rand.Chance(CustomFireUtility.ChanceToAttachCustomFireCumulative(list[i], ComplexCalcsInterval, Def)))
                            list[i].TryAttachCustomFire(fireSize * 0.2f, instigator, Def);
                    }

                    if (thing is Fire && Def.replacesFire)
                        fireToDestroy = thing;
                }

                fireToDestroy?.Destroy();
            }
            else
            {
                FlammableList.Add(parent);
                flammabilityMax = parent.GetStatValue(Def.flammabilityStatDef);
            }
        }

        if (flammabilityMax < 0.01f)
        {
            Destroy();
            return;
        }

        var thingToDamage = parent ?? (FlammableList.Count <= 0 ? null : FlammableList.RandomElement());
        if (thingToDamage != null && (fireSize >= 0.4f || thingToDamage == parent || thingToDamage.def.category != ThingCategory.Pawn))
            DoFireDamage(thingToDamage);

        if (Spawned)
        {
            var heat = Def.heatBase + fireSize * Def.heatPerFireSize;
            if (heat > 0)
            {
                if (inDoorways)
                    heat *= HeatFactorWhenDoorPresent;

                GenTemperature.PushHeat(Position, Map, heat);
            }

            if (Rand.Value < 0.4f)
            {
                var depth = -Def.snowClearDepthBase - fireSize * Def.snowClearDepthPerFireSize;
                if (depth != 0f)
                {
                    var radius = Def.snowClearRadiusBase + fireSize * Def.snowClearRadiusPerFireSize;
                    WeatherBuildupUtility.AddSnowRadial(Position, Map, radius, depth);
                }
            }

            var effectiveVacuumForFire = FireUtility.GetEffectiveVacuumForFire(Position, Map);

            fireSize += Def.fireBaseGrowth * flammabilityMax;
            if (Def.vacuumAffectsFireSizeGrowth)
                fireSize *= 1f - effectiveVacuumForFire;
            if (fireSize > Def.maxFireSize)
                fireSize = Def.maxFireSize;

            if (Map.weatherManager.RainRate > 0.01f && VulnerableToRain() && Rand.Chance(Def.rainExtinguishDamageChance))
                TakeDamage(new DamageInfo(Def.extinguishDamageDef, RainExtinguishDamage));
            if (effectiveVacuumForFire > 0f && VulnerableToVacuum() && Rand.Chance(Def.vacuumExtinguishDamageChance))
                TakeDamage(new DamageInfo(Def.extinguishDamageDef, VacuumExtinguishDamage * effectiveVacuumForFire));
        }
    }

    protected virtual void TryBurnFloor()
    {
        if (parent == null && Spawned && Position.TerrainFlammableNow(Map))
            Map.terrainGrid.Notify_TerrainBurned(Position);
    }

    protected virtual bool VulnerableToVacuum() => VacuumExtinguishDamage > 0.01f && Spawned;

    protected virtual bool VulnerableToRain()
    {
        if (RainExtinguishDamage <= 0.01f)
            return false;

        if (!Spawned)
            return false;

        var roofDef = Map.roofGrid.RoofAt(Position);
        if (roofDef == null)
            return true;

        if (roofDef.isThickRoof)
            return false;

        return Position.GetEdifice(Map)?.def.holdsRoof ?? false;
    }

    protected virtual void DoFireDamage(Thing targ)
    {
        var damageAmount = GenMath.RoundRandom(Def.damageRange.ClampToRange(Def.damageBase + Def.damagePerFireSize * fireSize));
        if (damageAmount <= 0)
            return;

        if (targ is Pawn pawn)
        {
            var logEntry = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire);
            Find.BattleLog.Add(logEntry);

            var damage = new DamageInfo(Def.fireDamageDef, damageAmount, 0f, -1f, instigator ?? this);
            damage.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            targ.TakeDamage(damage).AssociateWithLog(logEntry);

            if (Def.damagesApparel && pawn.apparel != null && pawn.apparel.WornApparel.TryRandomElement(out var result))
                result.TakeDamage(new DamageInfo(Def.fireDamageDef, damageAmount, 0f, -1f, instigator ?? this));
        }
        else
        {
            targ.TakeDamage(new DamageInfo(Def.fireDamageDef, damageAmount, 0f, -1f, instigator ?? this));
        }
    }

    protected virtual void TrySpread()
    {
        IntVec3 spreadPos;
        bool closeSpread;

        if (Rand.Chance(Def.chanceForNonAdjacentSpread))
        {
            spreadPos = Position + GenRadial.ManualRadialPattern[Rand.RangeInclusive(10, 20)];
            closeSpread = false;
        }
        else
        {
            spreadPos = Position + GenRadial.ManualRadialPattern[Rand.RangeInclusive(1, 8)];
            closeSpread = true;
        }

        if (!spreadPos.InBounds(Map) || !Rand.Chance(CustomFireUtility.ChanceToStartCustomFireIn(spreadPos, Map, Def)))
            return;

        if (!closeSpread)
        {
            var startRect = CellRect.SingleCell(Position);
            var endRect = CellRect.SingleCell(spreadPos);
            if (GenSight.LineOfSight(Position, spreadPos, Map, startRect, endRect))
                SpawnSpark(spreadPos);
        }
        else
        {
            CustomFireUtility.TryStartCustomFireIn(spreadPos, Map, MinFireSize, instigator, Def);
        }
    }

    protected virtual Thing SpawnSpark(IntVec3 spreadPos)
    {
        if (Def.sparkDef == null)
            return null;

        var spark = GenSpawn.Spawn(Def.sparkDef, Position, Map);

        if (spark is CustomFireSpark customSpark)
            customSpark.fireDef = Def;
        if (spark is Projectile projectile)
            projectile.Launch(this, spreadPos, spreadPos, ProjectileHitFlags.All);

        return spark;
    }

    public virtual bool TryBeatFire(Pawn pawn) => !pawn.stances.FullBodyBusy && pawn.natives.BeatFireVerb.TryStartCastOn(this);

    public virtual int SelfExtinguishDuration(Pawn pawn) => Def.selfExtinguishDuration;

    public virtual bool CausesPanic(Pawn pawn) => Def.causesPanic;

    public virtual bool CanExtinguishInWater(Pawn pawn) => Def.canExtinguishInWater;
}