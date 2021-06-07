using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_AddFields : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            CurrentGenerationOption.currentGenStep = "Generating crops fields";

            Random r = new Random();
            List<CustomVector> allFields = new List<CustomVector>();
            List<ThingDef> sourceList = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.plant != null && !t.plant.cavePlant && t.plant.Harvestable && !t.plant.IsTree);

            foreach (CustomVector c in CurrentGenerationOption.vectors)
            {
                if (CurrentGenerationOption.grid[(int)c.X][(int)c.Y].Type == CellType.NONE && AwayFromAllField(allFields, 20, c))
                {
                    ResolveParams gzp = rp;
                    gzp.cultivatedPlantDef = sourceList.RandomElement();
                    CurrentGenerationOption.currentGenStepMoreInfo = $"Generating {gzp.cultivatedPlantDef.label} field";

                    allFields.Add(c);
                    int x = rp.rect.Corners.ElementAt(2).x,
                        y = rp.rect.Corners.ElementAt(2).z;
                    IntVec3 cell = new IntVec3(x + (int)c.X + CurrentGenerationOption.radius / 2, 0, y - (int)c.Y - CurrentGenerationOption.radius / 2);

                    gzp.rect = CellRect.CenteredOn(cell, 10, 10).ClipInsideRect(rp.rect);
                    BaseGen.symbolStack.Push("cultivatedPlants", gzp, null);
                }
            }
        }

        private bool AwayFromAllField(List<CustomVector> allFields, int distance, CustomVector point)
        {
            foreach (CustomVector customVector in allFields)
            {
                if (customVector.DistanceToPow(point) < Math.Pow(distance, 2))
                    return false;
            }
            return true;
        }
    }
}