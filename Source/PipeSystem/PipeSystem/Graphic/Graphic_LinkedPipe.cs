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
    }
}