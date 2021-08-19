// IReloadable.cs by Joshua Bennett
// 
// Created 2021-02-06

using System.Collections.Generic;
using Verse;

namespace Reloading
{
    public interface IReloadable
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
        bool CanReloadFrom(Thing ammo);
        Thing Reload(Thing ammo);
        int ReloadTicks(Thing ammo);
        bool NeedsReload();
        void Unload();
        void Notify_ProjectileFired();
        void ReloadEffect(int curTick, int ticksTillDone);
    }
}