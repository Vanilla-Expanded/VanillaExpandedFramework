using RimWorld;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Used to show all pipe and transmitter of Resource type.
    /// Re-use the vanilla PowerGrid flag, as it being regenerated (either by us or anyone else) should
    /// have minimal impact, at least compared to the original approach of it being tied to 8 different flags.
    /// </summary>
    public class SectionLayer_Resource : SectionLayer_Things
    {
        public SectionLayer_Resource(Section section) : base(section)
        {
            requireAddToMapMesh = false;
            relevantChangeTypes = MapMeshFlagDefOf.PowerGrid;
        }

        private static int lastFrameDraw;
        private static PipeNetDef pipeNet;

        public virtual bool ShouldDraw => lastFrameDraw + 1 >= Time.frameCount;

        public static void UpdateAndDrawFor(PipeNetDef pipeNetDef)
        {
            if (pipeNetDef != pipeNet)
            {
                pipeNet = pipeNetDef;
                Find.CurrentMap.mapDrawer.WholeMapChanged(MapMeshFlagDefOf.PowerGrid);
                PipeSystemDebug.Message($"Regenerated MapMeshFlag {(ulong)MapMeshFlagDefOf.PowerGrid} for SectionLayer_Resource.");
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