using RimWorld;
using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class CompProperties_Blink : CompProperties
    {

        //A comp class that makes the animal "blink" uncontrollably between different positions

        public CompProperties_Blink()
        {
            this.compClass = typeof(CompBlink);
        }

        public int blinkInterval = 500;
        public IntRange distance = new IntRange(5, 10);
        public bool warpEffect = false;
        public bool effectOnlyWhenManhunter = false;
        public bool blinkWhenManhunter = false;

    }
}