
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
            foreach(PlantsWithCommonality plantsWithCommonality in GetExtension.plantDefsWithCommonality)
            {
                yield return new BiomePlantRecord
                {
                    plant = plantsWithCommonality.plantDef,
                    commonality = plantsWithCommonality.commonality
                };

            }
           
        }

       
    }
}