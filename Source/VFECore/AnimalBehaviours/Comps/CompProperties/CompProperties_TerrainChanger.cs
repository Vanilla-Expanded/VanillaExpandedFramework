using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_TerrainChanger : CompProperties
    {

        //Makes the animal change a given terrain to a second one, and then (optionally) that second one to a third one

        public string FirstStageTerrain = "";
        public string SecondStageTerrain = "";

        //The animal will need obedience training to do this third terrain change step
        public bool doThirdStage = false;
        public string ThirdStageTerrain = "";

        public CompProperties_TerrainChanger()
        {
            this.compClass = typeof(CompTerrainChanger);
        }
    }
}