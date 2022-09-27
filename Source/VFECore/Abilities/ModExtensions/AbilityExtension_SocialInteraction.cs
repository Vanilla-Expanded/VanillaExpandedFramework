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
		public override void Cast(GlobalTargetInfo[] targets, Ability ability)
		{
			foreach (GlobalTargetInfo target in targets)
			{
				if (target.Thing is Pawn pawn && ability.pawn != pawn)
					ability.pawn.interactions?.TryInteractWith(pawn, this.interactionDef);
			}
		}

		public override bool CanApplyOn(LocalTargetInfo target, Ability ability, bool throwMessages = false)
		{
			if (target.Thing?.Map != null)
            {
				return Valid(new[] { target.ToGlobalTargetInfo(target.Thing.Map) }, ability, throwMessages);
			}
			return false;
		}

		public override bool Valid(GlobalTargetInfo[] targets, Ability ability, bool throwMessages = false)
		{
			foreach (GlobalTargetInfo target in targets)
			{
				if (target.Thing is Pawn pawn)
				{
					if (!canApplyToMentallyBroken && !AbilityUtility.ValidateNoMentalState(pawn, throwMessages, null))
					{
						return false;
					}
					if (!AbilityUtility.ValidateIsAwake(pawn, true, null))
					{
						return false;
					}
					if (!canApplyToUnconscious && !AbilityUtility.ValidateIsConscious(pawn, throwMessages, null))
					{
						return false;
					}
				}
			}
			return base.Valid(targets, ability, throwMessages);
		}
	}
}