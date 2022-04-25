namespace VFECore.Abilities
{
    using RimWorld;
    using Verse;

    public class AbilityExtension_SocialInteraction : AbilityExtension_AbilityMod
	{
		public InteractionDef interactionDef;

		public bool canApplyToMentallyBroken;

		public bool canApplyToUnconscious;
		public override void Cast(LocalTargetInfo target, Ability ability)
		{
			Pawn pawn = target.Pawn;
			if (pawn != null && ability.pawn != pawn)
			{
				ability.pawn.interactions?.TryInteractWith(pawn, interactionDef);
			}
		}

        public override bool CanApplyOn(LocalTargetInfo target, Ability ability, bool throwMessages = false)
        {
            return Valid(target, ability, throwMessages);
        }

        public override bool Valid(LocalTargetInfo target, Ability ability, bool throwMessages = false)
        {
			Pawn pawn = target.Pawn;
			if (pawn != null)
			{
				if (!canApplyToMentallyBroken && !AbilityUtility.ValidateNoMentalState(pawn, throwMessages))
				{
					return false;
				}
				if (!AbilityUtility.ValidateIsAwake(pawn, true))
				{
					return false;
				}
				if (!canApplyToUnconscious && !AbilityUtility.ValidateIsConscious(pawn, throwMessages))
				{
					return false;
				}
			}
			return true;
		}
	}
}