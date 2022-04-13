using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Link with adjacent building part of the same kind of PipeNet. For overlay.
    /// </summary>
    public class Graphic_LinkedOverlayPipe : Graphic_LinkedPipe
    {
        public Graphic_LinkedOverlayPipe(Graphic innerGraphic, PipeNetDef resource) : base(innerGraphic, resource) { }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            foreach (IntVec3 item in thing.OccupiedRect())
            {
                Vector3 center = item.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay);
                Printer_Plane.PrintPlane(layer, center, new Vector2(1f, 1f), LinkedDrawMatFrom(thing, item), extraRotation);
            }
        }
    }
}