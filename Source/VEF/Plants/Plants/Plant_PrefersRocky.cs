using RimWorld;
using Verse;

namespace VEF.Plants
{
    public class Plant_PrefersRocky : Plant
    {

        public override float GrowthRate
        {
            get
            {
                if (Blighted)
                {
                    return 0f;
                }
                if (Spawned && !PlantUtility.GrowthSeasonNow(Position, Map, def))
                {
                    return 0f;
                }
                return GrowthRateFactor_Fertility_Inverse * GrowthRateFactor_Temperature * GrowthRateFactor_Light * GrowthRateFactor_NoxiousHaze * GrowthRateFactor_Drought;
            }
        }

        public float GrowthRateFactor_Fertility_Inverse
        {
            get
            {
                float fertilityAtCell = Map.fertilityGrid.FertilityAt(Position);

                if (fertilityAtCell <= 0.7)
                {
                    return 1f;
                }
                else if (fertilityAtCell > 0.7 && fertilityAtCell <= 1)
                {
                    return 0.6f;
                }
                else
                {
                    return 0;
                }


            }
        }

        public override string GetInspectString()
        {
            if (GrowthRateFactor_Fertility_Inverse == 0.6)
            {
                return base.GetInspectString() + "\n" + "VCE_StuntedGrowthFertility".Translate();

            }
            else if (GrowthRateFactor_Fertility_Inverse == 0)
            {
                return base.GetInspectString() + "\n" + "VCE_StoppedGrowthFertility".Translate();

            }
            else return base.GetInspectString();


        }

    }
}
