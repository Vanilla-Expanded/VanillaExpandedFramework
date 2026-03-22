using UnityEngine;
using Verse;

namespace VEF.Maps;

public class RoofExtension : DefModExtension
{
    public bool dealDamageOnCollapsed = true;
    public Color roofOverlayTint = Color.white;
    public CustomRoofGraphic customRoofGraphic = null;

    /// <summary>
    /// Used to determine early if the Harmony patch responsible for disabling damage should run or not. If false, the patch will run.
    /// </summary>
    protected internal virtual bool AlwaysDealsDamageOnCollapsed => dealDamageOnCollapsed;

    /// <summary>
    /// Used to determine early if the Harmony patch responsible for tinting the roof overlay should run or not. If true, the patch will run.
    /// </summary>
    protected internal virtual bool EverUsesCustomOverlayTint => !roofOverlayTint.IndistinguishableFrom(Color.white);

    /// <summary>
    /// Used to determine early if the SectionLayer responsible for drawing roofs should be activated or not. If true, it'll be active.
    /// </summary>
    protected internal virtual bool EverUsesCustomRoofGraphic => customRoofGraphic != null;

    /// <summary>
    /// Determines if the roof at a specific tile in a specific map should deal damage.
    /// </summary>
    /// <param name="map">The map at which the roof has collapsed</param>
    /// <param name="cell">The cell at which a roof has collapsed</param>
    /// <param name="roof">The roof that just collapsed</param>
    /// <returns>True if the damage should be dealt on a specific cell, false otherwise.</returns>
    public virtual bool DealDamageOnCollapsed(Map map, IntVec3 cell, RoofDef roof) => dealDamageOnCollapsed;

    /// <summary>
    /// Used to tint the color of the roof overlay.
    /// </summary>
    /// <param name="map">The map for which the overlay is being drawn</param>
    /// <param name="cellIndex">The cell index at which the overlay is being drawn (passed as int for performance, use Map.cellIndices to convert to IntVec3 if needed)</param>
    /// <param name="roof">The roof for which the overlay is drawn</param>
    /// <returns>Color tint applied to the original roof overlay color. White is no tint, and lets the original method run.</returns>
    public virtual Color RoofOverlayTint(Map map, int cellIndex, RoofDef roof) => roofOverlayTint;

    public class CustomRoofGraphic
    {
        // Drawing data
        public Vector2 drawSize = Vector2.one;
        public Vector3 offset = Vector3.zero;
        public AltitudeLayer layer = AltitudeLayer.Skyfaller;

        // Material data
        public string customRoofGraphicPath = null;
        public ShaderTypeDef customRoofGraphicShader = null;
        public Color customRoofGraphicColor = Color.white;
        public int renderQueue = 0;

        // Cached universal draw data.
        [Unsaved] protected RoofDrawData drawData;

        /// <summary>
        /// Draw data used for drawing roof at a specific location.
        /// </summary>
        /// <param name="map">The map at which we're drawing custom roofs at</param>
        /// <param name="cell">The cell for which we're trying to draw the graphic for</param>
        /// <param name="roof">The roof for which we're trying to draw the graphic</param>
        /// <returns>Custom </returns>
        public virtual RoofDrawData DrawDataAt(Map map, IntVec3 cell, RoofDef roof)
        {
            drawData ??= new RoofDrawData
            {
                drawSize = drawSize,
                offset = offset,
                layer = layer,
                material = MaterialPool.MatFrom(customRoofGraphicPath, customRoofGraphicShader?.Shader ?? ShaderDatabase.Cutout, customRoofGraphicColor, renderQueue),
            };

            return drawData;
        }

        public class RoofDrawData
        {
            public Vector2 drawSize;
            public Vector3 offset;
            public AltitudeLayer layer;
            public Material material;

            public virtual void Print(MapDrawLayer mapDrawLayer, IntVec3 cell)
            {
                var pos = cell.ToVector3ShiftedWithAltitude(layer) + offset;

                Graphic.TryGetTextureAtlasReplacementInfo(material, TextureAtlasGroup.Misc, false, true, out var mat, out var array, out var color);
                Printer_Plane.PrintPlane(mapDrawLayer, pos, drawSize, mat, 0, false, array, [color, color, color, color]);
            }
        }
    }
}