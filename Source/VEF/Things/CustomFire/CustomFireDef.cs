using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Things;

public class CustomFireDef : ThingDef
{
    /// <summary>
    /// Maximum size the fire can reach.
    /// </summary>
    public float maxFireSize = 1.75f;
    /// <summary>
    /// The amount that the fire will grow by each complex calculations interval (150 ticks), multiplied by flammability of whatever is on fire.
    /// </summary>
    public float fireBaseGrowth = 0.00055f * CustomFire.ComplexCalcsInterval;

    /// <summary>
    /// Base damage dealt by the fire.
    /// </summary>
    public float damageBase = 0.0125f * CustomFire.ComplexCalcsInterval * 2;
    /// <summary>
    /// Extra damage dealt by the fire per fire size.
    /// </summary>
    public float damagePerFireSize = 0.0036f * CustomFire.ComplexCalcsInterval * 2;
    /// <summary>
    /// Min/max damage dealt by the fire. Fractional values will be randomly rounded up or down (with the chance based on the fractional part).
    /// As opposed to vanilla fire, 0 damage is supported.
    /// </summary>
    public FloatRange damageRange = new(0.0125f * CustomFire.ComplexCalcsInterval * 2, 0.05f * CustomFire.ComplexCalcsInterval * 2);

    /// <summary>
    /// Base amount of heat (independent of fire size) pushed each complex calculations interval (150 ticks). Negative values will cool down the area instead.
    /// </summary>
    public float heatBase = 0f;
    /// <summary>
    /// Amount of heat pushed per fire size each complex calculations interval (150 ticks). Negative values will cool down the area instead.
    /// </summary>
    public float heatPerFireSize = 160f;

    /// <summary>
    /// The base radius (independent of fire size) used for clearing snow around the fire. Radius of under 1 will only affect the cell with fire itself.
    /// </summary>
    public float snowClearRadiusBase = 0f;
    /// <summary>
    /// The radius based on fire size used for clearing snow around the fire. Radius of under 1 will only affect the cell with fire itself.
    /// </summary>
    public float snowClearRadiusPerFireSize = 3f;
    /// <summary>
    /// The base snow clear depth (independent of fire size) used for clearing snow around the fire. Negative values will increase the snow amount.
    /// </summary>
    public float snowClearDepthBase = 0f;
    /// <summary>
    /// The snow clear depth based on fire size used for clearing snow around the fire. Negative values will increase the snow amount.
    /// </summary>
    public float snowClearDepthPerFireSize = 0.1f;

    /// <summary>
    /// Amount of damage the fire will take from rain.
    /// </summary>
    public float rainExtinguishDamage = 10f;
    /// <summary>
    /// Chance each complex calculation (150 ticks) that rain will take damage from active rain.
    /// This is different from vanilla, as in vanilla this value is 6 (which is pointless, as it has no effect over 1 - 0.04 base * 150 ticks = 6).
    /// </summary>
    public float rainExtinguishDamageChance = 1f;

    /// <summary>
    /// Amount of damage the fire will take from vacuum, scaled with vacuum at its location.
    /// </summary>
    public float vacuumExtinguishDamage = 20f;
    /// <summary>
    /// Determines the chance for the fire to take damage from vacuum.
    /// </summary>
    public float vacuumExtinguishDamageChance = 1f;
    /// <summary>
    /// Determines if the fire's size growth should be affected by the vacuum.
    /// </summary>
    public bool vacuumAffectsFireSizeGrowth = true;

    /// <summary>
    /// Minimum size of the fire before it can ignite a nearby pawn. Use "Infinity" if you want to disable fire spreading to pawns.
    /// </summary>
    public float minSizeToIgnitePawns = 0.4f;
    /// <summary>
    /// Amount of ticks it takes for a pawn to extinguish themselves.
    /// </summary>
    public int selfExtinguishDuration = 150;
    /// <summary>
    /// Determines if this fire should be affected by vanilla extinguish damage (on top of specified extinguishDamageDef).
    /// If extinguishDamageDef is left untouched/as vanilla extinguish damage this should remain as true.
    /// </summary>
    public bool affectedByExtinguishDamage = true;
    /// <summary>
    /// Determines if a pawn will panic while they are set on fire.
    /// </summary>
    public bool causesPanic = true;
    /// <summary>
    /// Determines if water can be used to extinguish by an intelligent enough pawn (Intelligence of ToolUser or Humanoid).
    /// </summary>
    public bool canExtinguishInWater = true;
    /// <summary>
    /// The chance that a pawn on fire will extinguish themselves while they are on fire.
    /// </summary>
    public float chanceToSelfExtinguish = 0.1f;

