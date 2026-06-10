using LudeonTK;
using RimWorld;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VEF.AnimalBehaviours;
using VEF.Buildings;
using Verse;
using Verse.Noise;
using static HarmonyLib.Code;
using static UnityEngine.GraphicsBuffer;

namespace VEF.Plants
{

    public class Plant_Blooming : Plant
    {
        public int realAge = 0;

        public bool isBlooming = false;

        public bool alreadyBloomed = false;

        public bool plantAwaitingExtraction = false;

        public int lowTempBloomStopCounter = lowTempBloomStopCounterBase; //15 long ticks = 12 hours

        public const int lowTempBloomStopCounterBase = 15;

        public int itemProducedCounter;

        public int filthProducedCounter;

        public BloomingPlantExtension cachedExtension;

        public Graphic cachedGraphic = null;

        MapComponent_BloomingPlants cachedMapComp;

        public int cachedDeadlyTemperature = -200;


        private Graphic BloomGraphic
        {
            get
            {
                if (cachedGraphic == null)
                {
                    string graphicToUse = GetExtension.bloomGraphicPath;
                    if (GetExtension.alternateBloomGraphicPath != "" && PlantsMapComp.alternateBloomingTextures)
                    {
                        graphicToUse = GetExtension.alternateBloomGraphicPath;
                    } 

                    cachedGraphic = GraphicDatabase.Get(
                        def.graphicData.graphicClass,
                        graphicToUse,
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

        public MapComponent_BloomingPlants PlantsMapComp
        {
            get
            {
                if (cachedMapComp is null)
                {
                    cachedMapComp = Map.GetComponent<MapComponent_BloomingPlants>(); 
                }
                return cachedMapComp;
            }
        }

        public int SeasonAsInt(Season season)
        {
            switch (season)
            {
                case Season.Spring:
                    return 0;
                case Season.Summer:
                    return 1;
                case Season.Fall:
                    return 2;
                case Season.Winter:
                    return 3;
            }
            return 1;
       }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                cachedDeadlyTemperature = Rand.Range(GetExtension.DeadlyColdTemperature, GetExtension.DeadlyColdTemperature - 8);
                if (GetExtension.itemProducedWhenBlooming != null)
                {
                    itemProducedCounter = GetExtension.longTicksPerItemProduced;
                }
                if (GetExtension.filthProducedWhenBlooming != null)
                {
                    filthProducedCounter = GetExtension.longTicksPerFilthProduced;
                }
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

                int dayOfYear = GenLocalDate.DayOfYear(Map);
                if(dayOfYear == 1)
                {
                    alreadyBloomed = false;
                }

                float currentTemperature = GridsUtility.GetTemperature(Position, Map);
                //Deadly temperature check
                if (currentTemperature < cachedDeadlyTemperature)
                {
                    TakeDamage(new DamageInfo(DamageDefOf.Rotting, 10));
                }
                //Blooming due to low temp stop
                if (currentTemperature < GetExtension.BloomTemperatureMin)
                {
                    lowTempBloomStopCounter--;
                    if(lowTempBloomStopCounter < 0)
                    {
                        TryEndBloom();
                        lowTempBloomStopCounter = lowTempBloomStopCounterBase;
                    }
                }
                else
                {
                    lowTempBloomStopCounter = lowTempBloomStopCounterBase;
                }

                //Season check. If permanent summer or winter it will transform season in the def extension to just day of the year
                Season season = GenDate.Season(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(Map.Tile));
               
                if (season is Season.PermanentSummer || season is Season.PermanentWinter)
                {
                    int dayOfYearBeginBloom = SeasonAsInt(GetExtension.BloomSeasonStart) * 15 + GetExtension.BloomDayStart;
                    int dayOfYearEndBloom = SeasonAsInt(GetExtension.BloomSeasonStop) * 15 + GetExtension.BloomDayEnd;                

                    if (dayOfYear >= dayOfYearBeginBloom - 1 && dayOfYear < dayOfYearEndBloom-1)
                    {
                        TryDoBloom();                        
                    }

                    if (dayOfYear >= dayOfYearEndBloom - 1 || dayOfYear < dayOfYearBeginBloom - 1)
                    {
                        TryEndBloom();
                    }
                }
                if (season == GetExtension.BloomSeasonStart)
                {
                   
                    if (GenLocalDate.DayOfQuadrum(Map) >= GetExtension.BloomDayStart-1)
                    {
                        TryDoBloom();
                    }                  
                }
                if (season == GetExtension.BloomSeasonStop)
                {                   
                    if (GenLocalDate.DayOfQuadrum(Map) >= GetExtension.BloomDayEnd - 1)
                    {
                        TryEndBloom();
                    }
                }
                if (isBlooming)
                {
                    //Item production when blooming
                    if (GetExtension.itemProducedWhenBlooming != null)
                    {
                        itemProducedCounter--;
                        if (itemProducedCounter <= 0)
                        {
                            Thing thing = ThingMaker.MakeThing(GetExtension.itemProducedWhenBlooming, null);
                            thing.stackCount = GetExtension.itemProducedAmount;
                            GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near);
                            itemProducedCounter = GetExtension.longTicksPerItemProduced;
                        }
                    }
                    //Filth production when blooming
                    if (GetExtension.filthProducedWhenBlooming != null)
                    {
                        filthProducedCounter--;
                        if (filthProducedCounter <= 0)
                        {
                            for (int i = 0; i < GetExtension.filthProducedAmount.RandomInRange; i++)
                            {
                                IntVec3 c;
                                CellFinder.TryFindRandomReachableNearbyCell(Position, Map, GetExtension.filthProducedRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                                FilthMaker.TryMakeFilth(c, Map, GetExtension.filthProducedWhenBlooming);
                            }
                            filthProducedCounter = GetExtension.longTicksPerFilthProduced;
                        }
                    }
                    //Hediff causing when blooming
                    if (GetExtension.hediffWhenBlooming != null)
                    {
                        foreach (Pawn pawn in Map.mapPawns.AllPawnsSpawned)
                        {
                            if (pawn != null && !pawn.IsAnimal && !pawn.Dead && !pawn.Downed &&
                                (pawn.IsColonist || !GetExtension.hediffOnlyAffectsColonists) 
                                && pawn.PositionHeld.DistanceTo(PositionHeld) <= GetExtension.hediffRadius)
                            {
                                GiveOrUpdateHediff(pawn);
                            }
                        }
                    }
                }             
            }
        }

        private void GiveOrUpdateHediff(Pawn target)
        {
            Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(GetExtension.hediffWhenBlooming);
            if (hediff == null)
            {
                hediff = target.health.AddHediff(GetExtension.hediffWhenBlooming, target.health.hediffSet.GetBrain());
                hediff.Severity = GetExtension.hediffSeverity;
                
            }
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (hediffComp_Disappears == null)
            {
                Log.ErrorOnce("CompCauseHediff_AoE has a hediff in props which does not have a HediffComp_Disappears", 78945945);
            }
            else
            {
                hediffComp_Disappears.ticksToDisappear = 4000;
            }
        }

        public void TryDoBloom() {

            if (!isBlooming && !alreadyBloomed && GridsUtility.GetTemperature(Position, Map) >= GetExtension.BloomTemperatureMin)
            {

                if (!GetExtension.CanBloomAgain)
                {
                    alreadyBloomed = true;
                }
                isBlooming = true;
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
            }
        }

        public void TryEndBloom()
        {
            if (isBlooming)
            {
                isBlooming = false;
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
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
            Scribe_Values.Look(ref lowTempBloomStopCounter, "lowTempBloomStopCounter", lowTempBloomStopCounterBase);
            Scribe_Values.Look(ref itemProducedCounter, "itemProducedCounter", 0);
            Scribe_Values.Look(ref filthProducedCounter, "filthProducedCounter", 0);

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

                    if (isBlooming) {
                        stringBuilder.AppendLine("VPE_FlowerIsBlooming".Translate());
                    }
                    else if(GridsUtility.GetTemperature(Position, Map)<GetExtension.BloomTemperatureMin)
                    {
                        stringBuilder.AppendLine("VPE_TempTooLowForBlooming".Translate());
                    }
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
            if (LifeStage == PlantLifeStage.Mature)
            {
                
                yield return new Command_Action
                {
                    defaultLabel = "VPE_ExtractFlower".Translate(),
                    defaultDesc = "VPE_ExtractFlower_Desc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/ExtractFlower"),
                    hotKey = KeyBindingDefOf.Misc6,
                    Disabled = plantAwaitingExtraction,
                    action = delegate
                    {
                        if (Map != null)
                        {                         
                            if (PlantsMapComp != null)
                            {
                                PlantsMapComp.AddObjectToMap(this);
                                plantAwaitingExtraction = true;
                            }
                        }
                    }
                };
                if (plantAwaitingExtraction) {
                    yield return new Command_Action
                    {
                        defaultLabel = "VPE_CancelExtractFlower".Translate(),
                        defaultDesc = "VPE_CancelExtractFlower_Desc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                        hotKey = KeyBindingDefOf.Misc7,                   
                        action = delegate
                        {
                            if (Map != null)
                            {
                                if (PlantsMapComp != null)
                                {
                                    PlantsMapComp.RemoveObjectFromMap(this);
                                    plantAwaitingExtraction = false;
                                }
                            }
                        }
                    };

                }

            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            if (Map!=null && PlantsMapComp?.objects_InMap.Contains(this) == true)
            {
                Vector3 drawPos = DrawPos;
                drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.181818187f;
                float num = ((float)Math.Sin((double)((Time.realtimeSinceStartup + 397f * (float)(thingIDNumber % 571)) * 4f)) + 1f) * 0.5f;
                num = 0.3f + num * 0.7f;
                Material material = FadedMaterialPool.FadedVersionOf(MaterialPool.MatFrom("UI/ExtractFlowerOverlay", ShaderDatabase.MetaOverlay), num);
                UnityEngine.Graphics.DrawMesh(MeshPool.plane08, drawPos, Quaternion.identity, material, 0);
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (StatDrawEntry item in base.SpecialDisplayStats())
            {
                yield return item;
            }
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "VPE_AgeBeautyModifier".Translate(), "+"+GetExtension.AgeBeautyModifier, "VPE_AgeBeautyModifier_Desc".Translate(GetExtension.MaxAgeBeautyModifier), 4170);
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "VPE_BloomBeautyModifier".Translate(), "x" + GetExtension.BloomBeautyModifier, "VPE_BloomBeautyModifier_Desc".Translate(), 4171);
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "VPE_BloomingPeriod".Translate(), GetExtension.BloomSeasonStart +" "+ GetExtension.BloomDayStart
                +" to "+ GetExtension.BloomSeasonStop + " " + GetExtension.BloomDayEnd, "VPE_BloomingPeriod_Desc".Translate(GetExtension.MaxAgeBeautyModifier), 4172);
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "VPE_BloomTemperatureMin".Translate(), GetExtension.BloomTemperatureMin+"ºC", "VPE_BloomTemperatureMin_Desc".Translate(GetExtension.CanBloomAgain ? "VPE_CanBloom".Translate() : "VPE_CantBloom".Translate()), 4173);
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "VPE_DeadlyColdTemperature".Translate(), GetExtension.DeadlyColdTemperature + "ºC", "VPE_DeadlyColdTemperature_Desc".Translate(), 4174);
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "VPE_LeaflessBeauty".Translate(),  GetExtension.LeaflessBeauty.ToString(), "VPE_LeaflessBeauty_Desc".Translate(), 3001);            
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (Map != null)
            {                
                if (PlantsMapComp != null)
                {
                    PlantsMapComp.RemoveObjectFromMap(this);
                }
            }
            base.DeSpawn(mode);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (Map != null)
            {             
                if (PlantsMapComp != null)
                {
                    PlantsMapComp.RemoveObjectFromMap(this);
                }
            }
            base.Destroy(mode);
        } 
    }
}
