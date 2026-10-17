using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Things;

public class CustomFireDamageExtension : DefModExtension
{
    /// <summary>
    /// Determines which fire this damage is related to.
    /// </summary>
    public CustomFireDef fireDef;
    /// <summary>
    /// The initial size of the fire when attached.
    /// </summary>
    public FloatRange initialAttachedFireSize = new(0.15f, 0.25f);
    /// <summary>
    /// TThe initial size of the fire when created by an explosion.
    /// </summary>
    public FloatRange initialExplosionFireSize = new(0.2f, 0.6f);
    /// <summary>
    /// True if filth should be spawned when a thing (other than pawns) is destroyed from fire.
    /// </summary>
    public bool spawnFilthOnThingDestroyed = true;
    /// <summary>
    /// Filth that will be spawned after a thing (other than pawns) is destroyed from fire.
    /// </summary>
    public ThingDef filthOnThingDestroyed;
    /// <summary>
    /// If true, any damage def will create fire when used for explosions. If false, only the fire's damage def can create the fire.
    /// </summary>
    public bool ignoreFireDamageDefForExplosions = false;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors())
            yield return configError;

        if (fireDef == null)
            yield return $"{nameof(fireDef)} cannot be null";
        if (filthOnThingDestroyed is { IsFilth: false })
            yield return $"{nameof(filthOnThingDestroyed)} is not filth";
    }

    public override void ResolveReferences(Def parentDef)
    {
        base.ResolveReferences(parentDef);

        filthOnThingDestroyed ??= ThingDefOf.Filth_Ash;
    }
}