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
            defaultLabel = verb.Props?.toggleLabel ?? "Toggle " + verb.Verb.verbProps.label;
        }
    }
}