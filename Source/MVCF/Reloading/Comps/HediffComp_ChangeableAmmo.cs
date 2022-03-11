using System.Collections.Generic;
using MVCF.Reloading.Comps;
using MVCF.VerbComps;
using Verse;

namespace Reloading
{
    public class HediffComp_ChangeableAmmo : HediffComp_Reloadable
    {
        public override IEnumerable<VerbCompProperties> GetCompsFor(VerbProperties verbProps)
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
                    compClass = typeof(VerbComp_Reloadable_ChangeableAmmo)
                };
        }
    }
}