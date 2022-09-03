using RimWorld;
using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class CompProperties_BuildPeriodically : CompProperties
    {

        //A comp class to make animals periodically create buildings around them. Terrains where buildings are accepted can
        //be specified

        public CompProperties_BuildPeriodically()
        {
            this.compClass = typeof(CompBuildPeriodically);
        }

        public string defOfBuilding = "";
        public int ticksToBuild = 1000;
        public int maxBuildingsPerMap = 10;
        public List<string> acceptedTerrains = null;
        public bool onlyOneExistingPerPawn = false;
        public bool checkForExistingEdifices = false;
        public bool ifBedAssignOwnership = false;
        public bool onlyTamed = false;
    }
}