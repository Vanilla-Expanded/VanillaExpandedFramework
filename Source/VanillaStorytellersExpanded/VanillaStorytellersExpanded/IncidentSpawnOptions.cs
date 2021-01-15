using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace VanillaStorytellersExpanded
{
	public class IncidentSpawnOptions
	{
		public bool alliesReduceThreats;
		public bool alliesIncreaseGoodIncidents;

		public bool enemiesReduceThreats;
		public bool enemiesIncreaseGoodIncidents;

		public List<string> goodIncidents = new List<string>();
		public List<string> negativeIncidents = new List<string>();
		public List<string> neutralIncidents = new List<string>();
	}
}
