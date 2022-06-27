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
    public class JobGiver_ReturnToStationIdle : ThinkNode_JobGiver
    {
        private float maxLevelPercentage = 0.99f;
        protected override Job TryGiveJob(Pawn pawn)
        {
            var myBuilding = CompMachine.cachedMachinesPawns[pawn].myBuilding;
            var buildingPosition = myBuilding.Position;
            Need_Power power = pawn.needs.TryGetNeed<Need_Power>();
            if (power == null || power.CurLevelPercentage > maxLevelPercentage)
                return null;
            if (myBuilding != null)
            {
                if (myBuilding.Spawned && myBuilding.Map == pawn.Map && pawn.CanReserveAndReach(myBuilding, PathEndMode.OnCell, Danger.Deadly))
                {
                    if (pawn.Position != buildingPosition)
                    {
                        return JobMaker.MakeJob(JobDefOf.Goto, buildingPosition);
                    }
                    else
                    {
                        pawn.Rotation = Rot4.South;
                        if (myBuilding.TryGetComp<CompPowerTrader>().PowerOn)
                        {
                            return JobMaker.MakeJob(VFEDefOf.VFE_Mechanoids_Recharge, myBuilding);
                        }
                    }
                }
                return JobMaker.MakeJob(JobDefOf.Wait, 300);
            }
            return null;
        }
    }
}

