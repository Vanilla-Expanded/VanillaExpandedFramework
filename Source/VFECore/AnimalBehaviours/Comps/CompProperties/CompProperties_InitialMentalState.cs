using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_InitialMentalState : CompProperties
    {

        //A comp class that makes animals always spawn with a mental state

        public MentalStateDef mentalstate;
      

        public CompProperties_InitialMentalState()
        {
            this.compClass = typeof(CompInitialMentalState);
        }
    }
}