    /// <summary>
    /// Minimum size the fire must be before it can spread.
    /// </summary>
    public float minSizeForFireSpread = 1f;
    /// <summary>
    /// The base amount of ticks between 2 attempts for fire to spread.
    /// </summary>
    public float spreadIntervalTicksBase = 150f;
    /// <summary>
    /// The amount of ticks reduced per fire size between fire spread attempts.
    /// </summary>
    public float spreadIntervalTicksReductionPerFireSize = 40f;
    /// <summary>
    /// Minimum amount of ticks between 2 fire spread attempts.
    /// </summary>
    public float minTicksBetweenFireSpread = 75f;
    /// <summary>
    /// Chance for fire to spread to a non-adjacent tile, creating a spark in the process.
    /// </summary>
    public float chanceForNonAdjacentSpread = 0.2f;
    /// <summary>
    /// Amount of damage the fire will take when pawns are beating the fire out.
    /// </summary>
    public float beatFireExtinguishDamageAmount = 16f;

    /// <summary>
    /// Amount of ticks since the fire has spawned before the floor under it is burned down. Negative values disable burning floors down.
    /// </summary>
    public int ticksToBurnFloor = 7500;
    /// <summary>
    /// True if the fire should damage apparel of pawns wearing it, false otherwise.
    /// </summary>
    public bool damagesApparel = true;
    /// <summary>
    /// True if the custom fire should replace vanilla fire if it occupies the same cell.
    /// </summary>
    public bool replacesFire = false;
    /// <summary>
    /// True if the vanilla "fire in home area" alert should include this fire type, false results in no alert.
    /// If you want a custom alert, you need to make one in C#. 
    /// </summary>
    public bool useVanillaFireAlert = true;
    /// <summary>
    /// True if the default fire fighting work giver should be used. False will not use a work giver, this pawns assigned to firefighting won't automatically beat this fire out.
    /// </summary>
    public bool useDefaultWorkGiver = true;

    /// <summary>
    /// The interval between smoke flecks and fire glow flecks, interpolated between the two values using half the current fire's size.
    /// A fire of size 0 will have 200 ticks between smoke, while a fire of size 2 will have 130 ticks.
    /// Only smoke and fire glow flecks supported, custom flecks/motes require custom C# for the fire.
    /// </summary>
    public IntRange smokeIntervalRange = new(130, 200);
    /// <summary>
    /// A random amount of ticks (between 0 and this value) will be added to the smoke interval.
    /// </summary>
    public int smokeIntervalRandomAddition = 10;
    /// <summary>
    /// True if smoke fleck should be spawned by the fire.
    /// </summary>
    public bool throwSmoke = true;
    /// <summary>
    /// True if fire glow fleck should be spawned by the fire.
    /// </summary>
    public bool throwFireGlow = true;
    /// <summary>
    /// Minimum fire size required to throw micro sparks. Negative numbers disable micro sparks.
    /// </summary>
    public float fireSizeForMicroSparks = 0.7f;
    /// <summary>
    /// Chance for micro sparks to spawn each 15 ticks per fire size.
    /// </summary>
    public float microSparksChancePerFireSize = 0.01f;

    /// <summary>
    /// Translation key (vanilla key: 'FireOn') used for alternative fire label if it's attached to something.
    /// 2 arguments: thing attached to (capitalized), thing attached to.
    /// </summary>
    public string translationKeyFireOn = "FireOn";
    /// <summary>
    /// Translation key (vanilla key: `Burning`) used for inspect string on anything it's attached to. Part of "Burning (fire size {0})" text.
    /// No arguments.
    /// </summary>
    public string translationKeyBurning = "Burning";
    /// <summary>
    /// Translation key (vanilla key: `FireSizeLower`) used for inspect string on anything it's attached to. Part of "Burning (fire size {0})" text.
    /// Single argument for fire size (size multiplied by 100 to be more user-readable).
    /// No arguments.
    /// </summary>
    public string translationKeyFireSizeLower = "FireSizeLower";
    /// <summary>
    /// Translation key (vanilla key: `ReportExtinguishingFire`) used for JobDriver_ExtinguishCustomFireSelf.
    /// No arguments.
    /// </summary>
    public string translationKeyReportExtinguishingFire = "ReportExtinguishingFire";
    /// <summary>
    /// Translation key (vanilla key: `ReportExtinguishingFireOn`) used for JobDriver_ExtinguishCustomFireSelf.
    /// 2 arguments: thing attached to (capitalized), thing attached to (named argument: 'TARGET').
    /// </summary>
    public string translationKeyReportExtinguishingFireOn = "ReportExtinguishingFireOn";
    /// <summary>
    /// Translation key (vanilla equivalent: BeatFire JobDef's reportString) used for JobDriver_BeatCustomFire. Optional, base report string will be used instead.
    /// No arguments.
    /// </summary>
    public string translationKeyReportBeatFire = null;
    /// <summary>
    /// Translation key (vanilla equivalent: ExtinguishFiresNearby JobDef's reportString) used for JobDriver_BeatCustomFire. Optional, base report string will be used instead.
    /// No arguments.
    /// </summary>
    public string translationKeyReportExtinguishFiresNearby = null;

