using Verse;

namespace AnimalBehaviours
{
    public class HediffCompProperties_Animation : HediffCompProperties
    {

        public AnimationDef animation;
        public bool shamblerParticles = false;

        public HediffCompProperties_Animation()
        {
            this.compClass = typeof(HediffComp_Animation);
        }
    }
}

