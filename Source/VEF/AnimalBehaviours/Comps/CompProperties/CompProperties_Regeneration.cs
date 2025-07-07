using Verse;

namespace VEF.AnimalBehaviours
{
    public class CompProperties_Regeneration : CompProperties
    {

        //A very simple class that regenerates wounds

        public int rateInTicks = 1000;
        public float healAmount = 0.1f;
        public bool healAll = true;
        public bool needsSun = false;
        public bool needsWater = false;
        public bool onlyBleeding = false;
        public bool onlyTendButNotHeal = false;

        //If not null, regeneration will only affect this BodyPartDef
        public BodyPartDef bodypart =null;


        public CompProperties_Regeneration()
        {
            this.compClass = typeof(CompRegeneration);
        }


    }
}