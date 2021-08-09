using NocturnalAnimals;
using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_GetRest2 : ThinkNode_JobGiver
	{
		private RestCategory minCategory;

		private float maxLevelPercentage = 1f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GetRest2 obj = (JobGiver_GetRest2)base.DeepCopy(resolve);
			obj.minCategory = minCategory;
			obj.maxLevelPercentage = maxLevelPercentage;
			return obj;
		}

		public static bool ShouldSleep(Pawn pawn)
		{
			int hour = GenLocalDate.HourOfDay(pawn);
			pawn.jobs.debugLog = true;
			ExtendedRaceProperties extendedRaceProps = pawn.def.GetModExtension<ExtendedRaceProperties>();
			if (extendedRaceProps != null && extendedRaceProps.bodyClock == BodyClock.Crepuscular)
			{
				return hour > 3 && hour < 16;
			}
			else if (extendedRaceProps != null && extendedRaceProps.bodyClock == BodyClock.Nocturnal)
			{
				return hour > 9 && hour < 19;
			}
			return hour >= 7 && hour <= 21;
		}
		public override float GetPriority(Pawn pawn)
		{
			Need_Rest rest = pawn.needs.rest;
			if (rest == null)
			{
				return 0f;
			}
			if ((int)rest.CurCategory < (int)minCategory)
			{
				return 0f;
			}
			if (rest.CurLevelPercentage > maxLevelPercentage)
			{
				return 0f;
			}
			if (Find.TickManager.TicksGame < pawn.mindState.canSleepTick)
			{
				return 0f;
			}
			Lord lord = pawn.GetLord();
			if (lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds)
			{
				return 0f;
			}
			TimeAssignmentDef timeAssignmentDef;
			if (pawn.RaceProps.Humanlike)
			{
				timeAssignmentDef = ((pawn.timetable == null) ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment);
			}
			else
			{
				timeAssignmentDef = (ShouldSleep(pawn) ? TimeAssignmentDefOf.Anything : TimeAssignmentDefOf.Sleep);
			}
			float curLevel = rest.CurLevel;
			if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
			{
				if (curLevel < 0.3f)
				{
					return 8f;
				}
				return 0f;
			}
			if (timeAssignmentDef == TimeAssignmentDefOf.Work)
			{
				return 0f;
			}
			if (timeAssignmentDef == TimeAssignmentDefOf.Meditate)
			{
				if (curLevel < 0.16f)
				{
					return 8f;
				}
				return 0f;
			}
			if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
			{
				if (curLevel < 0.3f)
				{
					return 8f;
				}
				return 0f;
			}
			if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
			{
				if (curLevel < RestUtility.FallAsleepMaxLevel(pawn))
				{
					return 8f;
				}
				return 0f;
			}
			throw new NotImplementedException();
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Need_Rest rest = pawn.needs.rest;
			if (rest == null || (int)rest.CurCategory < (int)minCategory || rest.CurLevelPercentage > maxLevelPercentage)
			{
				return null;
			}
			if (RestUtility.DisturbancePreventsLyingDown(pawn))
			{
				return null;
			}
			Lord lord = pawn.GetLord();
			Building_Bed building_Bed = (((lord == null || lord.CurLordToil == null || lord.CurLordToil.AllowRestingInBed) && !pawn.IsWildMan() && (!pawn.InMentalState || pawn.MentalState.AllowRestingInBed)) ? RestUtility.FindBedFor(pawn) : null);
			if (building_Bed != null)
			{
				return JobMaker.MakeJob(JobDefOf.LayDown, building_Bed);
			}
			return JobMaker.MakeJob(JobDefOf.LayDown, FindGroundSleepSpotFor(pawn));
		}

		private IntVec3 FindGroundSleepSpotFor(Pawn pawn)
		{
			Map map = pawn.Map;
			for (int i = 0; i < 2; i++)
			{
				int radius = ((i == 0) ? 4 : 12);
				if (CellFinder.TryRandomClosewalkCellNear(pawn.Position, map, radius, out var result, (IntVec3 x) => !x.IsForbidden(pawn) && !x.GetTerrain(map).avoidWander))
				{
					return result;
				}
			}
			return CellFinder.RandomClosewalkCellNearNotForbidden(pawn, 4);
		}
	}
}