    /// <summary>
    /// The def used for sparks while spreading to non-adjacent tiles. Defaults to vanilla fire sparks, thus spreading fire. Use a custom def with CustomFireSpark class.
    /// </summary>
    public ThingDef sparkDef;
    /// <summary>
    /// The damage def used by the fire damage. Defaults to Flame, same as vanilla flame.
    /// </summary>
    public DamageDef fireDamageDef;
    /// <summary>
    /// The damage def used for extinguishing the flame. Defaults to vanilla Extinguish. The custom fire itself is also generally going to support the vanilla Extinguish damage as well.
    /// </summary>
    public DamageDef extinguishDamageDef;
    /// <summary>
    /// The stat used for flammability of the pawn. Uses vanilla Flammability stat by default.
    /// </summary>
    public StatDef flammabilityStatDef;
    /// <summary>
    /// Job def used for beating fire.
    /// </summary>
    public JobDef beatFireJobDef;
    /// <summary>
    /// Job def used for extinguishing fires nearby.
    /// </summary>
    public JobDef extinguishFiresNearbyJobDef;
    /// <summary>
    /// Job def used for pawns extinguishing themselves.
    /// </summary>
    public JobDef extinguishSelfJobDef;
    /// <summary>
    /// Sound used for burning fire. Defaults to vanilla FireBurning def.
    /// </summary>
    public SoundDef burningSoundDef;

    /// <summary>
    /// Curve used to calculate the chance for a pawn to catch fire, based on their flammability.
    /// </summary>
    public SimpleCurve chanceToCatchFirePerSecondForPawnFromFlammability =
    [
        new CurvePoint(0f, 0f),
        new CurvePoint(0.1f, 0.07f),
        new CurvePoint(0.3f, 1f),
        new CurvePoint(1f, 1f)
    ];

    /// <summary>
    /// The worker used for things that aren't (fully) related to existing fire.
    /// For example, worker used for handling extinguishing fire in an area - it doesn't check for a particular fire instance, but any related fire in an area.
    /// </summary>
    public Type customFireWorkerType = typeof(CustomFireWorker);

    [Unsaved] public ushort fireIndex = ushort.MaxValue;

    public static bool AnyFireAffectedByVanillaExtinguishDamage { get; protected set; } = false;
    public static bool AnyFireCausesPanic { get; protected set; } = false;

    public CustomFireWorker Worker
    {
        get
        {
            if (field == null)
            {
                try
                {
                    field = (CustomFireWorker)Activator.CreateInstance(customFireWorkerType);
                    // Log an error, re-initialize with default worker.
                    if (field == null)
                        throw new NullReferenceException();
                    field.fireDef = this;
                }
                catch (Exception e)
                {
                    Log.Error($"Caught an exception initializing {nameof(CustomFireWorker)} using worker type {customFireWorkerType}, using default worker as fallback. Exception:\n{e}");
                    field = new CustomFireWorker
                    {
                        fireDef = this,
                    };
                }
            }

            return field;
        }
    }

    public override void ResolveReferences()
    {
        base.ResolveReferences();

        fireDamageDef ??= DamageDefOf.Flame;
        sparkDef ??= VEFDefOf.VEF_BaseCustomSparkSpark;
        extinguishDamageDef ??= DamageDefOf.Extinguish;
        flammabilityStatDef ??= StatDefOf.Flammability;
        beatFireJobDef ??= VEFDefOf.VEF_BeatCustomFire;
        extinguishFiresNearbyJobDef ??= VEFDefOf.VEF_ExtinguishCustomFiresNearby;
        extinguishSelfJobDef ??= VEFDefOf.VEF_ExtinguishCustomFireSelf;
        burningSoundDef ??= SoundDefOf.FireBurning;
    }

    public override void PostLoad()
    {
        base.PostLoad();

        if (affectedByExtinguishDamage)
            AnyFireAffectedByVanillaExtinguishDamage = true;
        if (causesPanic)
            AnyFireCausesPanic = true;
    }

    public override void PostSetIndices()
    {
        base.PostSetIndices();

        if (fireIndex == ushort.MaxValue && DefDatabase<CustomFireDef>.DefCount > 0)
        {
            for (var i = 0; i < DefDatabase<CustomFireDef>.AllDefsListForReading.Count; i++)
                DefDatabase<CustomFireDef>.AllDefsListForReading[i].fireIndex = (ushort)i;
        }
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors())
            yield return configError;

        if (extinguishDamageDef == DamageDefOf.Extinguish && !affectedByExtinguishDamage)
            yield return $"Attempted to use {nameof(DamageDefOf)} {nameof(DamageDefOf.Extinguish)} while {nameof(affectedByExtinguishDamage)} is false.";
    }
}