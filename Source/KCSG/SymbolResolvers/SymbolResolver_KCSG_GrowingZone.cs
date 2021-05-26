using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KCSG
{
    class SymbolResolver_KCSG_GrowingZone : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            Log.Message($"Growing zone count {CurrentGenerationOption.vectors.Count}");
            ThingDef plantDef = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.plant != null && !t.plant.cavePlant && t.plant.Harvestable && !t.plant.IsTree).RandomElement();
            ResolveParams resolveParams4 = rp;
            resolveParams4.cultivatedPlantDef = plantDef;
            resolveParams4.rect = CurrentGenerationOption.fullRect;
            BaseGen.symbolStack.Push("cultivatedPlants", resolveParams4, null);
            /*CellRect rect = CellRect.CenteredOn(rp.rect.CenterCell, 1);

            while (rect.Cells.Count() < rp.rect.Cells.Count())
            {
                foreach (IntVec3 cell in rect.EdgeCells)
                {
                    if (CurrentGenerationOption.fullRect.Contains(cell))
                    {
                        TerrainDef t = map.terrainGrid.TerrainAt(cell);
                        if (t.fertility >= plantDef.plant.fertilityMin)
                        {
                            Plant p = ThingMaker.MakeThing(plantDef) as Plant;
                            p.Growth = 0.75f;
                            GenSpawn.Spawn(p, cell, map, WipeMode.VanishOrMoveAside);
                        }
                    }
                }
                rect = CellRect.CenteredOn(rp.rect.CenterCell, rect.Width + 1, rect.Height + 1);
            }*/
            // SpreadZone(rp.rect.CenterCell, rp.rect.CenterCell, plantDef, rp.rect, map);
        }

        private void SpreadZone(IntVec3 origin, IntVec3 c, ThingDef plant, CellRect r, Map map)
        {
            if (origin.DistanceTo(c) < 10 && c.InBounds(map) && CurrentGenerationOption.fullRect.Contains(c))
            {
                TerrainDef t = map.terrainGrid.TerrainAt(c);
                if (t.fertility >= plant.plant.fertilityMin)
                {
                    Plant p = ThingMaker.MakeThing(plant) as Plant;
                    p.Growth = 0.75f;
                    GenSpawn.Spawn(p, c, map, WipeMode.VanishOrMoveAside);

                    SpreadZone(origin, c.RandomAdjacentCellCardinal(), plant, r, map);
                }
                else return;
            }
            else return;
        }
    }
}
