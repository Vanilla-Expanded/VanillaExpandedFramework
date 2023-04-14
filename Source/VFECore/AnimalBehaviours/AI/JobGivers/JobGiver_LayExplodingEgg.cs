using System;
using Verse;
using Verse.AI;
using RimWorld;


namespace AnimalBehaviours
{
	public class JobGiver_LayExplodingEgg : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			CompExplodingEggLayer compEggLayer = pawn.TryGetComp<CompExplodingEggLayer>();
			if (compEggLayer == null || !compEggLayer.CanLayNow)
			{
				return null;
			}
			IntVec3 c = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 5f, null, Danger.Some);
			return JobMaker.MakeJob(InternalDefOf.VEF_LayExplodingEgg, c);
		}

		private const float LayRadius = 5f;
	}
}
