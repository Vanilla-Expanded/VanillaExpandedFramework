using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VanillaFurnitureExpanded
{
	public class PlaceWorker_OnWall : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			var adjacementCell = loc + rot.FacingCell;
			if (adjacementCell.InBounds(map))
			{
				var adjacementEdifice = adjacementCell.GetEdifice(map);
				if (adjacementEdifice != null && adjacementEdifice.def.IsWall())
				{
					return false;
				}
			}
			if (loc.InBounds(map))
			{
				var edifice = loc.GetEdifice(map);
				if (edifice != null && edifice.def.IsWall())
				{
					return true;
				}
			}
			return false;
		}
	}
}


