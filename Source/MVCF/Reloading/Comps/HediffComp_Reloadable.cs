using System;
using System.Collections.Generic;
using System.Linq;
using MVCF.Reloading.Comps;
using MVCF.VerbComps;
using Verse;

namespace Reloading;

public class HediffComp_Reloadable : HediffComp
{
    public HediffCompProperties_Reloadable Props => props as HediffCompProperties_Reloadable;

    public virtual IEnumerable<VerbCompProperties> GetCompsFor(VerbProperties verbProps)
    {
        if (verbProps.label is null || verbProps.label == Props.VerbLabel)
            yield return new VerbCompProperties_Reloadable
            {
                AmmoFilter = Props.AmmoFilter,
                GenerateAmmo = Props.GenerateAmmo,
                GenerateBackupWeapon = Props.GenerateBackupWeapon,
                ItemsPerShot = Props.ItemsPerShot,
                MaxShots = Props.MaxShots,
                NewVerbClass = Props.NewVerbClass,
                ReloadTimePerShot = Props.ReloadTimePerShot,
                ReloadSound = Props.ReloadSound,
                compClass = typeof(VerbComp_Reloadable)
            };
    }
}

public class HediffCompProperties_Reloadable : HediffCompProperties
{
    public ThingFilter AmmoFilter;
    public List<ThingDefCountRangeClass> GenerateAmmo;
    public bool GenerateBackupWeapon;
    public int ItemsPerShot;
    public int MaxShots;
    public Type NewVerbClass;
    public SoundDef ReloadSound;
    public float ReloadTimePerShot;
    public string VerbLabel;

    public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
    {
        if (TargetVerb(parentDef) == null) yield return "Cannot find verb to be reloaded.";
        yield return "HediffCompProperties_Reloadable has been deprecated. Use VerbCompProperties_Reloadable";

        foreach (var e in base.ConfigErrors(parentDef)) yield return e;
    }

    public override void ResolveReferences(HediffDef parent)
    {
        base.ResolveReferences(parent);
        AmmoFilter.ResolveReferences();
        MVCF.MVCF.EnabledFeatures.Add("Reloading");
        MVCF.MVCF.EnabledFeatures.Add("VerbComps");
        MVCF.MVCF.EnabledFeatures.Add("ExtraEquipmentVerbs");
        var verb = TargetVerb(parent);
        if (verb == null) return;
        if (NewVerbClass != null) verb.verbClass = NewVerbClass;
        // PatchSet_ReloadingAuto.RegisterVerb(verb.verbClass, PatchFirstFound);
    }

    private VerbProperties TargetVerb(HediffDef parent)
    {
        var verbs = parent.CompProps<HediffCompProperties_VerbGiver>().verbs;
        return VerbLabel.NullOrEmpty()
            ? verbs.FirstOrDefault()
            : verbs.FirstOrDefault(v => v.label == VerbLabel);
    }
}