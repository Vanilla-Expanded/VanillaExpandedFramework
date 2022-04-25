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
			if (CanApplyOn(target))
            {
				Apply(target, ability);
			}
		}

		public void Apply(LocalTargetInfo target, Ability ability)
		{
			Pawn pawn = target.Pawn;
			if (pawn != null && ability.pawn != pawn)
			{
				ability.pawn.interactions?.TryInteractWith(pawn, interactionDef);
			}
		}

		public bool CanApplyOn(LocalTargetInfo target)
		{
			return Valid(target);
		}

		public bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			Pawn pawn = target.Pawn;
			if (pawn != null)
			{
				if (!canApplyToMentallyBroken && !AbilityUtility.ValidateNoMentalState(pawn, throwMessages))
				{
					return false;
				}
				if (!AbilityUtility.ValidateIsAwake(pawn, throwMessages))
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