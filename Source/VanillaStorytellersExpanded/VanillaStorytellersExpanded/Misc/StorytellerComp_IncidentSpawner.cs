using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VanillaStorytellersExpanded
{
	public class StorytellerCompProperties_IncidentSpawner : StorytellerCompProperties
	{
		public IncidentDef incident;

		public float baseIncidentsPerYear;

		public float minSpacingDays;
		public StorytellerCompProperties_IncidentSpawner()
		{
			compClass = typeof(StorytellerComp_IncidentSpawner);
		}
	}
	public class StorytellerComp_IncidentSpawner : StorytellerComp
	{
		private StorytellerCompProperties_IncidentSpawner Props => (StorytellerCompProperties_IncidentSpawner)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			int incCount = IncidentCycleUtility.IncidentCountThisInterval(target, Find.Storyteller.storytellerComps.IndexOf(this), Props.minDaysPassed, 60f, 0f, 
				Props.minSpacingDays, Props.baseIncidentsPerYear, Props.baseIncidentsPerYear);
			for (int i = 0; i < incCount; i++)
			{
				IncidentParms parms = GenerateParms(Props.incident.category, target);
				if (Props.incident.Worker.CanFireNow(parms))
				{
					yield return new FiringIncident(Props.incident, this, parms);
				}
			}
		}

		public override string ToString()
		{
			return base.ToString() + " (" + Props.incident.defName + ")";
		}
	}
}
