using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace KCSG
{
    internal class Designator_ExportToXmlFromArea : Designator_Area
    {
        public override bool Visible
        {
            get
            {
                if (Prefs.DevMode) return true;
                else return false;
            }
        }

        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
        }

        public Designator_ExportToXmlFromArea()
        {
            this.defaultLabel = "Export from area";
            this.defaultDesc = "Export a building to xml from area";
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/exportFA", true);
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            return true;
        }

        public override void ProcessInput(Event ev)
        {
            if (!base.CheckCanInteract())
            {
                return;
            }

            MakeAllowedAreaListFloatMenu(delegate (Area a)
            {
                Log.Message($"{a}");
                RectUtils.EdgeFromArea(a.ActiveCells.ToList(), out int height, out int width);
                List<IntVec3> cellExport = RectUtils.AreaToSquare(a, height, width);

                Dialog_ExportWindow exportWindow = new Dialog_ExportWindow(base.Map, cellExport, a);
                Find.WindowStack.Add(exportWindow);
            }, false, true, base.Map);
        }

        public static void MakeAllowedAreaListFloatMenu(Action<Area> selAction, bool addNullAreaOption, bool addManageOption, Map map)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            if (addNullAreaOption)
            {
                list.Add(new FloatMenuOption("NoAreaAllowed".Translate(), delegate ()
                {
                    selAction(null);
                }, MenuOptionPriority.High, null, null, 0f, null, null));
            }
            foreach (Area localArea2 in from a in map.areaManager.AllAreas
                                        where a.AssignableAsAllowed()
                                        select a)
            {
                Area localArea = localArea2;
                FloatMenuOption item = new FloatMenuOption(localArea.Label, delegate ()
                {
                    selAction(localArea);
                }, MenuOptionPriority.Default, delegate ()
                {
                    localArea.MarkForDraw();
                }, null, 0f, null, null);
                list.Add(item);
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}