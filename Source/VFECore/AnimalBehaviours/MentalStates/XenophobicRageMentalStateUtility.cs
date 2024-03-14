using System;
using System.Collections.Generic;
using Verse.AI;
using Verse;

namespace AnimalBehaviours
{
	public static class XenophobicRageMentalStateUtility
	{
		public static Pawn FindPawnToKill(Pawn pawn)
		{
			if (!pawn.Spawned)
			{
				return null;
			}
			XenophobicRageMentalStateUtility.tmpTargets.Clear();
			CompExtremeXenophobia comp = pawn.TryGetComp<CompExtremeXenophobia>();
		
			if (comp == null)
			{
				return null;
			}
			IReadOnlyList<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				
				Pawn pawn2 = allPawnsSpawned[i];
				if ((pawn2.Faction == pawn.Faction)&& !comp.Props.AcceptedDefnames.Contains(pawn2.def.defName) && pawn2.RaceProps.Humanlike && pawn2 != pawn && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly, false, false,TraverseMode.ByPawn) && (pawn2.CurJob == null || !pawn2.CurJob.exitMapOnArrival))
				{
					
					XenophobicRageMentalStateUtility.tmpTargets.Add(pawn2);
				}
			}
			if (!XenophobicRageMentalStateUtility.tmpTargets.Any<Pawn>())
			{
				return null;
			}
			Pawn result = XenophobicRageMentalStateUtility.tmpTargets.RandomElement<Pawn>();
			XenophobicRageMentalStateUtility.tmpTargets.Clear();
			return result;
		}

		private static List<Pawn> tmpTargets = new List<Pawn>();
	}
}
