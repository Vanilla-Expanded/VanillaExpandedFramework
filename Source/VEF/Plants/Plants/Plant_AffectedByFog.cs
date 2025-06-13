
using RimWorld;


namespace VEF.Plants
{
    public class Plant_AffectedByFog : Plant
    {


        public override float GrowthRate
        {
            get
            {
                if (Blighted)
                {
                    return 0f;
                }
                if (base.Spawned && !PlantUtility.GrowthSeasonNow(base.Position, base.Map, def))
                {
                    return 0f;
                }
                return GrowthRateFactor_Fertility * GrowthRateFactor_Temperature * GrowthRateFactor_Light * GrowthRateFactor_NoxiousHaze * GrowthRateFactor_Drought * this.GrowthRateFactor_Fog;
            }
        }

        public float GrowthRateFactor_Fog
        {
            get
            {
                if (this.Map.weatherManager.curWeather.defName.Contains("fog") || this.Map.weatherManager.curWeather.defName.Contains("Fog"))
                {
                    return 1.3f;
                }
                else return 1f;
            }
        }
    }
}
