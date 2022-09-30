using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Link with adjacent building part of the same kind of PipeNet.
    /// </summary>
    public class Graphic_LinkedPipe : Graphic_Linked
    {
        internal readonly PipeNetDef resourceDef;
        internal PipeNetManager pipeNetManager;

        public Graphic_LinkedPipe(Graphic innerGraphic, PipeNetDef resource) : base(innerGraphic)
        {
            resourceDef = resource;
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            var map = parent.Map;
            if (pipeNetManager == null || pipeNetManager.map != map)
            {
                pipeNetManager = map.GetComponent<PipeNetManager>();
            }
            return c.InBounds(map) && pipeNetManager.GetPipeNetAt(c, resourceDef) != null;
        }

        public override void Print(SectionLayer layer, Thing thing, float extraRotation)
        {
            base.Print(layer, thing, extraRotation);
            var map = thing.Map;
            var pos = thing.Position;

            for (int i = 0; i < 4; ++i)
            {
                IntVec3 adj = pos + GenAdj.CardinalDirections[i];
                if (adj.InBounds(map) && adj.GetNetTransmitter(map, thing) is Building transmitter && !resourceDef.pipeDefs.Contains(transmitter.def))
                {
                    Material mat = LinkedDrawMatFrom(thing, adj);
                    Printer_Plane.PrintPlane(layer, adj.ToVector3ShiftedWithAltitude(thing.def.Altitude), Vector2.one, mat, extraRotation);
                }
            }
        }
    }
}