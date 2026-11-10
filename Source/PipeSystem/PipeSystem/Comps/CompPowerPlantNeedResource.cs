using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Check for all compResourceTrader status before pushing heat.
    /// </summary>
    public class CompPowerPlantNeedResource : CompPowerPlant
    {
        private List<CompResourceTrader> compResourceTraders;

        public override void PostPostMake()
        {
            base.PostPostMake();
            InitComps();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                InitComps();
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

        private void InitComps() => compResourceTraders = [.. parent.GetComps<CompResourceTrader>()];
    }
}