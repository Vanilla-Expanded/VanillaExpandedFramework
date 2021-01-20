using MVCF.Utilities;
using Verse;

namespace MVCF
{
    public class Command_ToggleVerbUsage : Command_Toggle
    {
        public Command_ToggleVerbUsage(ManagedVerb verb)
        {
            icon = verb.Props?.ToggleIcon ?? verb.Verb.UIIcon;
            isActive = () => verb.Enabled;
            toggleAction = verb.Toggle;
            defaultLabel = PawnVerbGizmoUtility.FirstNonEmptyString(verb.Props?.toggleLabel,
                "Toggle " + verb.Verb.Label(verb.Props));
            defaultDesc = PawnVerbGizmoUtility.FirstNonEmptyString(verb.Props?.toggleDescription,
                "Toggle using " + verb.Verb.Label(verb.Props) + " automatically");
        }
    }
}