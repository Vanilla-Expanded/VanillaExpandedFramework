using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using VanillaFurnitureExpanded;
using Verse.Sound;
using VFECore;

//These lootable buildings use base game IOpenable interface, which automatically handles the gizmo, the workgivers, the jobdrivers, etc


namespace VanillaFurnitureExpanded
{
    public class LootableBuilding : Building, IOpenable
    {

        public int OpenTicks => 300;
        LootableBuildingDetails contentDetails;

        public LootableBuildingDetails GetDetails()
        {
            if (contentDetails == null)
            {
                contentDetails = this.def.GetModExtension<LootableBuildingDetails>();
            }
            return contentDetails;
        }

        public bool CanOpen
        {
            get
            {
                GetDetails();
                if (contentDetails?.requiredMod != "")
                {
                    return ModLister.HasActiveModWithName(contentDetails?.requiredMod);
                }
                else return true;
            }
        }

        public void Open()
        {
            var comp = this.GetComp<CompBouncingArrow>();
            if (comp != null)
            {
                comp.doBouncingArrow = false;
            }
            var site = Map.Parent;
            var signal = "LootableBuildingOpened";
            Find.SignalManager.SendSignal(new Signal(signal, site.Named("SUBJECT")));
            QuestUtility.SendQuestTargetSignals(site.questTags, signal, site.Named("SUBJECT"));
            GetDetails();
            if (contentDetails != null)
            {
               
                    if (contentDetails.randomFromContents)
                    {
                        for(int i=0;i< contentDetails.totalRandomLoops.RandomInRange; i++)
                        {
                            ThingAndCount thingDefCount = contentDetails.contents.RandomElement();
                            Thing thingToMake = ThingMaker.MakeThing(thingDefCount.thing, null);
                            thingToMake.stackCount = thingDefCount.count;
                            thingToMake.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
                            GenPlace.TryPlaceThing(thingToMake, Position, Map, ThingPlaceMode.Near);
                        }
                        
                    }
                    else {

                        foreach (ThingAndCount thingDefCount in contentDetails.contents)
                        {
                            Thing thingToMake = ThingMaker.MakeThing(thingDefCount.thing, null);
                            thingToMake.stackCount = thingDefCount.count;
                            thingToMake.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
                            GenPlace.TryPlaceThing(thingToMake, Position, Map, ThingPlaceMode.Near);
                        }
                    }

                    
                    if (contentDetails.buildingLeft != null)
                    {
                        Thing buildingToMake = GenSpawn.Spawn(ThingMaker.MakeThing(contentDetails.buildingLeft), Position, Map);

                        if (buildingToMake.def.CanHaveFaction)
                        {
                            buildingToMake.SetFaction(this.Faction);
                        }
                    }
                    if (contentDetails.deconstructSound != null)
                    {
                        contentDetails.deconstructSound.PlayOneShot(this);
                    }
                    if (this.Spawned)
                    {
                        this.Destroy();
                    }
                


            }


        }

    }
}
