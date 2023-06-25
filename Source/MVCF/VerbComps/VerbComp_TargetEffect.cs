using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MVCF.VerbComps;

public class VerbComp_TargetEffect : VerbComp
{
    private List<CompTargetEffect> comps;
    public VerbCompProperties_TargetEffect Props => props as VerbCompProperties_TargetEffect;

    public override bool PreCastShot()
    {
        var verb = parent.Verb;
        if (verb is Verb_CastTargetEffect)
        {
            var casterPawn = verb.CasterPawn;
            var thing = verb.CurrentTarget.Thing;
            if (casterPawn == null || thing == null) return false;
            foreach (var comp in comps) comp.DoEffectOn(casterPawn, thing);
            verb.ReloadableCompSource?.UsedOnce();
        }

        return true;
    }

    public override void Initialize(VerbCompProperties props, bool fromLoad)
    {
        base.Initialize(props, fromLoad);
        comps = new List<CompTargetEffect>();
        foreach (var targetEffect in Props.targetEffects)
        {
            CompTargetEffect comp = null;
            try
            {
                comp = (CompTargetEffect)Activator.CreateInstance(targetEffect.compClass);
                comp.parent = parent.Verb.EquipmentSource ?? parent.Manager.Pawn;
                comps.Add(comp);
                comp.Initialize(targetEffect);
            }
            catch (Exception arg)
            {
                Log.Error("[MVCF] VerbComp_TargetEffect could not instantiate or initialize a TargetEffect: " + arg);
                comps.Remove(comp);
            }
        }
    }
}

// ReSharper disable InconsistentNaming
public class VerbCompProperties_TargetEffect : VerbCompProperties
{
    public List<CompProperties> targetEffects;

    public VerbCompProperties_TargetEffect() => compClass = typeof(VerbComp_TargetEffect);
}
// ReSharper restore InconsistentNaming
