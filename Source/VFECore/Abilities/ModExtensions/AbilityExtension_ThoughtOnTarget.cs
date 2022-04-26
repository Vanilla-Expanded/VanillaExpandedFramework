namespace VFECore.Abilities
{
    using RimWorld;
    using Verse;

    public class AbilityExtension_ThoughtOnTarget : AbilityExtension_AbilityMod
	{
		public ThoughtDef thoughtDef;
		public override void Cast(LocalTargetInfo target, Ability ability)
		{
			Pawn pawn = target.Pawn;
			if (pawn != null)
			{
				pawn.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, ability.pawn);
			}
		}
	}
}