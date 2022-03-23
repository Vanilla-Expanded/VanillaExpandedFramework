using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Link with adjacent building part of the same kind of PipeNet.
    /// </summary>
    public class Graphic_LinkedPipe : Graphic_Linked
    {
        private readonly PipeNetDef resourceDef;

        public Graphic_LinkedPipe(Graphic innerGraphic, PipeNetDef resource) : base(innerGraphic)
        {
            resourceDef = resource;
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            return c.InBounds(parent.Map) && parent.Map.GetComponent<PipeNetManager>().GetPipeNetAt(c, resourceDef) != null;
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            base.Print(layer, thing, extraRotation);
            for (int i = 0; i < 4; i++)
            {
                IntVec3 intVec = thing.Position + GenAdj.CardinalDirections[i];
                if (intVec.InBounds(thing.Map))
                {
                    Building transmitter = intVec.GetTransmitter(thing.Map);
                    if (transmitter != null && !transmitter.def.graphicData.Linked)
                    {
                        Material mat = LinkedDrawMatFrom(thing, intVec);
                        Printer_Plane.PrintPlane(layer, intVec.ToVector3ShiftedWithAltitude(thing.def.Altitude), Vector2.one, mat, extraRotation);
                    }
                }
            }
        }
    }
}