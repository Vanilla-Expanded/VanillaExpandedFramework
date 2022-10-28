using System;
using System.Collections.Generic;
using System.Linq;
using MVCF.Reloading.Comps;
using MVCF.VerbComps;
using Verse;

namespace Reloading;

[Obsolete]
public class CompReloadable : ThingComp, VerbComp.IVerbCompProvider
{
    public CompProperties_Reloadable Props => props as CompProperties_Reloadable;

    public virtual IEnumerable<VerbCompProperties> GetCompsFor(VerbProperties verbProps)
    {
        if (Props.VerbLabel.NullOrEmpty() ? !verbProps.IsMeleeAttack : verbProps.label == Props.VerbLabel)
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
                StartLoaded = Props.StartLoaded,
                compClass = typeof(VerbComp_Reloadable)
            };
    }
}

[Obsolete]
public class CompProperties_Reloadable : CompProperties
{
    public ThingFilter AmmoFilter;
    public List<ThingDefCountRangeClass> GenerateAmmo;
    public bool GenerateBackupWeapon;
    public int ItemsPerShot;
    public int MaxShots;
    public Type NewVerbClass;
    public bool PatchFirstFound;
    public SoundDef ReloadSound;
    public float ReloadTimePerShot;
    public bool StartLoaded = true;
    public string VerbLabel;

    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);
        AmmoFilter.ResolveReferences();
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        if (TargetVerb(parentDef) == null) yield return "Cannot find verb to be reloaded.";

        yield return "CompProperties_Reloadable has been deprecated. Use VerbCompProperties_Reloadable";

        foreach (var e in base.ConfigErrors(parentDef)) yield return e;
    }

    public override void PostLoadSpecial(ThingDef parent)
    {
        base.PostLoadSpecial(parent);
        MVCF.MVCF.EnabledFeatures.Add("Reloading");
        MVCF.MVCF.EnabledFeatures.Add("VerbComps");
        MVCF.MVCF.EnabledFeatures.Add("ExtraEquipmentVerbs");
        ref var type = ref TargetVerb(parent).verbClass;
        if (NewVerbClass != null) type = NewVerbClass;
        // PatchSet_ReloadingAuto.RegisterVerb(type, PatchFirstFound);
    }

    private VerbProperties TargetVerb(ThingDef parent)
    {
        return VerbLabel.NullOrEmpty()
            ? parent.Verbs.FirstOrDefault()
            : parent.Verbs.FirstOrDefault(v => v.label == VerbLabel);
    }
}