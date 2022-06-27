using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Create a deconstruct designator specific for a pipe def.
    /// </summary>
    public class Designator_DeconstructPipe : Designator_Deconstruct
    {
        public readonly PipeNetDef pipeNetDef;

        public Designator_DeconstructPipe(PipeNetDef pipeNet)
        {
            pipeNetDef = pipeNet;
            defaultLabel = "PipeSystem_DeconstructLabel".Translate(pipeNet.resource.name);
            defaultDesc = "PipeSystem_DeconstructDesc".Translate(pipeNet.resource.name);
            icon = ContentFinder<Texture2D>.Get(pipeNet.designator.deconstructIconPath);
            hotKey = null;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (!pipeNetDef.pipeDefs.Contains(t.def)) return false;
            if (t is Blueprint_Build blueprint && !pipeNetDef.pipeDefs.Contains(blueprint.def.entityDefToBuild)) return false;
            return base.CanDesignateThing(t);
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
            SectionLayer_Resource.UpdateAndDrawFor(pipeNetDef);
        }
    }
}