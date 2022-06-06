using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VFE.Mechanoids;
using VFE.Mechanoids.Needs;
using VFECore;
using VFEMech;

namespace VFE.Mechanoids.AI.JobGivers
{
    public class JobGiver_ReturnToStation : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            return 8f;
        }

        private static Dictionary<Pawn, int> pawnsWithLastJobScanTick = new Dictionary<Pawn, int>();
        protected override Job TryGiveJob(Pawn pawn)
        {
            var myBuilding = CompMachine.cachedMachinesPawns[pawn].myBuilding;
            if (myBuilding != null)
            {
                if (myBuilding.Spawned && myBuilding.Map == pawn.Map)
                {
                    var compMachine = CompMachine.cachedMachinesPawns[pawn];
                    var compMachineChargingStation = compMachine.myBuilding.TryGetComp<CompMachineChargingStation>();
                    if (compMachineChargingStation.wantsRest && compMachine.myBuilding.TryGetComp<CompPowerTrader>().PowerOn 
                        && pawn.CanReach(myBuilding, PathEndMode.OnCell, Danger.Deadly))
                        return JobMaker.MakeJob(VFEDefOf.VFE_Mechanoids_Recharge, compMachine.myBuilding);

                    if (pawn.mindState.lastJobTag == JobTag.Idle)
                    {
                        if (pawnsWithLastJobScanTick.ContainsKey(pawn))
                        {
                            if (Find.TickManager.TicksGame - pawnsWithLastJobScanTick[pawn] < 60)
                            {
                                pawnsWithLastJobScanTick[pawn] = Find.TickManager.TicksGame;
                                return JobMaker.MakeJob(JobDefOf.Wait, 300);
                            }
                        }
                        pawnsWithLastJobScanTick[pawn] = Find.TickManager.TicksGame;
                    }
                }
            }
            return null;
        }
    }
}

