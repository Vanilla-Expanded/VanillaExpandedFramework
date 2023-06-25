using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

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

    public static float? GetBaseValue(VerbProperties props, StatDef stat, ThingDef equipmentDef)
    {
        if (equipmentDef.Verbs.Count > 1)
        {
            var baseValue = props.GetStatValue(stat);
            if (baseValue.HasValue && (baseValue < stat.minValue || baseValue > stat.maxValue))
                return stat.defaultBaseValue;
            return baseValue;
        }

        return null;
    }

    public static int GetDamage(this Verb verb)
    {
        return verb switch
        {
            Verb_LaunchProjectile launch => launch.Projectile.projectile.GetDamageAmount(1f),
            Verb_Bombardment => int.MaxValue,
            Verb_PowerBeam => int.MaxValue,
            Verb_MechCluster => int.MaxValue,
            Verb_CastTargetEffect => int.MaxValue,
            Verb_CastAbility cast => cast.ability.EffectComps.Count * 100,
            Verb_ShootBeam beam => beam.verbProps.beamDamageDef.defaultDamage,
            _ => 1
        };
    }

    public static IEnumerable<StatDrawEntry> DisplayStatsForVerbs(List<VerbProperties> verbs, StatCategoryDef category, StatRequest req)
    {
        string VerbLabel(VerbProperties verb) => verb.label;

        var warmupTime = verbs.CreateAverageStat(category, "RangedWarmupTime".Translate(), "Stat_Thing_Weapon_RangedWarmupTime_Desc".Translate(), 3555,
            (verb, _) => verb.warmupTime, "0.##", VerbLabel);

        if (warmupTime != null)
            yield return warmupTime;

        var damageAmount = verbs.CreateAverageStat(category, "Damage".Translate(), "Stat_Thing_Damage_Desc".Translate(), 5500,
            (verb, builder) => verb.defaultProjectile?.projectile?.GetDamageAmount(req.Thing, builder) ?? 0, VerbLabel, null,
            verb => verb.defaultProjectile?.projectile?.damageDef is { harmsHealth: true });

        if (damageAmount != null)
            yield return damageAmount;

        var armorPenetration = verbs.CreateAverageStat(category, "ArmorPenetration".Translate(), "ArmorPenetrationExplanation".Translate(), 5400,
            (verb, builder) =>
                verb.defaultProjectile?.projectile?.GetArmorPenetration(req.Thing, builder) ?? verb.beamDamageDef?.defaultArmorPenetration ?? 0, VerbLabel,
            GenText.ToStringPercent);

        if (armorPenetration != null)
            yield return armorPenetration;

        var buildingDamageFactor = verbs.CreateAverageStat(category, "BuildingDamageFactor".Translate(), "BuildingDamageFactorExplanation".Translate(), 5410,
            (verb, _) => verb.defaultProjectile?.projectile?.damageDef?.buildingDamageFactor ?? 0, VerbLabel, GenText.ToStringPercent,
            verb => verb.defaultProjectile?.projectile?.damageDef is { harmsHealth: true }
                 && verb.defaultProjectile.projectile.damageDef.buildingDamageFactor != 1);
        var dmgBuildingsImpassable = verbs.CreateAverageStat(category, "BuildingDamageFactorImpassable".Translate(),
            "BuildingDamageFactorImpassableExplanation".Translate(), 5420,
            (verb, _) => verb.defaultProjectile?.projectile?.damageDef?.buildingDamageFactorImpassable ?? 0, VerbLabel, GenText.ToStringPercent,
            verb => verb.defaultProjectile?.projectile?.damageDef is { harmsHealth: true }
                 && verb.defaultProjectile.projectile.damageDef.buildingDamageFactorImpassable != 1);
        var dmgBuildingsPassable = verbs.CreateAverageStat(category, "BuildingDamageFactorPassable".Translate(),
            "BuildingDamageFactorPassableExplanation".Translate(), 5430,
            (verb, _) => verb.defaultProjectile?.projectile?.damageDef?.buildingDamageFactorPassable ?? 0, VerbLabel, GenText.ToStringPercent,
            verb => verb.defaultProjectile?.projectile?.damageDef is { harmsHealth: true }
                 && verb.defaultProjectile.projectile.damageDef.buildingDamageFactorPassable != 1);

        if (buildingDamageFactor != null)
            yield return buildingDamageFactor;

        if (dmgBuildingsImpassable != null)
            yield return dmgBuildingsImpassable;

        if (dmgBuildingsPassable != null)
            yield return dmgBuildingsPassable;

        var burstShotCount = verbs.CreateAverageStat(category, "BurstShotCount".Translate(), "Stat_Thing_Weapon_BurstShotCount_Desc".Translate(), 5391,
            (verb, _) => verb.burstShotCount, VerbLabel, null, verb => verb.Ranged && verb.burstShotCount > 1);
        var rmp = verbs.CreateAverageStat(category, "BurstShotFireRate".Translate(), "Stat_Thing_Weapon_BurstShotFireRate_Desc".Translate(), 5392,
            (verb, _) => verb.ticksBetweenBurstShots == 0 ? 0 : 60f / verb.ticksBetweenBurstShots.TicksToSeconds(), "0.##", VerbLabel,
            verb => verb.Ranged && verb.burstShotCount > 1);

        if (burstShotCount != null)
        {
            yield return burstShotCount;
            if (rmp != null) yield return rmp;
        }

        var range = verbs.CreateAverageStat(category, "Range".Translate(), "Stat_Thing_Weapon_Range_Desc".Translate(), 5391, (verb, _) => verb.burstShotCount,
            "F0", VerbLabel, verb => verb.Ranged);
        if (range != null)
            yield return range;

        var stoppingPower = verbs.CreateAverageStat(category, "StoppingPower".Translate(), "StoppingPowerExplanation".Translate(), 5402,
            (verb, _) => verb.defaultProjectile?.projectile?.stoppingPower ?? 0, "F1", VerbLabel,
            verb => verb.Ranged && verb.defaultProjectile?.projectile != null && verb.defaultProjectile.projectile.stoppingPower != 0);
        if (stoppingPower != null)
            yield return stoppingPower;

        var forcedMissRadius = verbs.CreateAverageStat(category, "MissRadius".Translate(), "Stat_Thing_Weapon_MissRadius_Desc".Translate(), 3557,
            (verb, _) => verb.ForcedMissRadius, "0.#", VerbLabel);
        var directHitChance = verbs.CreateAverageStat(category, "DirectHitChance".Translate(), "Stat_Thing_Weapon_MissRadius_Desc".Translate(), 3560,
            (verb, _) => 1f / GenRadial.NumCellsInRadius(verb.ForcedMissRadius), VerbLabel, GenText.ToStringPercent, verb => verb.ForcedMissRadius > 0);
        if (forcedMissRadius != null)
        {
            yield return forcedMissRadius;
            if (directHitChance != null) yield return directHitChance;
        }
    }

    public static StatDrawEntry CreateAverageStat<T>(this List<T> items, StatCategoryDef category, string label, string explanationHeader,
        int displayPriorityInCategory, Func<T, StringBuilder, float> getValue, Func<T, string> getItemLabel = null, Func<float, string> stringifyValue = null,
        Func<T, bool> predicate = null)
    {
        var builder = new StringBuilder();
        builder.AppendLine(explanationHeader);
        builder.AppendLine();
        var total = 0f;
        var validCount = 0;
        foreach (var item in items)
        {
            var innerBuilder = new StringBuilder();
            var value = getValue(item, innerBuilder);
            if (!predicate?.Invoke(item) ?? value <= 0) continue;
            validCount++;
            builder.AppendLine(getItemLabel?.Invoke(item) ?? item.ToString());
            total += value;
            var innerText = innerBuilder.ToString();
            if (!innerText.Contains("StatsReport_FinalValue".Translate()))
            {
                innerBuilder.AppendInNewLine("StatsReport_FinalValue".Translate() + ": " + (stringifyValue?.Invoke(value) ?? value.ToString()));
                builder.AppendIndented(innerBuilder.ToString());
            }
            else builder.AppendIndented(innerText);

            builder.AppendLine();
            builder.AppendLine();
        }

        var average = total / validCount;
        var valueString = stringifyValue?.Invoke(average) ?? average.ToString();
        builder.AppendLine("StatsReport_FinalValue".Translate() + ": " + valueString);
        return total != 0
            ? new StatDrawEntry(category, label, valueString, builder.ToString(), displayPriorityInCategory)
            : null;
    }

    public static StatDrawEntry CreateAverageStat<T>(this List<T> items, StatCategoryDef category, string label, string explanationHeader,
        int displayPriorityInCategory, Func<T, StringBuilder, float> getValue, string format, Func<T, string> getItemLabel = null,
        Func<T, bool> predicate = null)
    {
        var builder = new StringBuilder();
        builder.AppendLine(explanationHeader);
        builder.AppendLine();
        var total = 0f;
        var validCount = 0;
        foreach (var item in items)
        {
            var innerBuilder = new StringBuilder();
            var value = getValue(item, innerBuilder);
            if (!predicate?.Invoke(item) ?? value <= 0) continue;
            validCount++;
            builder.AppendLine(getItemLabel?.Invoke(item) ?? item.ToString());
            total += value;
            var innerText = innerBuilder.ToString();
            if (!innerText.Contains("StatsReport_FinalValue".Translate()))
            {
                innerBuilder.AppendInNewLine("StatsReport_FinalValue".Translate() + ": " + value.ToString(format));
                builder.AppendIndented(innerBuilder.ToString());
            }
            else builder.AppendIndented(innerText);

            builder.AppendLine();
            builder.AppendLine();
        }

        var average = total / validCount;
        var valueString = average.ToString(format);
        builder.AppendLine("StatsReport_FinalValue".Translate() + ": " + valueString);
        return total != 0 ? new StatDrawEntry(category, label, valueString, builder.ToString(), displayPriorityInCategory) : null;
    }

    public static StringBuilder AppendIndented(this StringBuilder sb, string textBlock)
    {
        foreach (var line in textBlock.TrimEnd().Split('\n'))
            if (!string.IsNullOrWhiteSpace(line))
                sb.AppendLine($"    {line}");
        return sb;
    }
}
