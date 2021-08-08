using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_TerrainChanger : CompProperties
    {

        //Makes the animal change a given terrain to a second one, and then (optionally) that second one to a third one

        public int checkingRate = 100;

        public string FirstStageTerrain = "";
        public string SecondStageTerrain = "";

        //The animal will need obedience training to do this third terrain change step
        public bool doThirdStage = false;
        public string ThirdStageTerrain = "";

        //Act in a radius, instead of on the pawn's position
        public bool inRadius = false;
        public int radius = 2;

        public CompProperties_TerrainChanger()
        {
            this.compClass = typeof(CompTerrainChanger);
        }
    }
}