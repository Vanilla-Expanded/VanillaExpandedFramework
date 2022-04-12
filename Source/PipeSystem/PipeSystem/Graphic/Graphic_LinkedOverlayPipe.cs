using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Link with adjacent building part of the same kind of PipeNet. For overlay.
    /// </summary>
    public class Graphic_LinkedOverlayPipe : Graphic_Linked
    {
        private readonly PipeNetDef resourceDef;

        public Graphic_LinkedOverlayPipe(Graphic innerGraphic, PipeNetDef resource) : base(innerGraphic)
        {
            resourceDef = resource;
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            return c.InBounds(parent.Map) && parent.Map.GetComponent<PipeNetManager>().GetPipeNetAt(c, resourceDef) != null;
        }

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