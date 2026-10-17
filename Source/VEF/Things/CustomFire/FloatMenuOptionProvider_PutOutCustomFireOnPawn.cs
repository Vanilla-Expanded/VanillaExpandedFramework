using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VEF.Things;

public class FloatMenuOptionProvider_PutOutCustomFireOnPawn : FloatMenuOptionProvider
{
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
    {
        if (clickedPawn == null || clickedPawn.Dead || !clickedPawn.IsBurningCustomFire())
            yield break;

        if ((clickedPawn.Faction == null || clickedPawn.Faction != context.FirstSelectedPawn.Faction) && (clickedPawn.HostFaction == null || (clickedPawn.HostFaction != context.FirstSelectedPawn.Faction && clickedPawn.HostFaction != context.FirstSelectedPawn.HostFaction)))
            yield break;

        if (!context.FirstSelectedPawn.CanReach(clickedPawn, PathEndMode.Touch, Danger.Deadly))
        {
            yield return new FloatMenuOption($"{"CannotGenericWorkCustom".Translate(WorkGiverDefOf.FightFires.label)}: {"NoPath".Translate().CapitalizeFirst()}", null);
            yield break;
        }

        // No need to null check, it's done by IsBurningCustomFire.
        var comp = clickedPawn.GetComp<CompAttachBase>();
        for (var i = 0; i < comp.attachments.Count; i++)
        {
            if (comp.attachments[i] is CustomFire fire)
            {
                var option = fire.Def.Worker.GetExtinguishPawnFloatMenuOption(clickedPawn, context, fire);
                if (option != null)
                    yield return option;
            }
        }
    }
}