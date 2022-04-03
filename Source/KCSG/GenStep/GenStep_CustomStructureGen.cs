using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    internal class GenStep_CustomStructureGen : GenStep
    {
        public List<StructureLayoutDef> structureLayoutDefs = new List<StructureLayoutDef>();
        public bool fullClear = false;

        /* Ruin */
        public bool shouldRuin = false;
        public List<ThingDef> filthTypes = new List<ThingDef>();
        public List<ThingDef> scatterThings = new List<ThingDef>();
        public float scatterChance = 0.4f;
        public List<string> ruinSymbolResolvers = new List<string>();

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

            RectUtils.HeightWidthFromLayout(structureLayoutDef, out int h, out int w);
            CellRect cellRect = CellRect.CenteredOn(map.Center, w, h);

            GenUtils.PreClean(map, cellRect, structureLayoutDef.roofGrid, fullClear);

            foreach (List<string> item in structureLayoutDef.layouts)
            {
                GenUtils.GenerateRoomFromLayout(item, cellRect, map, structureLayoutDef);
            }

            if (shouldRuin)
            {
                CGO.factionSettlement = new CustomGenOption
                {
                    filthTypes = filthTypes,
                    scatterThings = scatterThings,
                    scatterChance = scatterChance
                };

                ResolveParams rp = new ResolveParams
                {
                    faction = map.ParentFaction,
                    rect = cellRect
                };
                foreach (string resolver in ruinSymbolResolvers)
                {
                    if (!(ruinSymbolResolvers.Contains("kcsg_randomroofremoval") && resolver == "kcsg_scatterstuffaround"))
                        BaseGen.symbolStack.Push(resolver, rp, null);
                }
            }

            // Flood refog
            if (map.mapPawns.FreeColonistsSpawned.Count > 0)
            {
                FloodFillerFog.DebugRefogMap(map);
            }
        }

        internal void SetAllFogged(Map map)
        {
            CellIndices cellIndices = map.cellIndices;
            if (map.fogGrid?.fogGrid != null)
            {
                foreach (IntVec3 c in map.AllCells)
                {
                    map.fogGrid.fogGrid[cellIndices.CellToIndex(c)] = true;
                }
                if (Current.ProgramState == ProgramState.Playing)
                {
                    map.roofGrid.Drawer.SetDirty();
                }
            }
        }
    }
}