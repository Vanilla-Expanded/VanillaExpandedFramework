using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VanillaFurnitureExpanded
{
    public class CompSpawnOtherBuildingWhenPowered : ThingComp
    {

        //A comp class to detect whether this Building is powered (and flicked ON) and then 
        //spawn a different Building on top of it. If the first Building is flicked OFF, or
        //runs out of power, or is moved away, the second Building despawns

        //This second building needs to have:
        //<clearBuildingArea>false</clearBuildingArea>
        //<building>
        //	<isEdifice>false</isEdifice>
        //	<canPlaceOverWall>true</canPlaceOverWall>
        //</building>
        //Or it will just delete the first one! Though maybe that's what you want, I won't judge

        protected CompPowerTrader compPower;
        protected CompFlickable compFlickable;
        public Building newHologram;
        public int tickCounter = 0;


        public CompProperties_SpawnOtherBuildingWhenPowered Props
        {
            get
            {
                return (CompProperties_SpawnOtherBuildingWhenPowered)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look<Building>(ref newHologram, "newHologram");
           
        }

        //On spawn, get the power and flickable comps, and destroy the possible building if it already exists

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.compPower = this.parent.GetComp<CompPowerTrader>();
            this.compFlickable = this.parent.GetComp<CompFlickable>();
            if (newHologram != null && parent.Map != null)
            {
                if (!newHologram.Destroyed) {
                    newHologram.Destroy(DestroyMode.Vanish);
                }
               
            }
        }

        //On despawn, destroy the building

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (newHologram != null && map != null)
            {
                if (!newHologram.Destroyed)
                {
                    newHologram.Destroy(DestroyMode.Vanish);
                }

            }
        }

        //On destroy, destroy the building

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
           
            if (newHologram != null && previousMap!=null)
            {
                if (!newHologram.Destroyed)
                {
                    newHologram.Destroy(DestroyMode.Vanish);
                }

            }
        }

        //By default, it checks every 4 seconds aprox (a rare tick) but it can be set to any multiple of this

        public override void CompTickRare()
        {
            base.CompTickRare();
            tickCounter++;

            if (tickCounter >= Props.tickRaresToCheck) {

                tickCounter = 0;

                if (parent.Map != null) {

                    //If powered AND flicked ON

                    if (compPower != null ? compPower.PowerOn : false && compFlickable != null ? compFlickable.SwitchIsOn : false)
                    {
                        //Check everything at the first building position
                        bool flagToSpawnBuilding = true;
                        List<Thing> list = this.parent.Map.thingGrid.ThingsListAt(this.parent.Position);
                        for (int i = 0; i < list.Count; i++)
                        {
                            //If there is already a defOfBuildingToSpawn here, don't do anything else

                            if ((list[i] is Building) && list[i].def.defName == Props.defOfBuildingToSpawn)
                            {
                                flagToSpawnBuilding = false;
                            }
                        }
                        //If there wasn't, spawn a defOfBuildingToSpawn

                        if (flagToSpawnBuilding)
                        {

                            Building new_Building = (Building)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(Props.defOfBuildingToSpawn, true));
                            new_Building.SetFaction(Faction.OfPlayer);
                            //And preserve quality if needed! (for art)
                            if ((new_Building.TryGetComp<CompQuality>() is CompQuality qualityComp) && (this.parent.TryGetComp<CompQuality>() is CompQuality parentQualityComp))
                            {
                                qualityComp.SetQuality(parentQualityComp.Quality, ArtGenerationContext.Colony);
                            }
                            GenSpawn.Spawn(new_Building, this.parent.Position, this.parent.Map);

                            //Store the new building in a variable so it can be accessed to destroy it
                            newHologram = new_Building;
                        }
                    }
                    //If NOT powered OR flicked OFF
                    else
                    {
                        //Destroy any defOfBuildingToSpawn you find in this Position
                        List<Thing> list = this.parent.Map.thingGrid.ThingsListAt(this.parent.Position);
                        for (int i = 0; i < list.Count; i++)
                        {
                            if ((list[i] is Building) && list[i].def.defName == Props.defOfBuildingToSpawn)
                            {
                                list[i].Destroy(DestroyMode.Vanish);
                            }
                        }

                    }

                }
                
            }
           
                


        }

    }
}
