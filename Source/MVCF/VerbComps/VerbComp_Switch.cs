using MVCF.Comps;
using MVCF.Utilities;
using UnityEngine;
using Verse;

namespace MVCF.VerbComps;

public class VerbComp_Switch : VerbComp
{
    private bool enabled;
    public VerbCompProperties_Switch Props => props as VerbCompProperties_Switch;
    private string Label => parent.Verb.Label(parent.Props);
    public override bool Available() => base.Available() && enabled;

    public override void Initialize(VerbCompProperties props)
    {
        base.Initialize(props);
        parent.Enabled = enabled = Props.startEnabled;
    }

    public override Command OverrideTargetCommand(Command old)
    {
        if (enabled) return old;
        var newCommand = new Command_Action
        {
            defaultLabel = Label,
            defaultDesc = "VFED.SwitchTo".Translate(Label),
            icon = Props.SwitchIcon,
            action = Enable
        };
        if (old.disabled) newCommand.Disable(old.disabledReason);
        return newCommand;
    }

    public void Enable()
    {
        enabled = true;
        parent.Enabled = true;
        foreach (var verb in parent.Verb.EquipmentCompSource.AllVerbs)
            if (verb != parent.Verb && verb.Managed().TryGetComp<VerbComp_Switch>() is { enabled: true } comp)
            {
                comp.enabled = false;
                comp.parent.Enabled = false;
            }

        if (parent.Manager.Pawn.stances.curStance is Stance_Warmup stance) stance.Interrupt();
    }

    public override void ModifyScore(Pawn p, LocalTargetInfo target, ref float score)
    {
        base.ModifyScore(p, target, ref score);
        if (!enabled) score = 0f;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref enabled, nameof(enabled));
    }
}

public class VerbCompProperties_Switch : VerbCompProperties
{
    public bool startEnabled;
    public string switchIcon;
    public Texture2D SwitchIcon;

    public VerbCompProperties_Switch() : base(typeof(VerbComp_Switch)) { }

    public override void PostLoadSpecial(VerbProperties verbProps, AdditionalVerbProps additionalProps, Def parentDef)
    {
        base.PostLoadSpecial(verbProps, additionalProps, parentDef);
        LongEventHandler.ExecuteWhenFinished(delegate { SwitchIcon = ContentFinder<Texture2D>.Get(switchIcon); });
    }
}
