using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_KCSG_RoomGen : SymbolResolver
    {
        private Dictionary<IntVec3, List<Thing>> pairsCellThingList = new Dictionary<IntVec3, List<Thing>>();

        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            SettlementLayoutDef lDef = FactionSettlement.temp;
            List<CellRect> gridRects = FactionSettlement.tempRectList;
            if (VFECore.VFEGlobal.settings.enableLog) Log.Message("KCSG_RoomGen generating " + lDef.defName.ToString());

            int count = 0;
            foreach (string str in lDef.roomLayout)
            {
                if (str != ".")
                {
                    StructureLayoutDef rld = DefDatabase<StructureLayoutDef>.GetNamed(str);

                    KCSG_Utilities.FillCellThingsList(gridRects[count].Cells.ToList(), map, pairsCellThingList);

                    foreach (List<String> item in rld.layouts)
                    {
                        KCSG_Utilities.GenerateRoomFromLayout(item, gridRects[count], map, rld);
                    }

                    if (rld.isStockpile) KCSG_Utilities.FillStockpileRoom(rld, gridRects[count], map);
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