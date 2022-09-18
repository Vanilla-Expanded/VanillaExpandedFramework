using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VFECore
{
    public class NewFactionSpawningState : WorldComponent
    {
        private HashSet<FactionDef> ignoredFactions = new HashSet<FactionDef>();

        public NewFactionSpawningState(World world) : base(world) { }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref ignoredFactions, "ignoredFactions", LookMode.Def);
        }
        public void Ignore(FactionDef faction)
        {
            ignoredFactions.Add(faction);
        }

        public void Ignore(IEnumerable<FactionDef> factions)
        {
            ignoredFactions.AddRange(factions);
        }
        public void ClearIgnored()
        {
            ignoredFactions.Clear();
        }

        public bool IsIgnored(FactionDef faction)
        {
            return ignoredFactions.Contains(faction);
        }
    }
}
