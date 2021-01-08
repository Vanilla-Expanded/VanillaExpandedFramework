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
            
            var buildingPosition = CompMachine.cachedMachinesPawns[pawn].myBuilding.Position;
            if (pawn.Position != buildingPosition)
            {
                return JobMaker.MakeJob(JobDefOf.Goto, buildingPosition);
            }
            else
            {
                pawn.Rotation = Rot4.South;
                Need_Power power = pawn.needs.TryGetNeed<Need_Power>();
                if (power == null || power.CurLevelPercentage > maxLevelPercentage)
                    return null;
                var building = CompMachine.cachedMachinesPawns[pawn].myBuilding;
                if (building.TryGetComp<CompPowerTrader>().PowerOn)
                {
                    return JobMaker.MakeJob(VFEDefOf.VFE_Mechanoids_Recharge, building);
                }
                else
                {
                    return JobMaker.MakeJob(JobDefOf.Wait, 300);
                }
            }
        }
    }
}

