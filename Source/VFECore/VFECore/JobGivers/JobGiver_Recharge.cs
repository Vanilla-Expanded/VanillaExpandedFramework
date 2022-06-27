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
    public class JobGiver_Recharge : ThinkNode_JobGiver
    {

        private RestCategory minCategory = RestCategory.VeryTired;

        private float maxLevelPercentage = 0.99f;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_Recharge obj = (JobGiver_Recharge)base.DeepCopy(resolve);
            obj.minCategory = minCategory;
            obj.maxLevelPercentage = maxLevelPercentage;
            return obj;
        }

        public override float GetPriority(Pawn pawn)
        {
            Need_Power power = pawn.needs.TryGetNeed<Need_Power>();
            if (power == null)
            {
                return 0f;
            }
            if ((int)power.CurCategory < (int)minCategory)
            {
                return 0f;
            }
            if (power.CurLevelPercentage > maxLevelPercentage)
            {
                return 0f;
            }
            return 8f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Need_Power power = pawn.needs.TryGetNeed<Need_Power>();
            if (power == null || power.CurLevelPercentage > maxLevelPercentage)
                return null;
            if (pawn.CurJobDef != VFEDefOf.VFE_Mechanoids_Recharge && power.CurCategory <= minCategory)
            {
                return null;
            }
            var building = CompMachine.cachedMachinesPawns[pawn].myBuilding;
            if (building != null)
            {
                if (building.Spawned && building.Map == pawn.Map && pawn.CanReserveAndReach(building, PathEndMode.OnCell, Danger.Deadly))
                {
                    if (building.TryGetComp<CompPowerTrader>().PowerOn)
                    {
                        return JobMaker.MakeJob(VFEDefOf.VFE_Mechanoids_Recharge, building);
                    }
                    else if (building.Position != pawn.Position)
                    {
                        return JobMaker.MakeJob(JobDefOf.Goto, building);
                    }
                }
            }
            return null;
        }
    }
}

