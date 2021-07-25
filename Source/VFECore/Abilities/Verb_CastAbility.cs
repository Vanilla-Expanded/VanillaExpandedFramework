namespace VFECore.Abilities
{
    using Verse;

    public class Verb_CastAbility : Verb
    {
        public Ability ability;

        protected override bool TryCastShot()
        {
            if (this.ability.IsEnabledForPawn(out _))
            {
                this.ability.Cast(this.currentTarget.IsValid ? this.currentTarget : this.CurrentDestination);
                return true;
            }

            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.ability, nameof(this.ability));
        }
    }
}
