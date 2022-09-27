using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// A work station that need resource(s) to work.
    /// </summary>
    public class Building_ResourceWorkTable : Building_WorkTable, IBillGiver, IBillGiverWithTickAction
    {
        private List<CompResourceTrader> traders = new List<CompResourceTrader>();
        private int tradersCount = 0;

        public bool CanWorkWithoutResource
        {
            get
            {
                if (traders.NullOrEmpty())
                {
                    return true;
                }
                if (def.building.unpoweredWorkTableWorkSpeedFactor > 0f)
                {
                    return true;
                }
                return false;
            }
        }

        public bool AllResourceTraderOn
        {
            get
            {
                foreach (var comp in traders)
                {
                    if (!comp.ResourceOn)
                        return false;
                }
                return true;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            traders = GetComps<CompResourceTrader>().ToList();
            tradersCount = traders.Count;
        }

        public override void UsedThisTick()
        {
            base.UsedThisTick();
            for (int i = 0; i < tradersCount; i++)
            {
                traders[i].Notify_UsedThisTick();
            }
        }

        /// <summary>
        /// Can be used if all other comp are OK and if no trader have resource off.
        /// </summary>
        public new bool CurrentlyUsableForBills() => base.CurrentlyUsableForBills() && (CanWorkWithoutResource || AllResourceTraderOn);

        /// <summary>
        /// Same thing.
        /// </summary>
        public new bool UsableForBillsAfterFueling() => base.UsableForBillsAfterFueling() && (CanWorkWithoutResource || AllResourceTraderOn);
    }
}