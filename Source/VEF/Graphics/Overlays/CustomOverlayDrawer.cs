using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Graphics;

public class CustomOverlayDrawer(Map map) : MapComponent(map)
{
    // Our custom overlay type, as a constant for easy access.
    // Will be inserted into vanilla OverlayDrawer
    private const OverlayTypes CustomOverlayType = (OverlayTypes)(1 << 14);

    private static CustomOverlayDrawer cachedOverlayDrawer = null;

    private readonly Dictionary<Thing, (OverlayHandle? handle, List<CustomOverlayDef> overlays)> activeOverlays = new();

    private static readonly AccessTools.FieldRef<OverlayDrawer, DrawBatch> DrawBatchField
        = AccessTools.FieldRefAccess<OverlayDrawer, DrawBatch>(typeof(OverlayDrawer).DeclaredField("drawBatch"));
    private static readonly AccessTools.FieldRef<OverlayDrawer, Vector3> CurOffsetField
        = AccessTools.FieldRefAccess<OverlayDrawer, Vector3>(typeof(OverlayDrawer).DeclaredField("curOffset"));
    private static readonly AccessTools.FieldRef<OverlayDrawer, Dictionary<Thing, ThingOverlaysHandle>> OverlayHandles
        = AccessTools.FieldRefAccess<OverlayDrawer, Dictionary<Thing, ThingOverlaysHandle>>(typeof(OverlayDrawer).DeclaredField("overlayHandles"));

    private static readonly float BaseAlt = AltitudeLayer.MetaOverlays.AltitudeFor();

    static CustomOverlayDrawer() => CacheClearing.ClearCaches.OnClearCache += _ => cachedOverlayDrawer = null;

    /// <summary>
    /// Enable drawing of a specific overlay for a specific thing.
    /// </summary>
    /// <param name="thing">Thing for which do draw the overlay for. Must be spawned, overlay will be automatically cleared when it despawns.</param>
    /// <param name="def">The def for the overlay.</param>
    public void Enable(Thing thing, CustomOverlayDef def)
    {
        if (thing == null || def == null || !thing.Spawned)
            return;

        if (!activeOverlays.TryGetValue(thing, out var values))
        {
            values = (map.overlayDrawer.Enable(thing, CustomOverlayType), []);
            activeOverlays[thing] = values;
        }

        values.overlays.AddDistinct(def);
    }

    /// <summary>
    /// Disable drawing of a specific overlay for a specific thing.
    /// </summary>
    /// <param name="thing">Thing for which do draw the overlay for. Must be spawned, overlay will be automatically cleared when it despawns.</param>
    /// <param name="def">The def for the overlay.</param>
    public void Disable(Thing thing, CustomOverlayDef def)
    {
        if (thing == null || def == null)
            return;

        // If thing is not spawned, or doesn't exist in handles list (was removed), remove all active overlays
        if (!thing.Spawned || !OverlayHandles(map.overlayDrawer).ContainsKey(thing))
        {
            activeOverlays.Remove(thing);
            return;
        }

        if (!activeOverlays.TryGetValue(thing, out var values))
            return;

        values.overlays.Remove(def);

        if (values.overlays.Count <= 0)
        {
            map.overlayDrawer.Disable(thing, ref values.handle);
            activeOverlays.Remove(thing);
        }
    }

    internal static void PostDisposeHandle(OverlayDrawer _, Thing thing)
    {
        var map = thing.Map;
        if (cachedOverlayDrawer == null || cachedOverlayDrawer.map != map)
            cachedOverlayDrawer = map.GetComponent<CustomOverlayDrawer>();

        cachedOverlayDrawer.activeOverlays.Remove(thing);
    }

    internal static void RenderCustomOverlays(OverlayDrawer _, Thing thing, OverlayTypes overlayTypes)
    {
        // Don't do anything if we don't have custom overlay to draw
        if ((overlayTypes & CustomOverlayType) == OverlayTypes.None)
            return;

        var map = thing.Map;
        if (cachedOverlayDrawer == null || cachedOverlayDrawer.map != map)
            cachedOverlayDrawer = map.GetComponent<CustomOverlayDrawer>();

        if (cachedOverlayDrawer.activeOverlays.TryGetValue(thing, out var overlays) && overlays.overlays.Count > 0)
        {
            var drawBatch = DrawBatchField(cachedOverlayDrawer.map.overlayDrawer);
            ref var curOffset = ref CurOffsetField(cachedOverlayDrawer.map.overlayDrawer);

            foreach (var overlay in overlays.overlays)
            {
                var extraMaterials = overlay.Worker.ExtraMaterialsForThing(thing);
                if (extraMaterials.Count == 0)
                {
                    RenderOverlay(overlay.Worker.MaterialForThing(thing), 2, MeshPool.plane08, true, ref curOffset);
                    // We have no extra materials, just draw and increment offset
                    // renderPulsingOverlayDelegate(drawer, thing, overlay.Worker.MaterialForThing(thing), 2);
                }
                else
                {
                    // As opposed to having no extra materials, don't increment the offset yet
                    RenderOverlay(overlay.Worker.MaterialForThing(thing), 2, MeshPool.plane08, false, ref curOffset);
                    // renderPulsingOverlayDelegate(drawer, thing, overlay.Worker.MaterialForThing(thing), 2, false);
                    // Draw all the extra overlays in increasing altitudes, increment on the last one.
                    // Generally don't render more than 4 of those, as altInd of 7 or higher won't render with max zoom level.
                    for (var i = 0; i < extraMaterials.Count; i++)
                        RenderOverlay(extraMaterials[i], i + 3, MeshPool.plane08, i == extraMaterials.Count - 1, ref curOffset);
                        // renderPulsingOverlayDelegate(drawer, thing, extraMaterials[i], i + 3, i == extraMaterials.Count - 1);
                }

                void RenderOverlay(Material mat, int altInd, Mesh mesh, bool incrementOffset, ref Vector3 curOffset)
                {
                    var vector = thing.TrueCenter();

                    if (overlay.useCustomOffset)
                    {
                        vector += overlay.Worker.CustomOffsetForThing(thing);
                    }
                    else
                    {
                        vector.y = BaseAlt + 0.03658537f * altInd;
                        vector += curOffset;

                        if (thing.def.building is { isAttachment: true })
                            vector += (thing.Rotation.AsVector2 * 0.5f).ToVector3();
                        vector.y = Mathf.Min(vector.y, Find.Camera.transform.position.y - 0.1f);

                        if (incrementOffset)
                            curOffset.x += StackOffsetFor(thing);
                    }

                    if (overlay.pulsing)
                    {
                        var num = ((float)Math.Sin((Time.realtimeSinceStartup + 397f * (thing.thingIDNumber % 571)) * 4f) + 1f) * 0.5f;
                        num = 0.3f + num * 0.7f;
                        mat = FadedMaterialPool.FadedVersionOf(mat, num);
                    }

                    drawBatch.DrawMesh(mesh, Matrix4x4.TRS(vector, Quaternion.identity, Vector3.one), mat, 0, true);
                }
            }
        }
    }

    private static float StackOffsetFor(Thing t) => t.RotatedSize.x * 0.25f;
}