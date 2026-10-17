using Verse;
using Verse.AI;

namespace VEF.Things;

public class JobGiver_ExtinguishCustomFireSelf : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        var attachComp = pawn.GetComp<CompAttachBase>();
        if (attachComp?.attachments == null)
            return null;

        for (var i = 0; i < attachComp.attachments.Count; i++)
        {
            if (attachComp.attachments[i] is CustomFire fire && Rand.Chance(fire.Def.chanceToSelfExtinguish))
                return JobMaker.MakeJob(fire.Def.extinguishSelfJobDef, fire);
        }

        return null;
    }
}