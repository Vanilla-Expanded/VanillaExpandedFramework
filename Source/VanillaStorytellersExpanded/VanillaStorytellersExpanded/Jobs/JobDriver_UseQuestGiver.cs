using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace VanillaStorytellersExpanded
{
	public class JobDriver_UseQuestGiver : JobDriver
	{

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
			Toil openComms = new Toil();
			openComms.initAction = delegate
			{
				Pawn actor = openComms.actor;
				actor.CurJob.targetA.Thing.TryGetComp<CompQuestGiver>().Use();
			};
			yield return openComms;
		}
	}
}
