using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using UnityEngine.Events;
using System.Diagnostics;

namespace KCSG
{
    class Designator_ExportToXmlFromArea : Designator_Area
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
#pragma warning disable 0649
		private static Area selectedArea;
		public override void ProcessInput(Event ev)
		{
			if (!base.CheckCanInteract())
			{
				return;
			}
			if (Designator_ExportToXmlFromArea.selectedArea != null)
			{
				base.ProcessInput(ev);
			}

			Designator_ExportToXmlFromArea.MakeAllowedAreaListFloatMenu(delegate (Area a)
			{
				Log.Clear();

				KCSG_Utilities.EdgeFromArea(a.ActiveCells.ToList(), out int height, out int width);
				List<IntVec3> cellExport = KCSG_Utilities.AreaToSquare(a, height, width);

				List<string> justCreated = new List<string>();
				Dictionary<IntVec3, List<Thing>> pairsCellThingList = new Dictionary<IntVec3, List<Thing>>();

				KCSG_Utilities.FillCellThingsList(cellExport, base.Map, pairsCellThingList);

				KCSG_Utilities.CreateSymbolIfNeeded(cellExport, base.Map, justCreated, pairsCellThingList, a);

				KCSG_Utilities.CreateStructureDef(cellExport, base.Map, KCSG_Utilities.FillpairsSymbolLabel(), pairsCellThingList, a);

				Log.TryOpenLogWindow();
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
