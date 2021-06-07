using RimWorld.BaseGen;
using System.Linq;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_GridSecondPass : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            CleanGrid(rp, BaseGen.globalSettings.map);
        }

        private void CleanGrid(ResolveParams rp, Map map)
        {
            CurrentGenerationOption.currentGenStep = "Preparing road generation";

            int x = rp.rect.Corners.ElementAt(2).x,
                y = rp.rect.Corners.ElementAt(2).z;

            for (int i = 0; i < CurrentGenerationOption.grid.Length; i++)
            {
                for (int j = 0; j < CurrentGenerationOption.grid[i].Length; j++)
                {
                    IntVec3 cell = new IntVec3(x + i, 0, y - j);
                    if (CurrentGenerationOption.grid[i][j].Type == CellType.BUILDING)
                    {
                        if (!cell.Roofed(map))
                        {
                            CurrentGenerationOption.currentGenStepMoreInfo = $"Changing {cell} value";
                            if (cell.GetFirstBuilding(map) is Building b && b != null)
                            {
                                if (b.def.passability != Traversability.Impassable)
                                {
                                    CurrentGenerationOption.grid[i][j].Type = CellType.BUILDINGPASSABLE;
                                }
                            }
                            else
                            {
                                CurrentGenerationOption.grid[i][j].Type = CellType.NONE;
                            }
                        }
                    }
                }
            }

            foreach (CustomVector item in CurrentGenerationOption.doors)
            {
                if (!AStar.GetAdjacent(item, CurrentGenerationOption.grid, false).FindAll(c => c.Type == CellType.NONE).Any())
                {
                    item.Type = CellType.BUILDING;
                    CurrentGenerationOption.grid[(int)item.X][(int)item.Y].Type = CellType.BUILDING;
                }
            }

            BaseGen.symbolStack.Push("kcsg_generateroad", rp, null);
        }
    }
}