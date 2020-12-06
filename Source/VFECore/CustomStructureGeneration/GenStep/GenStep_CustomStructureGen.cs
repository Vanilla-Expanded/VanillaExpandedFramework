using System;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    internal class GenStep_CustomStructureGen : GenStep
    {
        public override int SeedPart
        {
            get
            {
                return 916595355;
            }
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            StructureLayoutDef structureLayoutDef = structureLayoutDefs.RandomElement();

            KCSG_Utilities.HeightWidthFromLayout(structureLayoutDef, out int h, out int w);
            CellRect cellRect = CellRect.CenteredOn(map.Center, w, h);

            if (VFECore.VFEGlobal.settings.enableLog) Log.Message("GenStep_CustomStructureGen - " + structureLayoutDef.defName + " " + map.ParentFaction.GetCallLabel());

            int count = 1;
            foreach (List<String> item in structureLayoutDef.layouts)
            {
                KCSG_Utilities.GenerateRoomFromLayout(item, cellRect, map, structureLayoutDef);
                if (VFECore.VFEGlobal.settings.enableLog) Log.Message("Layout " + count.ToString() + " generation - PASS");
                count++;
            }

            this.SetAllFogged(map);
            foreach (IntVec3 loc in map.AllCells)
            {
                map.mapDrawer.MapMeshDirty(loc, MapMeshFlag.FogOfWar);
            }
        }

        internal void SetAllFogged(Map map)
        {
            CellIndices cellIndices = map.cellIndices;
            if (this.fogGrid == null)
            {
                this.fogGrid = new bool[cellIndices.NumGridCells];
            }
            foreach (IntVec3 c in map.AllCells)
            {
                this.fogGrid[cellIndices.CellToIndex(c)] = true;
            }
            if (Current.ProgramState == ProgramState.Playing)
            {
                map.roofGrid.Drawer.SetDirty();
            }
        }

        public bool[] fogGrid;
        public List<StructureLayoutDef> structureLayoutDefs = new List<StructureLayoutDef>();
    }
}