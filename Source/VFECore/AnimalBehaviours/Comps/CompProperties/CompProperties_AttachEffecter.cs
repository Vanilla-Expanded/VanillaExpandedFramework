
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_AttachEffecter : CompProperties
    {

        public EffecterDef effecterDef;



        public CompProperties_AttachEffecter()
        {
            this.compClass = typeof(CompAttachEffecter);
        }


    }
}