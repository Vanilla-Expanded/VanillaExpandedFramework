using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KCSG
{
    class SymbolResolver_AddFields : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Random r = new Random(CurrentGenerationOption.seed);
            List<CustomVector> allFields = new List<CustomVector>();

            foreach (CustomVector c in CurrentGenerationOption.vectors)
            {
                if (CurrentGenerationOption.grid[(int)c.X][(int)c.Y].Type == CellType.NONE && AwayFromAllField(allFields, 20, c))
                {
                    allFields.Add(c);

                    int x = rp.rect.Corners.ElementAt(2).x,
                        y = rp.rect.Corners.ElementAt(2).z;
                    IntVec3 cell = new IntVec3(x + (int)c.X + CurrentGenerationOption.radius / 2, 0, y - (int)c.Y - CurrentGenerationOption.radius / 2);
                    
                    ResolveParams gzp = rp;
                    gzp.rect = CellRect.CenteredOn(cell, 10, 10).ClipInsideRect(rp.rect);
                    gzp.cultivatedPlantDef = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.plant != null && !t.plant.cavePlant && t.plant.Harvestable && !t.plant.IsTree).RandomElement();

                    BaseGen.symbolStack.Push("cultivatedPlants", gzp, null);
                }
            }
        }

        private bool AwayFromAllField(List<CustomVector> allFields, int distance, CustomVector point)
        {
            foreach (CustomVector customVector in allFields)
            {
                if (customVector.DistanceTo(point) < distance)
                    return false;
            }
            return true;
        }
    }
}
