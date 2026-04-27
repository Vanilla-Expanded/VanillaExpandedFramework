using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Weapons;

public class CompMultiVerbWeapon : ThingComp
{
    protected Verb activeVerb = null;
    protected CompProperties_MultiVerbWeapon.VerbData activeVerbData = null;
    protected CompEquippable equippable;

    public virtual Verb ActiveVerb
    {
        get
        {
            if (!VerbValid)
                InitActiveVerb();
            return activeVerb;
        }
        set
        {
            var data = Props.verbs.FirstOrDefault(x => x.verbLabel == value.verbProps.untranslatedLabel);
            if (data != null)
            {
                activeVerb = value;
                activeVerbData = data;
            }
            else Log.Error($"[VGE] {parent} is trying to set an active verb for {nameof(CompMultiVerbWeapon)}, but its props has no data for such verb.");
        }
    }

    public virtual CompProperties_MultiVerbWeapon.VerbData ActiveVerbData
    {
        get
        {
            if (!VerbValid)
                InitActiveVerb();
            return activeVerbData;
        }
    }

    protected virtual bool VerbValid
    {
        get
        {
            // If verb, data, equippable aren't null and equippable contains the verb, we're good.
            if (activeVerb != null && activeVerbData != null && equippable != null && equippable.AllVerbs.Contains(activeVerb))
                return true;

            // Something is not matching, will need to reinitialize the verb.
            activeVerb = null;
            activeVerbData = null;
            return false;
        }
    }

    public CompProperties_MultiVerbWeapon Props => (CompProperties_MultiVerbWeapon)props;

    public override float GetStatOffset(StatDef stat)
    {
        if (ActiveVerbData == null)
            return 0f;
        return ActiveVerbData.statOffsets.GetStatOffsetFromList(stat);
    }

    public override float GetStatFactor(StatDef stat)
    {
        if (ActiveVerbData == null)
            return 0f;
        return ActiveVerbData.statFactors.GetStatFactorFromList(stat);
    }

    public override void GetStatsExplanation(StatDef stat, StringBuilder sb, string whitespace = "")
    {
        if (ActiveVerbData == null)
            return;

        var offset = ActiveVerbData.statOffsets.GetStatOffsetFromList(stat);
        if (!Mathf.Approximately(offset, 0f))
            sb.AppendLine(StatModifierText(offset, ToStringNumberSense.Offset));
        var factor = ActiveVerbData.statFactors.GetStatFactorFromList(stat);
        if (!Mathf.Approximately(factor, 1f))
            sb.AppendLine(StatModifierText(factor, ToStringNumberSense.Factor));

        string StatModifierText(float value, ToStringNumberSense numberSense)
        {
            var label = ActiveVerbData.statExplanationLabelOverride.NullOrEmpty() ? Props.statExplanationLabel : ActiveVerbData.statExplanationLabelOverride;
            return $"{whitespace}{label}: {stat.Worker.ValueToString(value, false, numberSense)}";
        }
    }

