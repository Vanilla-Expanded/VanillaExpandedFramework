using System;
using System.Collections.Generic;
using MVCF.VerbComps;
using Verse;

namespace Reloading
{
    public interface IReloadable : ILoadReferenceable, VerbComp.IVerbCompProvider
    {
        int ShotsRemaining { get; set; }
        int ItemsPerShot { get; }
        int MaxShots { get; }
        Thing Thing { get; }
        ThingDef CurrentProjectile { get; }
        ThingDef AmmoExample { get; }
        object Parent { get; }
        List<ThingDefCountRangeClass> GenerateAmmo { get; }
        bool GenerateBackupWeapon { get; }
        Pawn Pawn { get; }
        bool CanReloadFrom(Thing ammo);
        Thing Reload(Thing ammo);
        int ReloadTicks(Thing ammo);
        bool NeedsReload();
        void Unload();
        void Notify_ProjectileFired();
        void ReloadEffect(int curTick, int ticksTillDone);
    }

    public interface IChangeableAmmo
    {
        IEnumerable<Pair<ThingDef, Action>> AmmoOptions { get; }
    }
}