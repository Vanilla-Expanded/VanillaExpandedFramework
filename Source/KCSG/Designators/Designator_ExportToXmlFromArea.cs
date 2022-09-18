using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace KCSG
{
    internal class Designator_ExportToXmlFromArea : Designator_Cells
    {
        public override bool Visible => Prefs.DevMode;

        public override int DraggableDimensions => 2;

        public Designator_ExportToXmlFromArea()
        {
            defaultLabel = "Export from area";
            defaultDesc = "Export a building to xml from area";
            icon = ContentFinder<Texture2D>.Get("UI/Designators/exportFA", true);
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map))
            {
                return false;
            }
            return true;
        }

        public override void ProcessInput(Event ev)
        {
            if (!CheckCanInteract())
            {
                return;
            }

            MakeAllowedAreaListFloatMenu(delegate (Area a)
            {
                List<IntVec3> cellExport = ExportUtils.AreaToSquare(a);

                Dialog_ExportWindow exportWindow = new Dialog_ExportWindow(Map, cellExport, a);
                Find.WindowStack.Add(exportWindow);
            }, false, true, Map);
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
                }, MenuOptionPriority.Default, delegate (Rect rect)
                {
                    localArea.MarkForDraw();
                }, null, 0f, null, null);
                list.Add(item);
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}