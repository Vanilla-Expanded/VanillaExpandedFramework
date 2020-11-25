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
                return 916516155;
            }
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            StructureLayoutDef structureLayoutDef = structureLayoutDefs.RandomElement();

            KCSG_Utilities.HeightWidthFromLayout(structureLayoutDef, out int h, out int w);
            CellRect cellRect = CellRect.CenteredOn(map.Center, w, h);

            if (structureLayoutDef.terrainGrid != null)
            {
                KCSG_Utilities.GenerateTerrainFromLayout(cellRect, map, structureLayoutDef);
                if (VFECore.VFEGlobal.settings.enableLog) Log.Message("Terrain generation - PASS");
            }
            int count = 1;
            foreach (List<String> item in structureLayoutDef.layouts)
            {
                KCSG_Utilities.GenerateRoomFromLayout(item, cellRect, map, structureLayoutDef);
                if (VFECore.VFEGlobal.settings.enableLog) Log.Message("Layout " + count.ToString() + " generation - PASS");
                count++;
            }
        }

        public List<StructureLayoutDef> structureLayoutDefs = new List<StructureLayoutDef>();
    }
}