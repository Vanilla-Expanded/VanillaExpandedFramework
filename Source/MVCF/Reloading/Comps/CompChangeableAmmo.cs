using System;
using System.Collections.Generic;
using MVCF.Reloading.Comps;
using MVCF.VerbComps;
using Verse;

namespace Reloading;

[Obsolete]
public class CompChangeableAmmo : CompReloadable
{
    public override IEnumerable<VerbCompProperties> GetCompsFor(VerbProperties verbProps)
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
                compClass = typeof(VerbComp_Reloadable_ChangeableAmmo)
            };
    }
}