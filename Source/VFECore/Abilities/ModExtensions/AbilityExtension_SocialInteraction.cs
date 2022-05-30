namespace VFECore.Abilities
{
    using RimWorld;
    using RimWorld.Planet;
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
            return Valid(new[] { target.ToGlobalTargetInfo(target.Thing.Map) }, ability, throwMessages);
        }

        public override bool Valid(GlobalTargetInfo[] targets, Ability ability, bool throwMessages = false)
        {
			foreach (GlobalTargetInfo target in targets)
            {
				Pawn pawn = target.Thing as Pawn;
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
			}
			return base.Valid(targets, ability, throwMessages);
        }
	}
}