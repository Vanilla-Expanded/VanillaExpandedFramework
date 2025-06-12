
using System.Collections.Generic;
using System.Linq;
using KCSG;
using RimWorld;
using UnityEngine.UIElements;
using Verse;

namespace VEF.Maps
{
    public class TileMutatorWorker_GenericKCSGSpawner : TileMutatorWorker
    {
        public TileMutatorWorker_GenericKCSGSpawner(TileMutatorDef def)
            : base(def)
        {
        }

      
        public override void GenerateCriticalStructures(Map map)
        {
            
            TileMutatorExtension extension = this.def.GetModExtension<TileMutatorExtension>();
            if (extension != null)
            {

                List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
                
                int count = extension.KCSGStructuresToSpawnAmount.RandomInRange;
                Log.Message(count +" structures wanted");
                for (int i = 0; i < count; i++)
                {
                   
                    KCSG.StructureLayoutDef prefab = extension.KCSGStructuresToSpawn.RandomElement();
                    if (!MapGenUtility.TryGetRandomClearRect(prefab.Sizes.x, prefab.Sizes.z, out var rect, -1, -1, Validator))
                    {
                      
                        if (!CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(CellRect.CenteredOn(c, prefab.MaxSize)), out IntVec3 cell2))
                        {
                            return;
                        }


                      
                        rect = CellRect.CenteredOn(cell2, prefab.MaxSize);
                       
                    }
                    GenOption.GetAllMineableIn(rect, map);
                    LayoutUtils.CleanRect(prefab, map, rect, false);
                    prefab.Generate(rect, map, forceNullFaction: true);
                    map.fogGrid.Refog(rect);
                    map.fogGrid.FloodUnfogAdjacent(rect.Cells.First(), sendLetters: false);


                    usedRects.Add(rect);


                }

                bool Validator(CellRect r)
                {
                    if (!r.InBounds(map))
                    {
                        return false;
                    }
                   
                    
                    return !usedRects.Any((CellRect ur) => ur.ExpandedBy(extension.minSeparationBetweenKCSGStructures).Overlaps(r));
                }

            }
            base.GenerateCriticalStructures(map);


        }

       
    }
}