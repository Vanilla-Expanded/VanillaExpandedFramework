using RimWorld;
using Verse;

namespace MVCF.Utilities;

public static class VerbStatsUtility
{
    public static float? ForceBaseValue;

    public static float GetStatValueWithBase(this Thing thing, StatDef stat, float? baseValue, bool applyPostProcess = true)
    {
        ForceBaseValue = baseValue;
        var result = thing.GetStatValue(stat, applyPostProcess);
        ForceBaseValue = null;
        return result;
    }

    public static float GetStatValueAbstractWithBase(this BuildableDef def, StatDef stat, float? baseValue, ThingDef stuff = null)
    {
        ForceBaseValue = baseValue;
        var result = def.GetStatValueAbstract(stat, stuff);
        ForceBaseValue = null;
        return result;
    }

    public static float? GetStatValue(this VerbProperties props, StatDef stat)
    {
        if (stat == StatDefOf.RangedWeapon_Cooldown) return props.defaultCooldownTime;
        if (stat == StatDefOf.AccuracyTouch) return props.accuracyTouch;
        if (stat == StatDefOf.AccuracyShort) return props.accuracyShort;
        if (stat == StatDefOf.AccuracyMedium) return props.accuracyMedium;
        if (stat == StatDefOf.AccuracyLong) return props.accuracyLong;
        return null;
    }

    public static float? GetBaseValue(VerbProperties props, StatDef stat, ThingDef equipmentDef) =>
        equipmentDef.Verbs.Count > 1 ? props.GetStatValue(stat) : null;
}
