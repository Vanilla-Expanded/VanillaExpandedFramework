using System.Collections.Generic;
using System.Linq;
using MVCF.Commands;
using RimWorld;
using UnityEngine;
using Verse;

namespace MVCF.VerbComps;

public class VerbComp_Cooldown : VerbComp
{
    private int cooldownTicksLeft;
    public VerbCompProperties_Cooldown Props => props as VerbCompProperties_Cooldown;

    public float CooldownPct => cooldownTicksLeft.TicksToSeconds() / Props.cooldownTime;
    public override bool NeedsTicking => true;

    public string CooldownDesc => cooldownTicksLeft.ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor);

    public override void Notify_ShotFired()
    {
        base.Notify_ShotFired();
        if (!parent.Verb.Bursting) cooldownTicksLeft = Props.cooldownTime.SecondsToTicks();
    }

    public override void CompTick()
    {
        base.CompTick();
        if (cooldownTicksLeft > 0) cooldownTicksLeft--;
    }

    public override IEnumerable<CommandPart> GetCommandParts(Command_VerbTargetExtended command) =>
        base.GetCommandParts(command)
           .Append(new CommandPart_Cooldown
            {
                parent = command,
                Comp = this
            });

    public override bool Available() => base.Available() && cooldownTicksLeft <= 0;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref cooldownTicksLeft, nameof(cooldownTicksLeft));
    }
}

[StaticConstructorOnStartup]
public class CommandPart_Cooldown : CommandPart
{
    public static readonly Texture2D CooldownTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));
    public VerbComp_Cooldown Comp;

    public override void PostInit()
    {
        base.PostInit();
        if (Comp.CooldownPct > 0.01f) parent.Disable("MVCF.Cooldown".Translate(Comp.CooldownDesc));
    }

    public override void PostGizmoOnGUI(Rect butRect, GizmoRenderParms parms, ref GizmoResult result)
    {
        base.PostGizmoOnGUI(butRect, parms, ref result);

        if (Comp.CooldownPct > 0.01f)
            GUI.DrawTexture(butRect.RightPartPixels(butRect.width * Comp.CooldownPct), CooldownTex);
    }
}

public class VerbCompProperties_Cooldown : VerbCompProperties
{
    public float cooldownTime;
}
