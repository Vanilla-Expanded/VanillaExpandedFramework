using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Buildings;


public class CompProperties_Refuelable_DualFuel : CompProperties_Refuelable
{
    public float secondaryFuelCapacity = 10f;
    public float initialSecondaryFuelPercent = 0f;
    public float autoRefuelSecondaryPercent = 0.3f;
    public ThingFilter secondaryFuelFilter;
    public bool initialAllowAutoRefuelSecondary = true;
    public bool showAllowAutoRefuelSecondaryToggle = false;
    public bool targetSecondaryFuelLevelConfigurable = false;
    public float initialConfigurableSecondaryTargetFuelLevel = 0f;
    public float minimumSecondaryFueledThreshold = 1f;

    private float secondaryFuelMultiplier = 1f;
    public bool factorSecondaryByDifficulty = false;

    [MustTranslate]
    public string secondaryFuelLabel;

    [MustTranslate]
    public string secondaryFuelGizmoLabel;

    [MustTranslate]
    public string outOfSecondaryFuelMessage;

    [NoTranslate]
    public string secondaryFuelIconPath;

    private Texture2D secondaryFuelIcon;

    public string SecondaryFuelLabel
    {
        get
        {
            if (secondaryFuelLabel.NullOrEmpty())
            {
                return "VFES_SecondaryFuel".Translate();
            }
            return secondaryFuelLabel;
        }
    }

    public string SecondaryFuelGizmoLabel
    {
        get
        {
            if (secondaryFuelGizmoLabel.NullOrEmpty())
            {
                return "VFES_SecondaryFuel".Translate();
            }
            return secondaryFuelGizmoLabel;
        }
    }

    public Texture2D SecondaryFuelIcon
    {
        get
        {
            if (secondaryFuelIcon == null)
            {
                if (!secondaryFuelIconPath.NullOrEmpty())
                {
                    secondaryFuelIcon = ContentFinder<Texture2D>.Get(secondaryFuelIconPath);
                }
                else
                {
                    ThingDef thingDef = secondaryFuelFilter?.AnyAllowedDef ?? ThingDefOf.Shell_HighExplosive;
                    secondaryFuelIcon = thingDef.uiIcon;
                }
            }
            return secondaryFuelIcon;
        }
    }

    public float SecondaryFuelMultiplierCurrentDifficulty
    {
        get
        {
            if (factorSecondaryByDifficulty && Find.Storyteller?.difficulty != null)
            {
                return secondaryFuelMultiplier / Find.Storyteller.difficulty.maintenanceCostFactor;
            }
            return secondaryFuelMultiplier;
        }
    }

    public CompProperties_Refuelable_DualFuel()
    {
        compClass = typeof(CompRefuelable_DualFuel);
    }

    public static HashSet<ThingDef> allSecondaryFuelDefs = new HashSet<ThingDef>();

    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);
        secondaryFuelFilter?.ResolveReferences();
        allSecondaryFuelDefs.Add(parentDef);
    }
}

public class CompRefuelable_DualFuel : CompRefuelable
{
    private float secondaryFuel;
    private float configuredSecondaryTargetFuelLevel = -1f;
    public bool allowAutoRefuelSecondary = true;
    public new CompProperties_Refuelable_DualFuel Props => props as CompProperties_Refuelable_DualFuel;
    public float SecondaryFuel => secondaryFuel;

    public float SecondaryFuelPercentOfTarget => secondaryFuel / SecondaryTargetFuelLevel;

    public float SecondaryFuelPercentOfMax => secondaryFuel / Props.secondaryFuelCapacity;

    public bool IsSecondaryFull => SecondaryTargetFuelLevel - secondaryFuel < 1f * Props.SecondaryFuelMultiplierCurrentDifficulty;

    public bool HasSecondaryFuel => secondaryFuel > 0f && secondaryFuel >= Props.minimumSecondaryFueledThreshold;

    public float SecondaryTargetFuelLevel
    {
        get
        {
            if (configuredSecondaryTargetFuelLevel >= 0f)
            {
                return configuredSecondaryTargetFuelLevel;
            }
            if (Props.targetSecondaryFuelLevelConfigurable)
            {
                return Props.initialConfigurableSecondaryTargetFuelLevel;
            }
            return Props.secondaryFuelCapacity;
        }
        set
        {
            configuredSecondaryTargetFuelLevel = Mathf.Clamp(value, 0f, Props.secondaryFuelCapacity);
        }
    }

