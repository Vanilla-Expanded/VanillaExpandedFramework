using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PipeSystem;

/// <summary>
/// Caches all consumer <see cref="CompResourceTrader"/> comps on the parent (one per connected pipe net)
/// and whether all of them are currently on. Read by the Building_TurretGun.Active and
/// Building_Turret.ThreatDisabled patches.
/// </summary>
public class CompTurretNeedsResource : ThingComp
{
    public CompProperties_TurretNeedsResource Props => (CompProperties_TurretNeedsResource)props;

    public CompResourceTrader[] Consumers { get; protected set; } = [];

    /// <summary>
    /// True if all consumer resource traders on the parent are on (or there are none).
    /// </summary>
    public bool ResourceActive { get; protected set; } = true;

    public override void PostPostMake()
    {
        base.PostPostMake();

        InitializeComps();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        if (Scribe.mode == LoadSaveMode.LoadingVars)
            InitializeComps();
    }

    public override string CompInspectStringExtra()
    {
        if (ResourceActive || !parent.Spawned || Props.resourceOffReasonKey.NullOrEmpty())
            return base.CompInspectStringExtra();

        // Use the first resource that's off for the translation arguments.
        for (var i = 0; i < Consumers.Length; i++)
        {
            var trader = Consumers[i];
            if (!trader.ResourceOn)
            {
                // Matches vanilla Building_TurretGun's "Cannot fire: Roofed" line
                return "CannotFire".Translate() + ": " + Props.resourceOffReasonKey.Translate([.. trader.InspectStringArguments()]).CapitalizeFirst();
            }
        }

        return base.CompInspectStringExtra();
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        // Recalculated here rather than in InitializeComps, since the traders may not have
        // loaded their ResourceOn state yet when our PostExposeData runs.
        RecalculateResourceActive();
    }

    /// <summary>
    /// Cache comps on PostPostMake and ExposeData (loading vars) only, matching vanilla behavior.
    /// Not done in PostSpawnSetup, as things (like minified buildings) may never be spawned.
    /// </summary>
    protected virtual void InitializeComps()
    {
        // Assign the injected field, so the patches only need to read it.
        // Turrets without this comp keep it null, and never call GetComp.
        if (parent is Building_Turret turret)
            turret.ResourceNeed = this;

        var list = new List<CompResourceTrader>();
        foreach (var trader in parent.GetComps<CompResourceTrader>())
        {
            // Classify by props rather than runtime consumption, since that can change
            // (idle consumption, buildings modifying BaseConsumption, etc.)
            if (trader.Props.consumptionPerTick > 0f)
                list.Add(trader);
        }

        Consumers = [.. list];
    }

    public override void ReceiveCompSignal(string signal)
    {
        base.ReceiveCompSignal(signal);

        // CompResourceTrader.ResourceOn broadcasts On/Off signals when its state changes.
        // Signals are rare, so recalculate on any of them instead of matching specific
        // signal strings. This also catches up if a state change happened without a signal.
        RecalculateResourceActive();
    }

    protected void RecalculateResourceActive()
    {
        for (var i = 0; i < Consumers.Length; i++)
        {
            if (!Consumers[i].ResourceOn)
            {
                ResourceActive = false;
                return;
            }
        }

        ResourceActive = true;
    }
}