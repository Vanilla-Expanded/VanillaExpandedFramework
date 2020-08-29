using RimWorld;
using Verse;

namespace VanillaFurnitureExpanded
{
    public class CompProperties_SpawnOtherBuildingWhenPowered : CompProperties
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

        public CompProperties_SpawnOtherBuildingWhenPowered()
        {
            this.compClass = typeof(CompSpawnOtherBuildingWhenPowered);
        }

        public string defOfBuildingToSpawn = "HorseshoesPin";
        public int tickRaresToCheck = 1;
    }
}