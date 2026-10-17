using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VEF.Things;

public class FloatMenuOptionProvider_ExtinguishCustomFires : FloatMenuOptionProvider
{
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => true;
    protected override bool RequiresManipulation => true;

    public override IEnumerable<FloatMenuOption> GetOptions(FloatMenuContext context)
    {
        var targetCell = new TargetInfo(context.ClickedCell, context.map);
        // Check if there's any custom fire in the cell/on the building/attached to anything in cell
        if (!targetCell.IsBurningCustomFire())
            yield break;

        for (var i = 0; i < DefDatabase<CustomFireDef>.AllDefsListForReading.Count; i++)
        {
            var def = DefDatabase<CustomFireDef>.AllDefsListForReading[i];
            // Check if there's this specific fire in the cell
            if (!targetCell.IsBurningCustomFire(def))
                continue;

            var report = AcceptanceReport.WasRejected;
            foreach (var pawn in context.ValidSelectedPawns)
            {
                report = def.Worker.PawnCanExtinguish(pawn, context.ClickedCell);
                if (report.Accepted)
                    break;
            }

            var floatMenuOption = def.Worker.GetExtinguishFloatMenuOption(context, report);
            if (floatMenuOption != null)
                yield return floatMenuOption;
        }
    }

    public static IEnumerable<CustomFire> GetRelevantCustomFires(TargetInfo t)
    {
        if (t.Thing is Pawn { Destroyed: false, Spawned: true } pawn)
        {
            var attachComp = pawn.GetComp<CompAttachBase>();
            if (attachComp?.attachments != null)
            {
                for (var i = 0; i < attachComp.attachments.Count; i++)
                {
                    if (attachComp.attachments[i] is CustomFire fire)
                        yield return fire;
                }
            }
        }

        var list = t.Map.thingGrid.ThingsListAt(t.Cell);

        for (var i = 0; i < list.Count; i++)
        {
            if (list[i] is CustomFire { parent: null } fire)
                yield return fire;
        }
    }
}