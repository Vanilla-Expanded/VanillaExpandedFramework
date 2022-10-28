using MVCF.Features;
using MVCF.Utilities;
using Verse;
using Verse.AI;

namespace Reloading;

public class JobGiver_SwitchWeapon : ThinkNode_JobGiver
{
    public override float GetPriority(Pawn pawn) => 99f;

    protected override Job TryGiveJob(Pawn pawn)
    {
        if (MVCF.MVCF.GetFeature<Feature_Reloading>().Enabled) TrySwitchWeapon(pawn);
        return null;
    }

    public static void TrySwitchWeapon(Pawn pawn)
    {
        if (pawn.equipment?.PrimaryEq?.PrimaryVerb?.GetReloadable() is { ShotsRemaining: 0, NewWeapon: ThingWithComps newWeapon })
        {
            if (pawn.equipment.Primary is { } oldWeapon)
                pawn.inventory.innerContainer.TryAddOrTransfer(oldWeapon, false);
            if (!pawn.IsColonist && pawn.equipment.Primary is { } eq && pawn.equipment.Contains(eq) &&
                !pawn.equipment.TryDropEquipment(eq, out var result, pawn.Position))
                Log.Warning("[MVCF] Failed to drop " + result);
            pawn.inventory.innerContainer.TryTransferToContainer(newWeapon, pawn.equipment.GetDirectlyHeldThings(), 1, false);
        }
    }
}