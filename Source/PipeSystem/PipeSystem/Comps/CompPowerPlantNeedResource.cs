using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace PipeSystem
{
    /// <summary>
    /// Check for all compResourceTrader status before pushing heat.
    /// </summary>
    public class CompPowerPlantNeedResource : CompPowerPlant
    {
        private List<CompResourceTrader> compResourceTraders;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            compResourceTraders = parent.GetComps<CompResourceTrader>().ToList();
            base.PostSpawnSetup(respawningAfterLoad);
        }

        protected override float DesiredPowerOutput
        {
            get
            {
                for (int i = 0; i < compResourceTraders.Count; i++)
                {
                    if (!compResourceTraders[i].ResourceOn)
                        return 0f;
                }
                return base.DesiredPowerOutput;
            }
        }
    }
}