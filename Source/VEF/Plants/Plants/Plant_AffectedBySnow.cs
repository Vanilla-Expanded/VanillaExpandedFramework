using RimWorld;

namespace VEF.Plants
{
    public class Plant_AffectedBySnow : Plant
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
                return GrowthRateFactor_Fertility * GrowthRateFactor_Temperature * GrowthRateFactor_Light * GrowthRateFactor_NoxiousHaze * GrowthRateFactor_Drought * GrowthRateFactor_Snow;
            }
        }

        public float GrowthRateFactor_Snow
        {
            get
            {
                if (Map.weatherManager.SnowRate > 0)
                {
                    return 1.5f;
                }
                else return 1f;
            }
        }

    }
}
