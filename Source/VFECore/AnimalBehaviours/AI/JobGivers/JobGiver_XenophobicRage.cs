using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace AnimalBehaviours
{
	public class JobGiver_XenophobicRage : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			MentalState_XenophobicRage mentalState_MurderousRage = pawn.MentalState as MentalState_XenophobicRage;
			if (mentalState_MurderousRage == null || !mentalState_MurderousRage.IsTargetStillValidAndReachable())
			{
				return null;
			}
			Thing spawnedParentOrMe = mentalState_MurderousRage.target.SpawnedParentOrMe;
			Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, spawnedParentOrMe);
			job.canBashDoors = true;
			job.canBashFences = true;

			job.killIncappedTarget = true;
			if (spawnedParentOrMe != mentalState_MurderousRage.target)
			{
				job.maxNumMeleeAttacks = 2;
			}
			return job;
		}
	}
}