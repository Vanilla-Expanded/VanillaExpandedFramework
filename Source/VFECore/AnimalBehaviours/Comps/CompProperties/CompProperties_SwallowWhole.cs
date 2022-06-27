
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    public class CompProperties_SwallowWhole : CompProperties
    {

        //A comp class that provides parameters for Pawn_SwallowWhole and DamageWorker_SwallowWhole

        //The total amount of prey it can hold inside
        public int stomachCapacity = 5;

        //Animals above this max body size won't be swallowed whole
        public float maximumBodysize = 30;

        //Amount of nutrition the animal will gain by swallowing prey (5 is basically all filled)
        public int nutritionGained = 5;

        //DefName of a Sound played when eating. If null, no sound will play
        public string soundPlayedWhenEating = null;

        //Send a letter when the animal devours a colony-owned pawn
        public bool sendLetterWhenEating = false;
        public string letterLabel = "";
        public string letterText = "";

        //Digestion time in rare ticks
        public int digestionPeriod = 240; // 1 day

        //Create filth around it if killed or otherwise destroyed
        public bool createFilthWhenKilled = false;
        public ThingDef filthToMake;
        //Play sound if killed or otherwise destroyed
        public bool playSoundWhenKilled = false;
        public string soundToPlay = "Hive_Spawn";




        public CompProperties_SwallowWhole()
        {
            this.compClass = typeof(CompSwallowWhole);
        }


    }
}