using Verse;
using System.Linq;
using RimWorld;

namespace AnimalBehaviours
{
    public class CompNoTamingDecay : ThingComp
    {
      

        public CompProperties_NoTamingDecay Props
        {
            get
            {
                return (CompProperties_NoTamingDecay)this.props;
            }
        }

       
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            AnimalCollectionClass.AddNoTamingDecayAnimalToList(this.parent.def);


        }

       
    }
}

