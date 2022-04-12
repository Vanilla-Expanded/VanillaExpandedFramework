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
    }
}