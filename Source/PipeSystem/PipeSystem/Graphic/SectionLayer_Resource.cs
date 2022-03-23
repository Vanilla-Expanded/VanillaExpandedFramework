using RimWorld;
using System.Linq;
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
        public bool DrawNow { get; set; }
        public virtual bool ShouldDraw => lastFrameDraw + 1 >= Time.frameCount;

        public static void UpdateAndDrawFor(PipeNetDef resource)
        {
            if (resource != PipeNet)
            {
                PipeNet = resource;
                Find.CurrentMap.mapDrawer.WholeMapChanged((MapMeshFlag)455);
                PipeSystemDebug.Message("Regenerated MapMeshFlag 455 for SectionLayer_Resource.");
            }
            lastFrameDraw = Time.frameCount;
        }

        public override void DrawLayer()
        {
            if (ShouldDraw || DrawNow)
            {
                base.DrawLayer();
            }
        }

        public override void Regenerate()
        {
            ClearSubMeshes(MeshParts.All);
            var cells = section.CellRect.Cells;
            for (int i = 0; i < cells.Count(); i++)
            {
                var cell = cells.ElementAt(i);
                var thingsAt = GridsUtility.GetThingList(cell, Map).OfType<ThingWithComps>();
                for (int o = 0; o < thingsAt.Count(); o++)
                {
                    var thing = thingsAt.ElementAt(o);
                    var comps = thing.AllComps.OfType<CompResource>();
                    for (int p = 0; p < comps.Count(); p++)
                    {
                        var comp = comps.ElementAt(p);
                        if (comp.Props.pipeNet == PipeNet && thing.Position.x == cell.x && thing.Position.z == cell.z)
                            TakePrintFrom(thing);
                    }
                }
            }
            FinalizeMesh(MeshParts.All);
        }

        protected override void TakePrintFrom(Thing t)
        {
            if ((t.Faction == null || t.Faction == Faction.OfPlayer)
                && t.TryGetComp<CompResource>() is CompResource compResource
                && compResource.TransmitResourceNow)
            {
                LinkedPipes.GetOverlayFor(compResource.Props.pipeNet).Print(this, t, 0);
            }
        }
    }
}