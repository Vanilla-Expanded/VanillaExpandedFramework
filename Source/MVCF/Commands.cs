using System.Linq;
using MVCF.Utilities;
using UnityEngine;
using Verse;

namespace MVCF
{
    public class Command_VerbTargetFixed : Command_VerbTarget
    {
        public override void ProcessInput(Event ev)
        {
            var man = verb.CasterPawn.Manager();
            if (man != null)
            {
                man.CurrentVerb = verb;
                man.ManagedVerbs.First(v => v.Verb == verb).Enabled = true;
            }

            base.ProcessInput(ev);
        }

        public override void MergeWith(Gizmo other)
        {
        }
    }

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