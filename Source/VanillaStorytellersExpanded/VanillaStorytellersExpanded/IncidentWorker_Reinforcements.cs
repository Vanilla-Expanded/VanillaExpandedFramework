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
	}
}