    public bool ShouldAutoRefuelSecondaryNow
    {
        get
        {
            if (SecondaryFuelPercentOfTarget <= Props.autoRefuelSecondaryPercent &&
                !IsSecondaryFull &&
                SecondaryTargetFuelLevel > 0f)
            {
                return ShouldAutoRefuelSecondaryNowIgnoringFuelPct;
            }
            return false;
        }
    }

    public bool ShouldAutoRefuelSecondaryNowIgnoringFuelPct
    {
        get
        {
            if (!parent.IsBurning() &&
                parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.Flick) == null)
            {
                return parent.Map.designationManager.DesignationOn(parent, DesignationDefOf.Deconstruct) == null;
            }
            return false;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref secondaryFuel, "secondaryFuel", 0f);
        Scribe_Values.Look(ref configuredSecondaryTargetFuelLevel, "configuredSecondaryTargetFuelLevel", -1f);
        Scribe_Values.Look(ref allowAutoRefuelSecondary, "allowAutoRefuelSecondary", false);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && !Props.showAllowAutoRefuelSecondaryToggle)
        {
            allowAutoRefuelSecondary = Props.initialAllowAutoRefuelSecondary;
        }
    }

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        secondaryFuel = Props.secondaryFuelCapacity * Props.initialSecondaryFuelPercent;
    }

    public void ConsumeSecondaryFuel(float amount)
    {
        if (secondaryFuel <= 0f)
        {
            return;
        }
        secondaryFuel -= amount;
        if (secondaryFuel <= 0f)
        {
            secondaryFuel = 0f;
        }
    }

    public void RefuelSecondary(List<Thing> fuelThings)
    {
        int num = GetSecondaryFuelCountToFullyRefuel();
        while (num > 0 && fuelThings.Count > 0)
        {
            Thing thing = fuelThings.Pop();
            int num2 = Mathf.Min(num, thing.stackCount);
            RefuelSecondary(num2);
            thing.SplitOff(num2).Destroy();
            num -= num2;
        }
    }

    public void RefuelSecondary(float amount)
    {
        secondaryFuel += amount * Props.SecondaryFuelMultiplierCurrentDifficulty;
        if (secondaryFuel > Props.secondaryFuelCapacity)
        {
            secondaryFuel = Props.secondaryFuelCapacity;
        }
    }

    public int GetSecondaryFuelCountToFullyRefuel()
    {
        return Mathf.Max(Mathf.CeilToInt((SecondaryTargetFuelLevel - secondaryFuel) / Props.SecondaryFuelMultiplierCurrentDifficulty), 1);
    }

    public override string CompInspectStringExtra()
    {
        string text = base.CompInspectStringExtra();

        if (!text.NullOrEmpty())
        {
            text += "\n";
        }
        text += Props.SecondaryFuelLabel + ": " + secondaryFuel.ToStringDecimalIfSmall() + " / " + Props.secondaryFuelCapacity.ToStringDecimalIfSmall();

        if (!HasSecondaryFuel && !Props.outOfSecondaryFuelMessage.NullOrEmpty())
        {
            text += "\n" + Props.outOfSecondaryFuelMessage;
            text += $" ({GetSecondaryFuelCountToFullyRefuel()}x {Props.secondaryFuelFilter.AnyAllowedDef.label})";
        }

        return text;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }
        if (Find.Selector.SelectedObjects.Count == 1)
        {
            yield return new Gizmo_SetSecondaryFuelLevel(this);
        }
        else
        {
            if (Props.showAllowAutoRefuelSecondaryToggle)
            {
                string str = (allowAutoRefuelSecondary ? "On".Translate() : "Off".Translate());
                var toggle = new Command_Toggle
                {
                    isActive = () => allowAutoRefuelSecondary,
                    toggleAction = () => { allowAutoRefuelSecondary = !allowAutoRefuelSecondary; },
                    defaultLabel = "VFES_CommandToggleAllowAutoRefuelSecondary".Translate(),
                    defaultDesc = "CommandToggleAllowAutoRefuelDescMult".Translate(str.UncapitalizeFirst().Named("ONOFF")),
                    icon = allowAutoRefuelSecondary ? TexCommand.ForbidOn : TexCommand.ForbidOff,
                    Order = 21f
                };
                yield return toggle;
            }
        }
    }
}