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
    public class JobGiver_StandIdle : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            return JobMaker.MakeJob(JobDefOf.Wait, 60);
        }
    }
}

