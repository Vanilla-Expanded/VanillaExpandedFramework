using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VFECore
{
    public class NewFactionLoadingState : WorldComponent
    {
        private List<FactionDef> ignoredFactions = new List<FactionDef>();

        public NewFactionLoadingState(World world) : base(world) { }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref ignoredFactions, "ignoredFactions", LookMode.Def);
        }

        public override void FinalizeInit()
        {
            Log.Message($"Hello from FactionLoadingState! {ignoredFactions.Count} factions are ignored.");
            base.FinalizeInit();
        }

        public void Ignore(FactionDef faction)
        {
            if (!ignoredFactions.Contains(faction)) ignoredFactions.Add(faction);
        }

        public bool IsIgnored(FactionDef faction)
        {
            return ignoredFactions.Contains(faction);
        }
    }
}
