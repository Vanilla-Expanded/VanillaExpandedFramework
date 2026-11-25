using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PipeSystem;

/// <summary>
/// Makes a turret inactive (won't shoot, isn't treated as a threat) while any of its consumer
/// <see cref="CompResourceTrader"/> comps are off. Producers are ignored.
/// </summary>
public class CompProperties_TurretNeedsResource : CompProperties
{
    /// <summary>
    /// Set to true when any def uses this comp. Used by the related Harmony patches' Prepare
    /// methods, so they are only applied when needed. PipeSystem patches are applied from a
    /// StaticConstructorOnStartup, which runs after all defs had their references resolved.
    /// </summary>
    public static bool AnyDefUsesComp { get; private set; } = false;

    /// <summary>
    /// Translation key for the reason shown in the inspect string while the resources are off,
    /// as "Cannot fire: {reason}" (same format as vanilla's "Cannot fire: Roofed").
    /// Translated with the <see cref="CompResource.InspectStringArguments"/> of the first consumer
    /// whose resource is off (e.g. {RESOURCENAME}, {RESOURCEUNIT}).
    /// Set to a null or an empty string to hide the line.
    /// </summary>
    public string resourceOffReasonKey = "PipeSystem_TurretResourceOff";

    public CompProperties_TurretNeedsResource() => compClass = typeof(CompTurretNeedsResource);

    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);

        AnyDefUsesComp = true;
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (var error in base.ConfigErrors(parentDef))
            yield return error;

        if (parentDef.thingClass == null || !typeof(Building_Turret).IsAssignableFrom(parentDef.thingClass))
            yield return $"{nameof(CompProperties_TurretNeedsResource)} is used on a def whose thingClass ({parentDef.thingClass}) isn't a {nameof(Building_Turret)}, it will do nothing.";

        var hasConsumer = false;
        if (parentDef.comps != null)
        {
            foreach (var comp in parentDef.comps)
            {
                if (comp is CompProperties_ResourceTrader { consumptionPerTick: > 0f })
                {
                    hasConsumer = true;
                    break;
                }
            }
        }

        if (!hasConsumer)
            yield return $"{nameof(CompProperties_TurretNeedsResource)} is used on a def without any consumer {nameof(CompProperties_ResourceTrader)}, it will do nothing.";
    }
}