
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_HediffWhenFleeing : CompProperties
    {

        //Applies a hediff when a creature starts fleeing. The hediff can also be applied to other pawns in a radius

        public int tickInterval = 60;
        public HediffDef hediffToCause;
        public bool graphicAndSoundEffect = false;
        public bool hediffOnRadius = false;
        public float radius = 3;


        public CompProperties_HediffWhenFleeing()
        {
            this.compClass = typeof(CompHediffWhenFleeing);
        }


    }
}