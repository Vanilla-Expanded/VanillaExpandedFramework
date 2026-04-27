using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Weapons;

public class GaussProperties
{
    /// <summary>
    /// The list of default altitude layers that gauss projectiles can't hit.
    /// </summary>
    public static readonly List<AltitudeLayer> DefaultAltitudeLayersBlackList =
    [
        AltitudeLayer.Item,
        AltitudeLayer.ItemImportant,
        AltitudeLayer.Conduits,
        AltitudeLayer.Floor,
        AltitudeLayer.FloorEmplacement
    ];

    /// <summary>
    /// Default properties used in case custom ones aren't provided in XML.
    /// </summary>
    public static readonly GaussProperties DefaultProperties = new();

    /// <summary>
    /// Determines if friendly fire should be affected by intercept chance from distance.
    /// If true, this would mean friendly fire can't happen within 5 tiles, and would have decreased chance (scaling with distance) up to 12 tiles away.
    /// </summary>
    public bool includeInterceptChanceFromDistanceForFriendlyFire = false;
    /// <summary>
    /// Chance to hit a laying (or more precisely, non-standing) pawn that wasn't the intended target of the projectile.
    /// </summary>
    public float chanceToHitUnintendedLayingTarget = 0f;
    /// <summary>
    /// The altitude layers of things that this projectile can't hit.
    /// By default, this includes layers: Item, ItemImportant, Conduits, Floor, FloorEmplacement.
    /// </summary>
    public List<AltitudeLayer> altitudeLayersBlackList = DefaultAltitudeLayersBlackList;

    /// <summary>
    /// The stat that's used for the damage falloff calculation. Some damage workers may end up not using this stat.
    /// </summary>
    public StatDef damageModifierStat;

    /// <summary>
    /// The class for the gauss projectile damage worker. If not specified, will default to the original equation (baseDamage / (1 + (hitThings / 10))).
    /// </summary>
    public Type damageWorkerClass = typeof(GaussProjectileDefaultDamageWorker);
    [Unsaved] private GaussProjectileDamageWorker damageWorkerInt;

    public GaussProjectileDamageWorker Worker => damageWorkerInt ??= (GaussProjectileDamageWorker)Activator.CreateInstance(damageWorkerClass);

    public void ResolveReferences(ExpandableProjectileDef def)
    {
        damageModifierStat ??= VEFDefOf.VEF_GaussProjectileDamageModifier;

        // Don't assign the def for default projectiles, as the default properties are re-used.
        if (this != DefaultProperties)
            Worker.def = def;
    }
}