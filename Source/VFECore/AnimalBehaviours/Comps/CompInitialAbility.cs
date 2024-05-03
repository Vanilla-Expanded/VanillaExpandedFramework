using Verse;
using System.Linq;
using RimWorld;

namespace AnimalBehaviours
{
    public class CompInitialAbility : ThingComp
    {
        private bool addHediffOnce = true;
     
        public CompProperties_InitialAbility Props
        {
            get
            {
                return (CompProperties_InitialAbility)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.addHediffOnce, "addHediffOnce", true, false);
          
        }

        public override void CompTickRare()
        {

            base.CompTickRare();

            //addHediffOnce is used (and saved) so the hediff is only added once when the creature spawns
            if (addHediffOnce)
            {
                Pawn pawn = this.parent as Pawn;

                if(pawn.abilities is null)
                {
                    pawn.abilities = new Pawn_AbilityTracker(pawn);
                }

                pawn.abilities.GainAbility(Props.initialAbility);

                addHediffOnce = false;
            }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            AnimalCollectionClass.AddAbilityUsingAnimalToList(this.parent);
           

        }

        public override void PostDeSpawn(Map map)
        {
            AnimalCollectionClass.RemoveAbilityUsingFromList(this.parent);
           
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            AnimalCollectionClass.RemoveAbilityUsingFromList(this.parent);
           
        }
    }
}

