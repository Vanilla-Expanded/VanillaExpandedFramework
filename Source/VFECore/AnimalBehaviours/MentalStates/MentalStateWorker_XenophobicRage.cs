using System;
using Verse.AI;
using Verse;

namespace AnimalBehaviours
{
	public class MentalStateWorker_XenophobicRage : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			return base.StateCanOccur(pawn) && XenophobicRageMentalStateUtility.FindPawnToKill(pawn) != null;
		}
	}
}

