using Verse;
using Verse.AI;

namespace VEF.Things;

public class ThinkNode_ConditionalCustomFireBurning : ThinkNode_Conditional
{
    public bool mustSatisfyAllConditions = false;
    public bool requirePanicCausingFire = false;
    public bool requireCanExtinguishInWater = false;

    protected override bool Satisfied(Pawn pawn)
    {
        var attachComp = pawn.GetComp<CompAttachBase>();
        if (attachComp?.attachments == null)
            return false;

        for (var i = 0; i < attachComp.attachments.Count; i++)
        {
            if (attachComp.attachments[i] is CustomFire fire)
            {
                if (!requirePanicCausingFire || fire.CausesPanic(pawn))
                {
                    // If we require all conditions, keep going. If we're not, return true.
                    if (!mustSatisfyAllConditions)
                        return true;
                }
                // If we require all conditions, we've failed right here. Continue and check for other fires.
                else if (mustSatisfyAllConditions)
                    continue;

                if (!requireCanExtinguishInWater || fire.CanExtinguishInWater(pawn))
                {
                    if (!mustSatisfyAllConditions)
                        return true;
                }
                else if (mustSatisfyAllConditions)
                    continue;

                // If all previous conditions passed, and we require all conditions to pass, this is where we return true for passing all of them.
                if (mustSatisfyAllConditions)
                    return true;
            }
        }

        return false;
    }
}