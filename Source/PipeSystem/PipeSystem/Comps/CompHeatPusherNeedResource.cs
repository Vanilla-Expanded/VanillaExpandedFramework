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
        private IEnumerable<CompResourceTrader> compResourceTraders;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            compResourceTraders = parent.GetComps<CompResourceTrader>();
            base.PostSpawnSetup(respawningAfterLoad);
        }

        protected override bool ShouldPushHeatNow
        {
            get
            {
                for (int i = 0; i < compResourceTraders.Count(); i++)
                {
                    var comp = compResourceTraders.ElementAt(i);
                    if (!comp.ResourceOn)
                        return false;
                }
                return base.ShouldPushHeatNow;
            }
        }
    }
}