    public override void PostPostMake()
    {
        base.PostPostMake();
        InitComps();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref activeVerb, nameof(activeVerb));
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            InitComps();
        }
        else if (Scribe.mode == LoadSaveMode.PostLoadInit && activeVerb != null)
        {
            activeVerbData = Props.verbs.FirstOrDefault(x => x.verbLabel == activeVerb.verbProps.untranslatedLabel);
            if (activeVerbData == null)
                activeVerb = null;
        }
    }

    public virtual IEnumerable<Command> CompGetSwitchModeGizmo()
    {
        if (equippable == null)
            yield break;

        switch (Props.switchMode)
        {
            case CompProperties_MultiVerbWeapon.SwitchMode.FloatMenuGizmo:
            {
                // Check if there's any verb (besides the current one) that we support
                if (equippable.AllVerbs.Any(v => v != ActiveVerb && Props.verbs.Any(d => d.verbLabel == v.verbProps.untranslatedLabel)))
                {
                    yield return new Command_Action
                    {
                        defaultLabel = Props.gizmoLabel,
                        defaultDesc = Props.gizmoDescription,
                        icon = Props.gizmoIcon,
                        action = () =>
                        {
                            var options = new List<FloatMenuOption>();
                            for (var i = 0; i < Props.verbs.Count; i++)
                            {
                                var verbData = Props.verbs[i];
                                // Don't display active mode
                                if (verbData == activeVerbData)
                                    continue;

                                var verb = equippable.AllVerbs.FirstOrDefault(x => x.verbProps.untranslatedLabel == verbData.verbLabel);
                                // Don't display options for verbs that aren't matching
                                if (verb == null)
                                    continue;

                                options.Add(new FloatMenuOption(verbData.gizmoLabelOverride ?? verb.verbProps.label, () => ActiveVerb = verb, verbData.gizmoIconOverride, Color.white));
                            }

                            if (options.Count == 0)
                                Log.Error("[VGE] CompMultiVerbWeapon doesn't have any supported verbs");
                            else
                                Find.WindowStack.Add(new FloatMenu(options));
                        }
                    };
                }

                break;
            }
            case CompProperties_MultiVerbWeapon.SwitchMode.DoubleVerbToggle:
            case CompProperties_MultiVerbWeapon.SwitchMode.DoubleVerbToggleMirrored:
            {
                // Grab the default verb
                var defaultVerb = ActiveVerbData.verbLabel == Props.defaultVerbLabel ? ActiveVerb : equippable.AllVerbs.FirstOrDefault(v => v.verbProps.untranslatedLabel == Props.defaultVerbLabel);
                // Grab the first non-default verb
                var alternativeVerb = ActiveVerbData.verbLabel != Props.defaultVerbLabel ? ActiveVerb : equippable.AllVerbs.FirstOrDefault(v => v.verbProps.untranslatedLabel != Props.defaultVerbLabel && Props.verbs.Any(d => d.verbLabel == v.verbProps.untranslatedLabel));

                if (defaultVerb != null && alternativeVerb != null)
                {
                    var verbToSwitchTo = (ActiveVerb == defaultVerb ? alternativeVerb : defaultVerb);
                    var newData = Props.verbs.FirstOrDefault(d => d.verbLabel == verbToSwitchTo.verbProps.untranslatedLabel);
                    if (newData != null)
                    {
                        yield return new Command_Toggle
                        {
                            defaultLabel = newData.gizmoLabelOverride ?? Props.gizmoLabel,
                            defaultDesc = newData.gizmoDescriptionOverride ?? Props.gizmoDescription,
                            icon = ActiveVerbData.gizmoIconOverride ?? Props.gizmoIcon,
                            toggleAction = () => ActiveVerb = verbToSwitchTo,
                            isActive = () => Props.switchMode == CompProperties_MultiVerbWeapon.SwitchMode.DoubleVerbToggle ? ActiveVerb.verbProps.untranslatedLabel == Props.defaultVerbLabel : ActiveVerb.verbProps.untranslatedLabel != Props.defaultVerbLabel
                        };
                    }
                }

                break;
            }
            case CompProperties_MultiVerbWeapon.SwitchMode.MultiSwitchGizmo:
            {
                for (var i = 0; i < Props.verbs.Count; i++)
                {
                    var data = Props.verbs[i];
                    // Don't display active
                    if (data == ActiveVerbData)
                        continue;

                    var verb = equippable.AllVerbs.FirstOrDefault(v => v.verbProps.untranslatedLabel == data.verbLabel);
                    if (verb == null)
                        continue;

                    yield return new Command_Action
                    {
                        defaultLabel = data.gizmoLabelOverride ?? Props.gizmoLabel,
                        defaultDesc = data.gizmoDescriptionOverride ?? Props.gizmoDescription,
                        icon = data.gizmoIconOverride ?? Props.gizmoIcon,
                        action = () => ActiveVerb = verb
                    };
                }

                break;
            }
            case CompProperties_MultiVerbWeapon.SwitchMode.SingleSwitchGizmo:
            default:
            {
                var index = Props.verbs.IndexOf(ActiveVerbData);
                for (var i = 1; i < Props.verbs.Count; i++)
                {
                    // Go through the entire list (besides current one), starting with the element after the current one, and looping around to the start.
                    var data = Props.verbs[(index + i) % Props.verbs.Count];
                    var verb = equippable.AllVerbs.FirstOrDefault(v => v.verbProps.untranslatedLabel == data.verbLabel);
                    if (verb != null)
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = data.gizmoLabelOverride ?? Props.gizmoLabel,
                            defaultDesc = data.gizmoDescriptionOverride ?? Props.gizmoDescription,
                            icon = data.gizmoIconOverride ?? Props.gizmoIcon,
                            action = () => ActiveVerb = verb
                        };
                    }
                }

                break;
            }
        }
    }

    protected virtual void InitActiveVerb()
    {
        if (equippable == null)
            return;

        // Try matching the verb to the default one we've set up
        if (!Props.defaultVerbLabel.NullOrEmpty())
        {
            var data = Props.verbs.FirstOrDefault(x => x.verbLabel == Props.defaultVerbLabel);
            if (data != null)
            {
                activeVerb = equippable.AllVerbs.FirstOrDefault(x => x.verbProps.untranslatedLabel == Props.defaultVerbLabel);
                if (activeVerb != null)
                {
                    activeVerbData = data;
                    return;
                }
            }
        }

        // If no match or no default verb, try matching any verb
        activeVerb = equippable.AllVerbs.FirstOrDefault(v =>
        {
            // Grab the verb data first
            var d = Props.verbs.FirstOrDefault(d => d.verbLabel == v.verbProps.untranslatedLabel);
            if (d == null)
                return false;

            // Set the active verb data before returning
            activeVerbData = d;
            return true;
        });
    }

    private void InitComps() => equippable = parent.GetComp<CompEquippable>();
}

// public class Command_MultiVerbTarget : Command_VerbTarget
// {
//     public override bool GroupsWith(Gizmo other)
//     {
//         if (other is not Command_MultiVerbTarget gizmo)
//             return false;
//         if (verb != gizmo.verb)
//             return false;
//
//         return base.GroupsWith(other);
//     }
// }

// [HarmonyPatch(typeof(VerbTracker), "CreateVerbTargetCommand")]
// public class Test
// {
//     private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
//     {
//         var ctor = typeof(Command_VerbTarget).DeclaredConstructor([]);
//         foreach (var ci in instr)
//         {
//             yield return ci;
//
//             if (ci.opcode == OpCodes.Newobj && ci.operand is ConstructorInfo info && info == ctor)
//             {
//                 yield return CodeInstruction.LoadArgument(0);
//                 yield return CodeInstruction.Call(() => Wrapper);
//             }
//         }
//     }
//
//     private static Command_VerbTarget Wrapper(Command_VerbTarget original, VerbTracker instance)
//     {
//         if (instance.directOwner is not CompEquippable equippable)
//             return original;
//         
//         if (equippable.parent.GetComp<CompMultiVerbWeapon>() == null)
//             return original;
//
//         return new Command_MultiVerbTarget();
//     }
//
//     // private static bool Prefix(Thing ownerThing, Verb verb, VerbTracker __instance)
//     // {
//     //     if (__instance.directOwner is not CompEquippable equippable)
//     //         return true;
//     //
//     //     var comp = equippable.parent.GetComp<CompMultiVerbWeapon>();
//     //     if (comp == null)
//     //         return true;
//     //
//     //     
//     // }
// }