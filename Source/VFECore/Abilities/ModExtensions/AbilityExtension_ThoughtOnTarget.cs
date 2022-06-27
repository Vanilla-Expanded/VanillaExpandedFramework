namespace VFECore.Abilities
{
    using RimWorld;
    using RimWorld.Planet;
    using Verse;

    public class AbilityExtension_ThoughtOnTarget : AbilityExtension_AbilityMod
	{
		public ThoughtDef thoughtDef;

        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
			foreach (var target in targets)
            {
                Pawn pawn = target.Thing as Pawn;
                if (pawn != null)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, ability.pawn);
                }
            }
        }
	}
}