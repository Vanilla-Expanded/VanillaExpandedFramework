using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Check for all compResourceTrader status before pushing heat.
    /// </summary>
    public class CompHeatPusherNeedResource : CompHeatPusherPowered
    {
        private List<CompResourceTrader> compResourceTraders;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            compResourceTraders = parent.GetComps<CompResourceTrader>().ToList();
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override bool ShouldPushHeatNow
        {
            get
            {
                for (int i = 0; i < compResourceTraders.Count; i++)
                {
                    if (!compResourceTraders[i].ResourceOn)
                        return false;
                }
                return base.ShouldPushHeatNow;
            }
        }
    }
}