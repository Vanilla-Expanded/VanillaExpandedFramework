using RimWorld;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Used to show all pipe and transmitter of Resource type.
    /// Introduce a new MapMeshFlag, 455. We use it to switch between resource pipe to higlight.
    /// </summary>
    public class SectionLayer_Resource : SectionLayer_Things
    {
        public SectionLayer_Resource(Section section) : base(section)
        {
            requireAddToMapMesh = false;
            relevantChangeTypes = MapMeshFlagDefOf.Buildings | 455;
        }

        private static int lastFrameDraw;
        private static PipeNetDef pipeNet;

        public virtual bool ShouldDraw => lastFrameDraw + 1 >= Time.frameCount;

        public static void UpdateAndDrawFor(PipeNetDef pipeNetDef)
        {
            if (pipeNetDef != pipeNet)
            {
                pipeNet = pipeNetDef;
                Find.CurrentMap.mapDrawer.WholeMapChanged(455);
                PipeSystemDebug.Message("Regenerated MapMeshFlag 455 for SectionLayer_Resource.");
            }
            lastFrameDraw = Time.frameCount;
        }

        public override void DrawLayer()
        {
            if (ShouldDraw)
                base.DrawLayer();
        }

        protected override void TakePrintFrom(Thing t)
        {
            if (t is ThingWithComps twc)
            {
                var comps = twc.AllComps;
                if (comps != null)
                {
                    for (int i = 0; i < comps.Count; i++)
                    {
                        if (comps[i] is CompResource comp && comp.Props.pipeNet == pipeNet)
                        {
                            comp.CompPrintForResourceGrid(this);
                            break; // We can only have one comp of the same netDef
                        }
                    }
                }
            }
        }
    }
}