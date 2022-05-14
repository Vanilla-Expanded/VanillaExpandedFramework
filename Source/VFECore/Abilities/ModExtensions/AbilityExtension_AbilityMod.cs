namespace VFECore.Abilities
{
	using RimWorld.Planet;
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

		[Obsolete("Use new method with GlobalTargetInfos instead")]
		public virtual void PreWarmupAction(LocalTargetInfo target, Ability ability) =>
			this.PreWarmupAction(new[] { target.ToGlobalTargetInfo(ability.Caster.Map) }, ability);

		public virtual void PreWarmupAction(GlobalTargetInfo[] targets, Ability ability)
		{
		}

		public virtual void WarmupToil(Toil toil)
		{

		}

		[Obsolete("Use new method using GlobalTargetInfo instead")]
		public virtual void PreCast(LocalTargetInfo target, Ability ability, ref bool startAbilityJobImmediately, Action startJobAction) =>
			this.PreCast(new[] { target.ToGlobalTargetInfo(ability.pawn.Map) }, ability, ref startAbilityJobImmediately, startJobAction);

		public virtual void PreCast(GlobalTargetInfo[] target, Ability ability, ref bool startAbilityJobImmediately, Action startJobAction)
		{
		}

		[Obsolete("Use the new Cast method using GlobalTargets instead")]
		public virtual void Cast(LocalTargetInfo target, Ability ability)
		{
			this.Cast(new[] { target.ToGlobalTargetInfo(ability.Caster.Map) }, ability);
		}

		public virtual void Cast(GlobalTargetInfo[] targets, Ability ability)
		{
		}

		[Obsolete("Use new method using GlobalTargetInfo instead")]
		public virtual bool Valid(LocalTargetInfo target, Ability ability, bool throwMessages = false) =>
			this.Valid(new[] { target.ToGlobalTargetInfo(ability.Caster.Map) }, ability, throwMessages);

		public virtual bool Valid(GlobalTargetInfo[] targets, Ability ability, bool throwMessages = false)
		{
			return true;
		}

		public virtual bool ValidTile(GlobalTargetInfo target, Ability ability, bool throwMessages = false)
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

		public virtual void GizmoUpdateOnMouseover(Ability ability)
		{
		}
	}
}