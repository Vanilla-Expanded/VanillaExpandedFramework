using System.Collections.Generic;
using Verse;

namespace VEF.Things;

public static class CustomFireThingGizmos
{
    public static IEnumerable<Gizmo> GetGizmos(Pawn pawn, IEnumerable<Gizmo> gizmos)
    {
        if (!DebugSettings.ShowDevGizmos)
            return gizmos;

        var attachComp = pawn.GetComp<CompAttachBase>();
        if (attachComp?.attachments == null)
            return gizmos;

        var options = new List<FloatMenuOption>();
        for (var i = 0; i < attachComp.attachments.Count; i++)
        {
            if (attachComp.attachments[i] is CustomFire fire)
            {
                options.Add(new FloatMenuOption(fire.Def.LabelCap, () =>
                {
                    if (!fire.Destroyed)
                        fire.Destroy();
                }));
            }
        }

        if (options.Count > 0)
        {
            var extinguishGizmo = new Command_Action
            {
                defaultLabel = "DEV: Extinguish specific...",
                action = () => Find.WindowStack.Add(new FloatMenu(options))
            };

            return gizmos.Concat(extinguishGizmo);
        }

        return gizmos;
    }
}