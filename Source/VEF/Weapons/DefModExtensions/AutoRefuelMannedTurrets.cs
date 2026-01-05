using RimWorld;
using Verse;

namespace VEF.Weapons;

public class AutoRefuelMannedTurrets : DefModExtension
{
    /// <summary>
    /// Used for logging errors if the def is missing CompProperties_Mannable.
    /// This should be left as true unless you're making a C# mod that uses this extension and doesn't rely on mannable comp.
    /// </summary>
    protected bool logMissingMannableComp = true;
    /// <summary>
    /// Determines if a pawn will ever reload a single piece of ammo at a time (default behavior for reinforced barrels), or as much as possible.
    /// </summary>
    public bool reloadsMoreThanSingleItem = false;

    /// <summary>
    /// Used to check if a pawn should automatically reload a given mannable turret, either due to running out of ammo while shooting, or when trying to man a turret without any.
    /// </summary>
    /// <param name="building">The turret we're trying to reload.</param>
    /// <param name="currentResult">Result from the original method. Will be true if the turret is reloaded with reinforced barrels outside classic mortars mode.</param>
    /// <returns>True if the turret can be reloaded, false otherwise.</returns>
    public virtual bool ShouldAutoReload(Building building, bool currentResult)
    {
        // No need to do anything, we don't want to override the vanilla behaviour.
        // Vanilla always returns true in case the fuel is mortar barrel and classic mortars are disabled.
        if (currentResult)
            return true;

        // Only support Building_TurretGun, just like vanilla auto refuel
        if (building is not Building_TurretGun turretGun)
            return false;

        // True if comp is not null and has no fuel.
        return turretGun.GetComp<CompRefuelable>() is { HasFuel: false };
    }

    /// <summary>
    /// Used to modify the amount of fuel that the pawn will try to put into the turret.
    /// </summary>
    /// <param name="building">The turret we're trying to reload.</param>
    /// <param name="fuel">The fuel we're trying to use to refuel.</param>
    /// <returns>The amount that will be reloaded. Default for vanilla is 1, and this value will be clamped between 1 and max stack size.</returns>
    public virtual int ModifyRefuelCount(Building building, Thing fuel)
    {
        // The count is clamped to (1, fuel.stackSize)
        return building.TryGetComp<CompRefuelable>()?.GetFuelCountToFullyRefuel() ?? 1;
    }

    public override void ResolveReferences(Def parentDef)
    {
        // Can't use config errors, they don't pass over parent def, so we need to use ResolveReferences
        if (Prefs.DevMode && !parentDef.ignoreConfigErrors && logMissingMannableComp)
        {
            if (parentDef is not ThingDef def || def.GetCompProperties<CompProperties_Mannable>() == null)
                Log.Error($"{parentDef} doesn't have mannable comp, which is required by {nameof(AutoRefuelMannedTurrets)}.");
        }
    }
}