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
        public HashSet<Lord> raidGroups;

        public RaidGroup()
        {
            pawns = new HashSet<Pawn>();
            raidGroups = new HashSet<Lord>();
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
            Scribe_Collections.Look(ref raidGroups, "raidGroups", LookMode.Reference);
        }
    }
}
