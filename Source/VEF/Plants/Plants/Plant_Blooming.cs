using LudeonTK;
using RimWorld;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace VEF.Plants
{

    public class Plant_Blooming : Plant
    {
        public int realAge = 0;

        public bool isBlooming = false;

        public bool alreadyBloomed = false;

        public BloomingPlantExtension cachedExtension;

        public Graphic cachedGraphic = null;

        public int cachedDeadlyTemperature = -200;


        private Graphic BloomGraphic
        {
            get
            {
                if (cachedGraphic == null)
                {

                    cachedGraphic = GraphicDatabase.Get(
                        def.graphicData.graphicClass,
                        GetExtension.bloomGraphicPath,
                        def.graphic.Shader,
                        def.graphicData.drawSize,
                        def.graphicData.color,
                        def.graphicData.colorTwo);
                }

                return cachedGraphic;
            }
        }

        public BloomingPlantExtension GetExtension
        {
            get
            {
                if (cachedExtension == null)
                {
                    cachedExtension = this.def.GetModExtension<BloomingPlantExtension>();
                }
                return cachedExtension;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                cachedDeadlyTemperature = Rand.Range(GetExtension.DeadlyColdTemperature, GetExtension.DeadlyColdTemperature - 8);
            }
        }

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (Growth >= 1)
            {
                realAge += delta * 2000;
            }
            CheckIfBlooming();
        }

        public override void TickLong()
        {
            base.TickLong();
            if (Growth >= 1)
            {
                realAge += 2000;
            }
            CheckIfBlooming();
        }

        public void CheckIfBlooming()
        {
            if (Map != null) {

                if(GridsUtility.GetTemperature(Position, Map)< cachedDeadlyTemperature)
                {
                    this.Destroy();
                }

                Season season = GenDate.Season(Find.TickManager.TicksGame, Find.WorldGrid.LongLatOf(Map.Tile));
              
                if (season is Season.PermanentSummer || season is Season.PermanentWinter)
                {
                    
                }
                if (season == GetExtension.BloomSeasonStart)
                {
                    
                    if (GenLocalDate.DayOfQuadrum(Map) >= GetExtension.BloomDayStart-1)
                    {
                      
                        if (!isBlooming && !alreadyBloomed && GridsUtility.GetTemperature(Position,Map)>= GetExtension.BloomTemperatureMin)
                        {
                           
                            if (!GetExtension.CanBloomAgain)
                            {
                                alreadyBloomed = true;
                            }
                            isBlooming = true;
                            Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
                        }
                        
                    }
                  
                }
                if (season == GetExtension.BloomSeasonStop)
                {

                    
                    if (GenLocalDate.DayOfQuadrum(Map) >= GetExtension.BloomDayEnd - 1)
                    {
                        if (isBlooming)
                        {
                            isBlooming = false;
                            Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
                        }

                    }
                }

            }

        }

        public override Graphic Graphic {
            get
            {
                if (Growth >= 1 && isBlooming && !LeaflessNow) {
                    return BloomGraphic;
                }
                return base.Graphic;
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref realAge, "realAge", 0);
            Scribe_Values.Look(ref isBlooming, "isBlooming", false);
            Scribe_Values.Look(ref alreadyBloomed, "alreadyBloomed", false);

        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (def.plant.showGrowthInInspectPane)
            {
                if (LifeStage == PlantLifeStage.Growing)
                {
                   
                    stringBuilder.AppendLine("PercentGrowth".Translate(GrowthPercentString));
                   
                    stringBuilder.Append("GrowthRate".Translate() + ": " + GrowthRate.ToStringPercent());
                    if (!Blighted)
                    {
                        string[] array = ArrayPool<string>.Shared.Rent(4);
                        int count2 = 0;
                        if (Resting)
                        {
                            AddCondition(array, ref count2, "PlantResting".Translate());
                        }
                        if (!HasEnoughLightToGrow)
                        {
                            AddCondition(array, ref count2, "PlantNeedsLightLevel".Translate() + " " + def.plant.growMinGlow.ToStringPercent());
                        }
                        float growthRateFactor_Temperature = GrowthRateFactor_Temperature;
                        if (growthRateFactor_Temperature < 0.99f)
                        {
                            if (Mathf.Approximately(growthRateFactor_Temperature, 0f) || !PlantUtility.GrowthSeasonNow(base.Position, base.Map, def))
                            {
                                AddCondition(array, ref count2, "OutOfIdealTemperatureRangeNotGrowing".Translate());
                            }
                            else
                            {
                                AddCondition(array, ref count2, "OutOfIdealTemperatureRange".Translate(Mathf.Max(1, Mathf.RoundToInt(growthRateFactor_Temperature * 100f)).ToString()));
                            }
                        }
                        if (GrowthRateFactor_Drought < 0.99f)
                        {
                            AddCondition(array, ref count2, GameConditionDefOf.Drought.label);
                        }
                        string text = string.Join(", ", array, 0, count2);
                        ArrayPool<string>.Shared.Return(array);
                        if (!text.NullOrEmpty())
                        {
                            stringBuilder.Append(" (").Append(text).Append(')');
                        }
                    }
                    stringBuilder.AppendLine();
                }
                else if (LifeStage == PlantLifeStage.Mature)
                {
                    stringBuilder.AppendLine("VPE_RealAge".Translate(realAge.ToStringTicksToPeriod()));

                    stringBuilder.AppendLine(HarvestableNow ? "ReadyToHarvest".Translate() : "Mature".Translate());
                }
                if (DyingBecauseExposedToLight)
                {
                    stringBuilder.AppendLine("DyingBecauseExposedToLight".Translate());
                }
                if (DyingBecauseExposedToVacuum)
                {
                    stringBuilder.AppendLine("DyingBecauseExposedToVacuum".Translate());
                }
                if (DyingBecauseOfTerrainTags)
                {
                    stringBuilder.AppendLine("DyingBecauseOfTerrain".Translate());
                }
                if (Blighted)
                {
                    stringBuilder.AppendLine(string.Format("{0} ({1})", "Blighted".Translate(), Blight.Severity.ToStringPercent()));
                }
            }
            string text2 = InspectStringPartsFromComps();
            if (!text2.NullOrEmpty())
            {
                stringBuilder.Append(text2);
            }
            return stringBuilder.ToString().TrimEndNewlines();
            void AddCondition(string[] conditions, ref int count, string condition)
            {
                if (count < conditions.Length)
                {
                    conditions[count++] = condition;
                }
                else
                {
                    Log.Error("Too many conditions for plant growth inspect string");
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (DebugSettings.ShowDevGizmos)
            {

                yield return new Command_Action
                {
                    defaultLabel = "Increase age 1 year",
                    action = delegate
                    {
                        if (Growth >= 1)
                        {
                            this.realAge += 3600000;
                        }
                        else
                        {
                            Messages.Message("VPE_MustBeGrown".Translate(), this, MessageTypeDefOf.RejectInput, null, historical: false);
                        }
                        
                    }
                };

            }
        }


    }
}
