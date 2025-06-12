using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Apparels
{
	public class CompProperties_ApparelHediffs : CompProperties
	{
		public List<string> hediffDefnames;
		public CompProperties_ApparelHediffs()
		{
			this.compClass = typeof(CompApparelHediffs);
		}
	}
}

