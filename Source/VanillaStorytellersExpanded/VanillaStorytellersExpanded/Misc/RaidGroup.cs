using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace VanillaStorytellersExpanded
{
	public class RaidGroup : IExposable
	{
        public HashSet<Pawn> pawns;
        public HashSet<Lord> lords;
        public Faction faction;

        public RaidGroup()
        {
            pawns = new HashSet<Pawn>();
            lords = new HashSet<Lord>();
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
            Scribe_Collections.Look(ref lords, "lords", LookMode.Reference);
            Scribe_References.Look(ref faction, "faction");
        }
    }
}
