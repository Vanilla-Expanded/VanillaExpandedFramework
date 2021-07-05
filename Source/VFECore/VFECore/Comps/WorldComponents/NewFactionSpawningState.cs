using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VFECore
{
    public class NewFactionSpawningState : WorldComponent
    {
        internal List<FactionDef> ignoredFactions = new List<FactionDef>();

        public NewFactionSpawningState(World world) : base(world) { }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref ignoredFactions, "ignoredFactions", LookMode.Def);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (UIUtilityData.factionCounts != null)
            {
                foreach (var item in UIUtilityData.factionCounts)
                {
                    if (item.Value <= 0) Ignore(item.Key); 
                }
            }
            UIUtilityData.factionCounts.Clear();
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
