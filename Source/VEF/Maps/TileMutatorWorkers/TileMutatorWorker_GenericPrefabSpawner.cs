
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VEF.Maps
{
    public class TileMutatorWorker_GenericPrefabSpawner : TileMutatorWorker
    {
        public TileMutatorWorker_GenericPrefabSpawner(TileMutatorDef def)
            : base(def)
        {
        }


        public override void GenerateCriticalStructures(Map map)
        {
            base.GenerateCriticalStructures(map);
            TileMutatorExtension extension = this.def.GetModExtension<TileMutatorExtension>();
            if (extension != null)
            {

                List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
                PrefabDef prefab;
                int count = extension.prefabsToSpawnAmount.RandomInRange;
                for (int i = 0; i < count; i++)
                {
                    prefab = extension.prefabsToSpawn.RandomElement();
                    if (!MapGenUtility.TryGetRandomClearRect(prefab.size.x, prefab.size.z, out var rect, -1, -1, Validator))
                    {
                        if (!CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(CellRect.CenteredOn(c, prefab.size)), out var cell2))
                        {
                            return;
                        }
                        rect = CellRect.CenteredOn(cell2, prefab.size);
                    }

                    PrefabUtility.SpawnPrefab(prefab, map, GetPrefabRoot(rect), Rot4.North);
                    usedRects.Add(rect);

                }

                bool Validator(CellRect r)
                {
                    if (!r.InBounds(map))
                    {
                        return false;
                    }
                    if (r.Cells.Any((IntVec3 c) => c.Fogged(map)))
                    {
                        return false;
                    }
                    if (!PrefabUtility.CanSpawnPrefab(prefab, map, GetPrefabRoot(r), Rot4.North, canWipeEdifices: false))
                    {
                        return false;
                    }
                    return !usedRects.Any((CellRect ur) => ur.ExpandedBy(extension.minSeparationBetweenPrefabs).Overlaps(r));
                }

            }


        }

        private IntVec3 GetPrefabRoot(CellRect rect)
        {
            IntVec3 center = rect.CenterCell;
            if (rect.Width % 2 == 0)
            {
                center.x--;
            }
            if (rect.Height % 2 == 0)
            {
                center.z--;
            }
            return center;
        }
    }
}