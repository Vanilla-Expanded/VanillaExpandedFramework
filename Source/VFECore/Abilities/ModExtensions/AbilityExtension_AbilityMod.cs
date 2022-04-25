namespace VFECore.Abilities
{
    using Verse;
    using Verse.AI;

    public class AbilityExtension_AbilityMod : DefModExtension
    {
        [Unsaved] public AbilityDef abilityDef;

        public virtual bool IsEnabledForPawn(Ability ability, out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public virtual string GetDescription(Ability ability) =>
            string.Empty;

        public virtual void WarmupToil(Toil toil)
        {
        }

        public virtual void Cast(LocalTargetInfo target, Ability ability)
        {

        }
    }
}