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
            relevantChangeTypes = MapMeshFlag.Buildings | (MapMeshFlag)455;
        }

        private static int lastFrameDraw;

        public static PipeNetDef PipeNet { get; set; }

        public virtual bool ShouldDraw => lastFrameDraw + 1 >= Time.frameCount;

        public static void UpdateAndDrawFor(PipeNetDef pipeNetDef)
        {
            if (pipeNetDef != PipeNet)
            {
                PipeNet = pipeNetDef;
                Find.CurrentMap.mapDrawer.WholeMapChanged((MapMeshFlag)455);
                PipeSystemDebug.Message("Regenerated MapMeshFlag 455 for SectionLayer_Resource.");
            }
            lastFrameDraw = Time.frameCount;
        }

        public override void DrawLayer()
        {
            if (ShouldDraw)
                base.DrawLayer();
        }

        public override void Regenerate()
        {
            ClearSubMeshes(MeshParts.All);
            // Loop in all cells of the section
            foreach (var cell in section.CellRect.Cells)
            {
                var things = GridsUtility.GetThingList(cell, Map);
                for (int o = 0; o < things.Count; o++)
                {
                    var thing = things[o];
                    if (thing is ThingWithComps thingWC)
                    {
                        var comps = thingWC.GetComps<CompResource>();
                        // Loop through comps
                        foreach (var comp in comps)
                        {
                            var compNet = comp.Props.pipeNet;
                            if (compNet == PipeNet && thing.Position.x == cell.x && thing.Position.z == cell.z)
                            {
                                LinkedPipes.GetOverlayFor(compNet).Print(this, thing, 0);
                                break; // Don't bother checking the others comps
                            }
                        }
                    }
                }
            }
            FinalizeMesh(MeshParts.All);
        }

        protected override void TakePrintFrom(Thing t) { }
    }
}