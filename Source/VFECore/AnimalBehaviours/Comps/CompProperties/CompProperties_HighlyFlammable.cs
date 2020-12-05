using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_HighlyFlammable : CompProperties
    {

        //A comp class that will make an animal get a hediff if it is set on fire

        public string hediffToInflict = "";
        public int tickInterval = 50;

        public CompProperties_HighlyFlammable()
        {
            this.compClass = typeof(CompHighlyFlammable);
        }
    }
}