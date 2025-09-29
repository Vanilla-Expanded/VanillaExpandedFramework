using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VEF.Maps;

public class CustomOverlayWorker(CustomOverlayDef def)
{
    private static readonly List<Material> EmptyList = [];

    public readonly CustomOverlayDef def = def;

    /// <summary>
    /// The main material which will be drawn as the overlay.
    /// </summary>
    /// <param name="thing">The thing for which the overlay is being drawn.</param>
    /// <returns>The material which will be drawn as the overlay. Unless overriden, it'll return material specified by <see cref="CustomOverlayDef.overlayPath"/>.</returns>
    public virtual Material MaterialForThing(Thing thing) => def.CachedMaterial;

    /// <summary>
    /// Materials which will be drawn (in order) on top of the main material.
    /// Recommended 4 or less, as otherwise top-most materials won't be drawn if zoomed-in too much.
    /// </summary>
    /// <param name="thing">The thing for which the overlay is being drawn.</param>
    /// <returns>A list of extra materials to draw on top of the main one. Unless overridden, it'll return an empty list.</returns>
    public virtual List<Material> ExtraMaterialsForThing(Thing thing) => EmptyList;

    /// <summary>
    /// An offset which will be used instead of the default behaviour. This offset won't be affected by other overlays being drawn.
    /// Requires <see cref="CustomOverlayDef.useCustomOffset"/> to be true.
    /// </summary>
    /// <param name="thing">The thing for which the overlay is being drawn.</param>
    /// <returns>The offset which will be used instead of the default behaviour. Unless overriden, it'll return offset specified by <see cref="CustomOverlayDef.customOffset"/>.</returns>
    public virtual Vector3 CustomOffsetForThing(Thing thing) => def.customOffset;
}