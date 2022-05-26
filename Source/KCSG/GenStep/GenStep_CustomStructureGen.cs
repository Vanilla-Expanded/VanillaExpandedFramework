using System.Collections.Generic;
using System.Linq;
using RimWorld.BaseGen;
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
            StructureLayoutDef layoutDef = structureLayoutDefs.RandomElement();

            CellRect cellRect = CellRect.CenteredOn(map.Center, layoutDef.width, layoutDef.height);

            GenUtils.PreClean(map, cellRect, fullClear, layoutDef.roofGridResolved);

            for (int i = 0; i < layoutDef.layouts.Count; i++)
            {
                GenUtils.GenerateRoomFromLayout(layoutDef, i, cellRect, map);
            }
            GenUtils.GenerateRoofGrid(layoutDef, cellRect, map);

            if (shouldRuin)
            {
                GenOption.ext = new CustomGenOption
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

                for (int i = 0; i < ruinSymbolResolvers.Count; i++)
                {
                    var resolver = ruinSymbolResolvers[i];
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
                var cells = map.AllCells;
                for (int i = 0; i < cells.Count(); i++)
                {
                    var cell = cells.ElementAt(i);
                    map.fogGrid.fogGrid[cellIndices.CellToIndex(cell)] = true;
                }

                if (Current.ProgramState == ProgramState.Playing)
                {
                    map.roofGrid.Drawer.SetDirty();
                }
            }
        }
    }
}