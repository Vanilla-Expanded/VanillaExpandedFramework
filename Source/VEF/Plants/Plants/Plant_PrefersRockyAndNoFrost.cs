using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VEF.Plants
{
    public class Plant_PrefersRockyAndNoFrost : Plant
    {

        public const float MinGrowthTemperature = 10f;

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
                return GrowthRateFactor_Fertility_Inverse * GrowthRateFactor_Temperature_BelowFrost * GrowthRateFactor_Light * GrowthRateFactor_NoxiousHaze * GrowthRateFactor_Drought;
            }
        }

        public float GrowthRateFactor_Temperature_BelowFrost
        {
            get
            {
                float num;
                if (!GenTemperature.TryGetTemperatureForCell(Position, Map, out num))
                {
                    return 1f;
                }
                if (num < 20f)
                {
                    return Mathf.InverseLerp(10f, 20f, num);
                }
                if (num > 42f)
                {
                    return Mathf.InverseLerp(58f, 42f, num);
                }
                return 1f;
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
            StringBuilder stringBuilder = new StringBuilder();
            if (GrowthRateFactor_Fertility_Inverse == 0.6f)
            {
                stringBuilder.AppendLine("VCE_StuntedGrowthFertility".Translate());

            }
            else if (GrowthRateFactor_Fertility_Inverse == 0f)
            {

                stringBuilder.AppendLine("VCE_StoppedGrowthFertility".Translate());
            }
            if (this.LifeStage == PlantLifeStage.Growing)
            {
                stringBuilder.AppendLine("PercentGrowth".Translate(this.GrowthPercentString));
                stringBuilder.AppendLine("GrowthRate".Translate() + ": " + this.GrowthRate.ToStringPercent());
                if (!this.Blighted)
                {
                    if (this.Resting)
                    {
                        stringBuilder.AppendLine("PlantResting".Translate());
                    }
                    if (!this.HasEnoughLightToGrow)
                    {
                        stringBuilder.AppendLine("PlantNeedsLightLevel".Translate() + ": " + this.def.plant.growMinGlow.ToStringPercent());
                    }
                    float growthRateFactor_Temperature = this.GrowthRateFactor_Temperature_BelowFrost;
                    if (growthRateFactor_Temperature < 0.99f)
                    {
                        if (growthRateFactor_Temperature < 0.01f)
                        {
                            stringBuilder.AppendLine("OutOfIdealTemperatureRangeNotGrowing".Translate());
                        }
                        else
                        {
                            stringBuilder.AppendLine("OutOfIdealTemperatureRange".Translate(Mathf.RoundToInt(growthRateFactor_Temperature * 100f).ToString()));
                        }
                    }
                }
            }
            else if (this.LifeStage == PlantLifeStage.Mature)
            {
                if (this.HarvestableNow)
                {
                    stringBuilder.AppendLine("ReadyToHarvest".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("Mature".Translate());
                }
            }
            if (this.DyingBecauseExposedToLight)
            {
                stringBuilder.AppendLine("DyingBecauseExposedToLight".Translate());
            }
            if (this.Blighted)
            {
                stringBuilder.AppendLine("Blighted".Translate() + " (" + this.Blight.Severity.ToStringPercent() + ")");
            }
            string text = base.InspectStringPartsFromComps();
            if (!text.NullOrEmpty())
            {
                stringBuilder.Append(text);
            }



            return stringBuilder.ToString().TrimEndNewlines();
        }



    }
}
