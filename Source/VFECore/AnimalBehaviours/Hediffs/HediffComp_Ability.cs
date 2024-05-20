
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    class HediffComp_Ability : HediffComp
    {

        public int tickCounter = 0;

        public HediffCompProperties_Ability Props
        {
            get
            {
                return (HediffCompProperties_Ability)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            tickCounter++;
            if (tickCounter > Props.checkingInterval)
            {
                if (this.parent.pawn.abilities is null)
                {
                    this.parent.pawn.abilities = new Pawn_AbilityTracker(this.parent.pawn);
                }
                if(this.parent.pawn.abilities.GetAbility(Props.ability) is null)
                {
                    this.parent.pawn.abilities.GainAbility(Props.ability);
                }
                AnimalCollectionClass.AddAbilityUsingAnimalToList(this.parent.pawn);
                tickCounter = 0;
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (this.parent.pawn.abilities is null)
            {
                this.parent.pawn.abilities = new Pawn_AbilityTracker(this.parent.pawn);
            }
            if (this.parent.pawn.abilities.GetAbility(Props.ability) is null)
            {
                this.parent.pawn.abilities.GainAbility(Props.ability);
            }
            AnimalCollectionClass.AddAbilityUsingAnimalToList(this.parent.pawn);
        }

        public override void CompPostPostRemoved()
        {
            this.parent.pawn.abilities.RemoveAbility(Props.ability);
        }


    }
}
