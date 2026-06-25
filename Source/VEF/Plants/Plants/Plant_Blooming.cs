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

        public bool hasWeeds = false;

        public bool alreadyBloomed = false;

        public bool plantAwaitingExtraction = false;

        public bool plantAwaitingWeedRemoval = false;

        public int lowTempBloomStopCounter = lowTempBloomStopCounterBase; //15 long ticks = 12 hours

        public const int lowTempBloomStopCounterBase = 15;

        public int itemProducedCounter;

        public int filthProducedCounter;

        public BloomingPlantExtension cachedExtension;

        public Graphic cachedGraphic = null;

        MapComponent_BloomingPlants cachedMapComp;

        CompGlowerBlooming cachedGlowingComp;

        public int cachedDeadlyTemperature = -200;

        public List<string> randomWeedMaterials = new List<string>() { "UI/Overlays/Weeds/WeedsA", "UI/Overlays/Weeds/WeedsB", "UI/Overlays/Weeds/WeedsC" };
        public string randomWeedMaterial;

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
                randomWeedMaterial = randomWeedMaterials.RandomElement();
            }
            cachedGlowingComp = this.GetComp<CompGlowerBlooming>();
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
                float currentLight = Map.glowGrid.GroundGlowAt(Position);

                //Deadly temperature check
                if (currentTemperature < cachedDeadlyTemperature)
                {
                    TakeDamage(new DamageInfo(DamageDefOf.Rotting, GetExtension.DamageWhenBelowDeadlyTemp));
                }
                //Blooming due to low or high temp stop
                if (currentTemperature < GetExtension.BloomTemperatureMin || currentTemperature > GetExtension.BloomTemperatureMax)
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

                //Blooming due to too much light stop
                if (GetExtension.BloomLightMax<1 && currentLight > GetExtension.BloomLightMax)
                {                 
                    TryEndBloom();                     
                }

                if (DetectBloomingByDate())
                {
                    if (!isBlooming)
                        TryDoBloom();
                }
                else
                {
                    if (isBlooming)
                        TryEndBloom();
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

        public bool DetectBloomingByDate()
        {
            if (Map == null) return false;

            Season currentSeason =GenDate.Season(Find.TickManager.TicksAbs,Find.WorldGrid.LongLatOf(Map.Tile));

            if (currentSeason == Season.PermanentSummer || currentSeason == Season.PermanentWinter)
            {
                int dayOfYear = GenLocalDate.DayOfYear(Map);
                int start =SeasonAsInt(GetExtension.BloomSeasonStart) * 15 +(GetExtension.BloomDayStart - 1);
                int end = SeasonAsInt(GetExtension.BloomSeasonStop) * 15 +(GetExtension.BloomDayEnd - 1);

                if (start < end)
                {
                    return dayOfYear >= start && dayOfYear < end;
                }

                return dayOfYear >= start || dayOfYear < end;
            }

            int currentDay = GenLocalDate.DayOfQuadrum(Map) + 1;

            int current = OrdinalPosition(currentSeason, currentDay);
            int startOrdinal = OrdinalPosition(GetExtension.BloomSeasonStart, GetExtension.BloomDayStart);
            int endOrdinal = OrdinalPosition(GetExtension.BloomSeasonStop, GetExtension.BloomDayEnd);

            if (startOrdinal < endOrdinal)
            {
                return current >= startOrdinal && current < endOrdinal;
            }

            return current >= startOrdinal || current < endOrdinal;
        }

        private int OrdinalPosition(Season season, int day)
        {
            return SeasonAsInt(season) * 15 + (day - 1);
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

            float currentTemp = GridsUtility.GetTemperature(Position, Map);
            if (!isBlooming && !alreadyBloomed && currentTemp >= GetExtension.BloomTemperatureMin 
                && currentTemp <= GetExtension.BloomTemperatureMax
                && (GetExtension.BloomLightMax==1 || Map.glowGrid.GroundGlowAt(Position)<= GetExtension.BloomLightMax)
                
                )
            {

                if (!GetExtension.CanBloomAgain)
                {
                    alreadyBloomed = true;
                }
                isBlooming = true;
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
                if (cachedGlowingComp != null)
                {
                    cachedGlowingComp.UpdateLit(Map);
                }
            }
        }

        public void TryEndBloom()
        {
            if (isBlooming)
            {
                isBlooming = false;
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
                if (cachedGlowingComp != null)
                {
                    cachedGlowingComp.UpdateLit(Map);
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
            Scribe_Values.Look(ref hasWeeds, "hasWeeds", false);
            Scribe_Values.Look(ref alreadyBloomed, "alreadyBloomed", false);
            Scribe_Values.Look(ref lowTempBloomStopCounter, "lowTempBloomStopCounter", lowTempBloomStopCounterBase);
            Scribe_Values.Look(ref itemProducedCounter, "itemProducedCounter", 0);
            Scribe_Values.Look(ref filthProducedCounter, "filthProducedCounter", 0);
            Scribe_Values.Look(ref randomWeedMaterial, "randomWeedMaterial", "");

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

                    float currentTemp = GridsUtility.GetTemperature(Position, Map);

                    if (isBlooming) {
                        stringBuilder.AppendLine("VPE_FlowerIsBlooming".Translate());
                    }
                    else if(currentTemp < GetExtension.BloomTemperatureMin)
                    {
                        stringBuilder.AppendLine("VPE_TempTooLowForBlooming".Translate());
                    }
                    else if (currentTemp > GetExtension.BloomTemperatureMax)
                    {
                        stringBuilder.AppendLine("VPE_TempTooHighForBlooming".Translate());
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
                if (!hasWeeds) {
                    yield return new Command_Action
                    {
                        defaultLabel = "Cause weeds",
                        action = delegate
                        {
                            hasWeeds = true;
                        }
                    };
                }
            }
            if (LifeStage == PlantLifeStage.Mature && !hasWeeds)
            {               
                yield return new Command_Action
                {
                    defaultLabel = "VPE_ExtractFlower".Translate(),
                    defaultDesc = "VPE_ExtractFlower_Desc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Gizmo/ExtractFlower"),
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
            if (hasWeeds)
            {

                yield return new Command_Action
                {
                    defaultLabel = "VPE_RemoveWeeds".Translate(),
                    defaultDesc = "VPE_RemoveWeeds_Desc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Gizmo/RemoveWeeds_Gizmo"),
                    hotKey = KeyBindingDefOf.Misc8,
                    Disabled = plantAwaitingWeedRemoval,
                    action = delegate
                    {
                        if (Map != null)
                        {
                            if (PlantsMapComp != null)
                            {
                                PlantsMapComp.AddWeedToMap(this);
                                plantAwaitingWeedRemoval = true;
                            }
                        }
                    }
                };
                if (plantAwaitingWeedRemoval)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "VPE_CancelWeedRemoval".Translate(),
                        defaultDesc = "VPE_CancelWeedRemoval_Desc".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                        hotKey = KeyBindingDefOf.Misc7,
                        action = delegate
                        {
                            if (Map != null)
                            {
                                if (PlantsMapComp != null)
                                {
                                    PlantsMapComp.RemoveWeedFromMap(this);
                                    plantAwaitingWeedRemoval = false;
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

            if (Map!=null)
            {
                Vector3 drawPos = DrawPos;
                drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor() + 0.181818187f;

                if (PlantsMapComp?.flowersOrderedForExtraction_InMap.Contains(this) == true)
                {                   
                    float num = ((float)Math.Sin((double)((Time.realtimeSinceStartup + 397f * (float)(thingIDNumber % 571)) * 4f)) + 1f) * 0.5f;
                    num = 0.3f + num * 0.7f;
                    Material material = FadedMaterialPool.FadedVersionOf(MaterialPool.MatFrom("UI/Gizmo/ExtractFlowerOverlay", ShaderDatabase.MetaOverlay), num);
                    UnityEngine.Graphics.DrawMesh(MeshPool.plane08, drawPos, Quaternion.identity, material, 0);
                }
                if (hasWeeds)
                {
                    Material material = FadedMaterialPool.FadedVersionOf(MaterialPool.MatFrom(randomWeedMaterial, ShaderDatabase.MetaOverlay), 0.8f);
                    UnityEngine.Graphics.DrawMesh(MeshPool.plane08, drawPos, Quaternion.identity, material, 0);
                }
                
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
            if (GetExtension.BloomTemperatureMin != -250) {
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "VPE_BloomTemperatureMin".Translate(), ((float)GetExtension.BloomTemperatureMin).ToStringTemperature("F0"), "VPE_BloomTemperatureMin_Desc".Translate(GetExtension.CanBloomAgain ? "VPE_CanBloom".Translate() : "VPE_CantBloom".Translate()), 4173);
            }
            if (GetExtension.BloomTemperatureMax != 999)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "VPE_BloomTemperatureMax".Translate(), ((float)GetExtension.BloomTemperatureMax).ToStringTemperature("F0"), "VPE_BloomTemperatureMax_Desc".Translate(GetExtension.CanBloomAgain ? "VPE_CanBloom".Translate() : "VPE_CantBloom".Translate()), 4173);

            }
            yield return new StatDrawEntry(StatCategoryDefOf.Basics, "VPE_DeadlyColdTemperature".Translate(), ((float)GetExtension.DeadlyColdTemperature).ToStringTemperature("F0"), "VPE_DeadlyColdTemperature_Desc".Translate(), 4174);
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
