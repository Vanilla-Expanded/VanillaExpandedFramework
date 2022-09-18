using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace KCSG
{
    internal class Designator_ExportToXml : Designator
    {
        public override bool Visible => Prefs.DevMode;

        public override int DraggableDimensions => 2;

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
        }

        public Designator_ExportToXml()
        {
            defaultLabel = "Export";
            defaultDesc = "Export a building to xml";
            icon = ContentFinder<Texture2D>.Get("UI/Designators/export", true);
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            return true;
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            Dialog_ExportWindow exportWindow = new Dialog_ExportWindow(base.Map, cells.ToList(), null);
            Find.WindowStack.Add(exportWindow);
        }
    }
}