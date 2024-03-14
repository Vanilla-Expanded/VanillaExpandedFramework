using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using UnityEngine;
using Verse;

namespace AnimalBehaviours
{
    [StaticConstructorOnStartup]
    public class CompGraphicByTerrain : ThingComp
    {

        public Graphic dessicatedGraphic;
        public int changeGraphicsCounter = 0;
        public string terrainName = "";
        public CompAnimalProduct animalProductComp;
        public string currentName = "";
        public int indexTerrain;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<string>(ref this.terrainName, "terrainName", "", false);
        }

        public CompProperties_GraphicByTerrain Props
        {
            get
            {
                return (CompProperties_GraphicByTerrain)this.props;
            }
        }

        public override void CompTick()
        {
            changeGraphicsCounter++;
            if (changeGraphicsCounter > Props.changeGraphicsInterval)
            {
                this.ChangeTheGraphics();
                changeGraphicsCounter = 0;
            }
            base.CompTick();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Pawn pawn = this.parent as Pawn;
            animalProductComp = this.parent.TryGetComp<CompAnimalProduct>();

            this.ChangeTheGraphics();

        }

        public void RemoveHediffs(Pawn pawn)
        {
            if (Props.hediffToApply != null)
            {
                foreach (string hediffInstance in Props.hediffToApply)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed(hediffInstance));
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }

            }
            if (Props.waterHediffToApply != "")
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed(Props.waterHediffToApply));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
            if (Props.lowTemperatureHediffToApply != "")
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed(Props.lowTemperatureHediffToApply));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
            if (Props.snowyHediffToApply != "")
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed(Props.snowyHediffToApply));
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }


        public void ChangeTheGraphics()
        {

            if (this.parent.Map != null && AnimalBehaviours_Settings.flagGraphicChanging)
            {
                Pawn pawn = this.parent as Pawn;

                Vector2 vector = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;
                if (Props.waterOverride && this.parent.Position.GetTerrain(this.parent.Map).IsWater)
                {
                    currentName = "Water";
                    if (this.terrainName != currentName)
                    {

                        this.terrainName = pawn.Position.GetTerrain(pawn.Map).defName;
                        RemoveHediffs(pawn);
                        if (Props.waterHediffToApply != "") { pawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed(Props.waterHediffToApply)); }
                        if (Props.provideSeasonalItems && animalProductComp != null) { animalProductComp.seasonalItemIndex = Props.waterSeasonalItemsIndex; }
                        pawn.Drawer.renderer.SetAllGraphicsDirty();

                    }
                }
                else
                if (Props.lowTemperatureOverride && this.parent.Map.mapTemperature.OutdoorTemp < Props.temperatureThreshold)
                {
                    currentName = "Cold";
                    if (this.terrainName != currentName)
                    {
                        this.terrainName = pawn.Position.GetTerrain(pawn.Map).defName;
                        RemoveHediffs(pawn);
                        if (Props.lowTemperatureHediffToApply != "") { pawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed(Props.lowTemperatureHediffToApply)); }
                        if (Props.provideSeasonalItems && animalProductComp != null) { animalProductComp.seasonalItemIndex = Props.lowTemperatureSeasonalItemsIndex; }
                        pawn.Drawer.renderer.SetAllGraphicsDirty();

                    }
                }
                else if (Props.snowOverride && this.parent.Position.GetSnowDepth(this.parent.Map) > 0)
                {
                    currentName = "Snowy";
                    if (this.terrainName != currentName)
                    {
                        this.terrainName = pawn.Position.GetTerrain(pawn.Map).defName;
                        RemoveHediffs(pawn);
                        if (Props.snowyHediffToApply != "") { pawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed(Props.snowyHediffToApply)); }
                        if (Props.provideSeasonalItems && animalProductComp != null) { animalProductComp.seasonalItemIndex = Props.snowySeasonalItemsIndex; }
                        pawn.Drawer.renderer.SetAllGraphicsDirty();

                    }
                }
                else

                if (Props.terrains?.Contains(pawn.Position.GetTerrain(pawn.Map).defName) == true)
                {
                    indexTerrain = Props.terrains.IndexOf(pawn.Position.GetTerrain(pawn.Map).defName);

                    currentName = pawn.Position.GetTerrain(pawn.Map).defName;
                    if (this.terrainName != currentName)
                    {
                        this.terrainName = pawn.Position.GetTerrain(pawn.Map).defName;
                        RemoveHediffs(pawn);
                        if (Props.hediffToApply != null)
                        {
                            pawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed(Props.hediffToApply[indexTerrain]));
                        }
                        if (Props.provideSeasonalItems && animalProductComp != null) { animalProductComp.seasonalItemIndex = Props.seasonalItemsIndexes[indexTerrain]; }
                        pawn.Drawer.renderer.SetAllGraphicsDirty();

                    }
                }
                else
                {
                    currentName = "Normal";
                    if (this.terrainName != currentName)
                    {
                        this.terrainName = "Normal";
                        RemoveHediffs(pawn);
                        if (Props.provideSeasonalItems && animalProductComp != null) { animalProductComp.seasonalItemIndex = 0; }
                        pawn.Drawer.renderer.SetAllGraphicsDirty();

                    }
                }
            }




        }


    }
}
