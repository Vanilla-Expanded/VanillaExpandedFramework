using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class CompBuildPeriodically : ThingComp
    {

        private Effecter effecter;

        public Thing thingBuilt;

        public CompProperties_BuildPeriodically Props
        {
            get
            {
                return (CompProperties_BuildPeriodically)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look<Thing>(ref this.thingBuilt, "thingBuilt",false);

        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(Props.ticksToBuild) && AnimalBehaviours_Settings.flagBuildPeriodically)
            {
                if(!Props.onlyTamed ||(Props.onlyTamed&&this.parent.Faction == Faction.OfPlayer)) { this.CreateBuildingSetup(); }
                
            }
        }

        public void CreateBuildingSetup()
        {
            if (this.parent.Map!=null&&this.parent.Map.listerThings.ThingsOfDef(ThingDef.Named(Props.defOfBuilding)).Count < Props.maxBuildingsPerMap)
            {
                Pawn pawn = this.parent as Pawn;

                if ((pawn.Map != null) && (pawn.Awake()) && (!pawn.Downed))
                {
                    if (Props.acceptedTerrains != null)
                    {
                        if (Props.acceptedTerrains.Contains(pawn.Position.GetTerrain(pawn.Map).defName))
                        {
                            CheckDuplicates(pawn);
                        }
                    }
                    else
                    {
                        CheckDuplicates(pawn);
                    }

                }
            }

            
        }

        public void CheckDuplicates(Pawn pawn)
        {
           
            if (!Props.onlyOneExistingPerPawn)
            {
                TryCreateBuilding(pawn);
            }
            else if ((thingBuilt is null || (thingBuilt != null && !this.parent.Map.listerThings.AllThings.Contains(thingBuilt))))
            {
                TryCreateBuilding(pawn);
            }
          
            

        }

        public void TryCreateBuilding(Pawn pawn)
        {
            bool buildingFound = false;
            List<Thing> list = parent.Map.thingGrid.ThingsListAt(pawn.Position);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].def.IsEdifice())
                {
                    buildingFound = true;
                }
            }

            if (!Props.checkForExistingEdifices || (Props.checkForExistingEdifices && !buildingFound)) {

                ThingDef newThing = ThingDef.Named(this.Props.defOfBuilding);
                Thing newbuilding = GenSpawn.Spawn(newThing, pawn.Position, pawn.Map, WipeMode.Vanish);
                if (Props.ifBedAssignOwnership)
                {
                    CompAssignableToPawn_Bed comp = newbuilding.TryGetComp<CompAssignableToPawn_Bed>();
                    if (comp != null)
                    {
                        comp.TryAssignPawn(pawn);
                        newbuilding.SetFaction ( Faction.OfPlayerSilentFail);

                    }
                }

                this.thingBuilt = newbuilding;

                if (this.effecter == null)
                {
                    this.effecter = EffecterDefOf.Mine.Spawn();
                }
                this.effecter.Trigger(pawn, newbuilding);

            }

            

        }


        public void NotifyBuildingDestroyed(Thing building)
        {
            if(building == this.thingBuilt)
            {
                this.thingBuilt = null;
            }

        }




    }
}
