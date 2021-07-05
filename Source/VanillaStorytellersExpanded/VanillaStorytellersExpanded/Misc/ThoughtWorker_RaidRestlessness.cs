using RimWorld;
using Verse;

namespace VanillaStorytellersExpanded
{
	public class ThoughtWorker_RaidRestlessness : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.Faction == Faction.OfPlayer)
            {
				var options = Find.Storyteller.def.GetModExtension<StorytellerDefExtension>();
				if (options != null && options.raidRestlessness != null)
				{
					var stageIndex = options.raidRestlessness.GetThoughtState();
					if (stageIndex == -1)
					{
						return ThoughtState.Inactive;
					}
					if (stageIndex > this.def.stages.Count - 1)
					{
						return ThoughtState.ActiveAtStage(this.def.stages.Count - 1);
					}
					return ThoughtState.ActiveAtStage(stageIndex);
				}
			}
			return ThoughtState.Inactive;
		}
	}
}
