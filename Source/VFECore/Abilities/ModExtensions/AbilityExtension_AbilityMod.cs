namespace VFECore.Abilities
{
	using Mono.Unix.Native;
	using RimWorld.Planet;
	using RimWorld;
	using System.Collections.Generic;
	using System;
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
		public virtual void PreCast(LocalTargetInfo target, Ability ability, ref bool startAbilityJob)
		{

		}
		public virtual void Cast(LocalTargetInfo target, Ability ability)
		{

		}

		public virtual bool Valid(LocalTargetInfo target, Ability ability, bool throwMessages = false)
        {
			return true;
        }
		public virtual bool CanApplyOn(LocalTargetInfo target, Ability ability, bool throwMessages = false)
		{
			return true;
		}

		public virtual string ExtraLabelMouseAttachment(LocalTargetInfo target, Ability ability)
		{
			return null;
		}
	}
}