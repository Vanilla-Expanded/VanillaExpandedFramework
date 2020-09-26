using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Verse;
using Verse.AI.Group;
using RimWorld;
using RimWorld.BaseGen;

namespace KCSG
{
    class SymbolResolver_KCSG_RoomGen : SymbolResolver
    {
		private Dictionary<IntVec3, List<Thing>> pairsCellThingList = new Dictionary<IntVec3, List<Thing>>();

		public override void Resolve(ResolveParams rp)
        {
			Map map = BaseGen.globalSettings.map;
			SettlementLayoutDef lDef = map.ParentFaction.def.GetModExtension<FactionSettlement>().temp;
			List<CellRect> gridRects = map.ParentFaction.def.GetModExtension<FactionSettlement>().tempRectList;

			int count = 0;
			foreach (string str in lDef.roomLayout)
			{
				if (str != ".")
                {
					StructureLayoutDef rld = DefDatabase<StructureLayoutDef>.GetNamed(str);
					KCSG_Utilities.FillCellThingsList(gridRects[count].Cells.ToList(), map, pairsCellThingList);
					if (rld.terrainGrid != null) KCSG_Utilities.GenerateTerrainFromLayout(gridRects[count], map, rld, pairsCellThingList);
					foreach (List<String> item in rld.layouts)
					{
						KCSG_Utilities.GenerateRoomFromLayout(item, gridRects[count], map, rld, pairsCellThingList);
					}
					if (rld.isStockpile) KCSG_Utilities.FillStockpileRoom(rld, gridRects[count], map, pairsCellThingList);
				}
				count++;
			}

			ThingDef conduit;
			if (LoadedModManager.RunningMods.ToList().FindAll(m => m.Name == "Subsurface Conduit").Count > 0) conduit = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(d => d.defName == "MUR_SubsurfaceConduit").First();
			else conduit = ThingDefOf.PowerConduit;

			KCSG_Utilities.EnsureBatteriesConnectedAndMakeSense(map, tmpThings, tmpPowerNetPredicateResults, tmpCells, conduit);
			KCSG_Utilities.EnsurePowerUsersConnected(map, tmpThings, tmpPowerNetPredicateResults, tmpCells, conduit);
			KCSG_Utilities.EnsureGeneratorsConnectedAndMakeSense(map, tmpThings, tmpPowerNetPredicateResults, tmpCells, conduit);
		}

		private List<Thing> tmpThings = new List<Thing>();
		private Dictionary<PowerNet, bool> tmpPowerNetPredicateResults = new Dictionary<PowerNet, bool>();
		private List<IntVec3> tmpCells = new List<IntVec3>();
	}
}
