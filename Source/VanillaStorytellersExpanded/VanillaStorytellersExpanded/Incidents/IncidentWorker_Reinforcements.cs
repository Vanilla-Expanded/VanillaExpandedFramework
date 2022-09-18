using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VanillaStorytellersExpanded
{
	public class IncidentWorker_Reinforcements : IncidentWorker_RaidEnemy
	{
		protected override string GetLetterLabel(IncidentParms parms)
		{
			return "VSE.Reinforcements".Translate() + ": " + parms.faction.Name;
		}

		protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
		{
			string str = "VSE.ReinforcementsDesc".Translate(parms.faction.Named("FACTION"));
			str += "\n\n";
			str += parms.raidStrategy.arrivalTextEnemy;
			Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
			if (pawn != null)
			{
				str += "\n\n";
				str += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
			}
			return str;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			ResolveRaidPoints(parms);
			if (!TryResolveRaidFaction(parms))
			{
				return false;
			}
			PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
			ResolveRaidStrategy(parms, combat);
			ResolveRaidArriveMode(parms);
			parms.raidStrategy.Worker.TryGenerateThreats(parms);
			if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
			{
				return false;
			}
			float points = parms.points;
			parms.points = AdjustedRaidPoints(parms.points, parms.raidArrivalMode, parms.raidStrategy, parms.faction, combat);
			RaidPatches.includeRaidToTheList = false;
			List<Pawn> list = parms.raidStrategy.Worker.SpawnThreats(parms);
			if (list == null)
			{
				list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms)).ToList();
				if (list.Count == 0)
				{
					Log.Error("Got no pawns spawning raid from parms " + parms);
					return false;
				}
				parms.raidArrivalMode.Worker.Arrive(list, parms);
			}
			RaidPatches.includeRaidToTheList = true;

			GenerateRaidLoot(parms, points, list);
			TaggedString letterLabel = GetLetterLabel(parms);
			TaggedString letterText = GetLetterText(parms, list);
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref letterLabel, ref letterText, GetRelatedPawnsInfoLetterText(parms), informEvenIfSeenBefore: true);
			List<TargetInfo> list2 = new List<TargetInfo>();
			if (parms.pawnGroups != null)
			{
				List<List<Pawn>> list3 = IncidentParmsUtility.SplitIntoGroups(list, parms.pawnGroups);
				List<Pawn> list4 = list3.MaxBy((List<Pawn> x) => x.Count);
				if (list4.Any())
				{
					list2.Add(list4[0]);
				}
				for (int i = 0; i < list3.Count; i++)
				{
					if (list3[i] != list4 && list3[i].Any())
					{
						list2.Add(list3[i][0]);
					}
				}
			}
			else if (list.Any())
			{
				foreach (Pawn item in list)
				{
					list2.Add(item);
				}
			}
			SendStandardLetter(letterLabel, letterText, GetLetterDef(), parms, list2);
			parms.raidStrategy.Worker.MakeLords(parms, list);
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
			if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts))
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].apparel?.WornApparel.Any((Apparel ap) => ap.GetComp<CompShield>() != null) ?? false)
					{
						LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);
						break;
					}
				}
			}
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			Find.StoryWatcher.statsRecord.numRaidsEnemy++;
			return true;
		}
	}
}
