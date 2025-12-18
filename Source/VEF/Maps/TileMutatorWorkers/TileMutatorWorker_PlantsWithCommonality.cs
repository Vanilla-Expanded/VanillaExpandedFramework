
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VEF.Maps
{
    public class TileMutatorWorker_PlantsWithCommonality : TileMutatorWorker
    {
        public TileMutatorExtension cachedExtension;

        public TileMutatorExtension GetExtension
        {
            get
            {
                if(cachedExtension == null) { 
                    cachedExtension = def.GetModExtension<TileMutatorExtension>();
                }
                return cachedExtension;
            }


        }

        public TileMutatorWorker_PlantsWithCommonality(TileMutatorDef def)
            : base(def)
        {
        }

        public override IEnumerable<BiomePlantRecord> AdditionalWildPlants(PlanetTile tile)
        {
            yield return new BiomePlantRecord
            {
                plant = GetPlantKind(tile),
                commonality = GetExtension?.plantCommonality ?? 0
            };
        }

        private ThingDef GetPlantKind(PlanetTile tile)
        {
            Rand.PushState();
            Rand.Seed = tile.GetHashCode();
            ThingDef result = def.plantKinds.RandomElement();
            Rand.PopState();
            return result;
        }
    }
}