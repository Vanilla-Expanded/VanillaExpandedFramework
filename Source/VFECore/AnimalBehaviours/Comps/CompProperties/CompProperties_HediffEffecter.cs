using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnimalBehaviours
{
    public class CompProperties_HediffEffecter : CompProperties
    {

        //A comp class that makes an animal produce a certain Hediff in nearby pawns

        public int radius = 1;
        public float severity = 1.0f;
        public int tickInterval = 1000;
        public string hediff = "Plague";
        public bool notOnlyAffectColonists = false;


        public CompProperties_HediffEffecter()
        {
            this.compClass = typeof(CompHediffEffecter);
        }


    }
}
