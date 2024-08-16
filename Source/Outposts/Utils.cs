using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Outposts;

public static class Utils
{
    public delegate void ImmunityTick(ImmunityHandler immunity);

    public static readonly ImmunityTick immunityTick =
        AccessTools.MethodDelegate<ImmunityTick>(AccessTools.Method(typeof(ImmunityHandler), "ImmunityHandlerTick"));

    public static bool SatisfiedBy(this List<AmountBySkill> minSkills, IEnumerable<Pawn> pawns) =>
        minSkills.All(abs => pawns.Sum(p => p.skills.GetSkill(abs.Skill).Level) >= abs.Count);

    public static List<Pawn> HumanColonists(this Caravan caravan) => caravan.PawnsListForReading.Where(p => p.IsFreeColonist).ToList();
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> source) => source ?? Enumerable.Empty<T>();

    public static IEnumerable<TResult> SelectOrEmpty<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
        source is null ? Enumerable.Empty<TResult>() : source.Select(selector);

    public static IEnumerable<TResult> SelectManyOrEmpty<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) =>
        source is null ? Enumerable.Empty<TResult>() : source.SelectMany(selector);

    public static string Line(this string input, bool show = true) => !show || input.NullOrEmpty() ? "" : "\n" + input;
    public static string Line(this TaggedString input, bool show = true) => !show || input.NullOrEmpty() ? "" : "\n" + input.RawText;

    public static IEnumerable<Thing> Make(this ThingDef thingDef, int count, ThingDef stuff = null)
    {
        while (count > thingDef.stackLimit)
        {
            var temp = ThingMaker.MakeThing(thingDef, stuff);
            temp.stackCount = thingDef.stackLimit;
            yield return temp;
            count -= thingDef.stackLimit;
        }

        var temp2 = ThingMaker.MakeThing(thingDef, stuff);
        temp2.stackCount = count;
        yield return temp2;
    }

    public static string Requirement(this string req, bool passed) => $"{(passed ? "✓" : "✖")} {req}".Colorize(passed ? Color.green : Color.red);
    public static string Requirement(this TaggedString req, bool passed) => $"{(passed ? "✓" : "✖")} {req.RawText}".Colorize(passed ? Color.green : Color.red);

    public static string RequirementsStringBase(this OutpostExtension ext, int tileIdx, IEnumerable<Pawn> ps)
    {
        var builder = new StringBuilder();
        var biome = Find.WorldGrid[tileIdx].biome;
        string reason = "Outposts.NoValidPawns".Translate();
        var pawns = ps.Where(p => ext.CanAddPawn(p, out reason)).ToList();
        if (pawns.Count == 0) builder.AppendLine(reason.Requirement(false));
        if (ext.AllowedBiomes is { Count: > 0 })
        {
            builder.AppendLine("Outposts.AllowedBiomes".Translate().Requirement(ext.AllowedBiomes.Contains(biome)));
            builder.AppendLine(ext.AllowedBiomes.Select(b => b.label).ToLineList("  ", true));
        }

        if (ext.DisallowedBiomes is { Count: > 0 })
        {
            builder.AppendLine("Outposts.DisallowedBiomes".Translate().Requirement(!ext.DisallowedBiomes.Contains(biome)));
            builder.AppendLine(ext.DisallowedBiomes.Select(b => b.label).ToLineList("  ", true));
        }

        if (ext.MinPawns > 0) builder.AppendLine(Requirement("Outposts.NumPawns".Translate(ext.MinPawns), pawns.Count >= ext.MinPawns));

        if (ext.RequiredSkills is { Count: > 0 })
            foreach (var requiredSkill in ext.RequiredSkills)
                builder.AppendLine("Outposts.RequiredSkill".Translate(requiredSkill.Skill.skillLabel, requiredSkill.Count)
                   .Requirement(pawns.Sum(p => p.skills.GetSkill(requiredSkill.Skill).Level) >= requiredSkill.Count));

        if (ext.RequiresGrowing)
            builder.AppendLine("Outposts.GrowingRequired".Translate()
               .Requirement(GenTemperature.TwelfthsInAverageTemperatureRange(tileIdx, 6f, 42f)?.Any() ?? false));

        if (ext.CostToMake is { Count: > 0 })
        {
            var caravan = Find.WorldObjects.PlayerControlledCaravanAt(tileIdx);
            foreach (var tdcc in ext.CostToMake)
                builder.AppendLine("Outposts.MustHaveInCaravan".Translate(tdcc.Label)
                   .Requirement(CaravanInventoryUtility.HasThings(caravan, tdcc.thingDef, tdcc.count)));
        }

        return builder.ToString();
    }

    public static bool CanAddPawn(this OutpostExtension ext, Pawn pawn, out string reason)
    {
        if (ext?.Event is not null && !IdeoUtility.DoerWillingToDo(ext.Event, pawn))
        {
            reason = "IdeoligionForbids".Translate();
            return false;
        }

        reason = null;
        return true;
    }

    public static string CanSpawnOnWithExt(this OutpostExtension ext, int tileIdx, IEnumerable<Pawn> ps)
    {
        string reason = "Outposts.NoValidPawns".Translate();
        var pawns = ps.Where(p => ext.CanAddPawn(p, out reason)).ToList();
        if (pawns.Count == 0) return reason;
        if (Find.WorldGrid[tileIdx] is { biome: var biome } && ((ext.DisallowedBiomes is { Count: > 0 } && ext.DisallowedBiomes.Contains(biome)) ||
                                                                (ext.AllowedBiomes is { Count: > 0 } && !ext.AllowedBiomes.Contains(biome))))
            return "Outposts.CannotBeMade".Translate(biome.label);
        if (Find.WorldObjects.AnySettlementBaseAtOrAdjacent(tileIdx) ||
            Find.WorldObjects.AllWorldObjects.OfType<Outpost>().Any(outpost => Find.WorldGrid.IsNeighborOrSame(tileIdx, outpost.Tile)))
            return "Outposts.TooClose".Translate();
        if (ext.MinPawns > 0 && pawns.Count < ext.MinPawns)
            return "Outposts.NotEnoughPawns".Translate(ext.MinPawns);
        if (ext.RequiredSkills is { Count: > 0 } &&
            ext.RequiredSkills.FirstOrDefault(requiredSkill => pawns.Sum(p => p.skills.GetSkill(requiredSkill.Skill).Level) < requiredSkill.Count) is
            {
                Skill: { skillLabel: var skillLabel }, Count: var minLevel
            })
            return "Outposts.NotSkilledEnough".Translate(skillLabel, minLevel);
        if (ext.CostToMake is { Count: > 0 })
        {
            var caravan = Find.WorldObjects.PlayerControlledCaravanAt(tileIdx);
            if (ext.CostToMake.FirstOrDefault(tdcc => !CaravanInventoryUtility.HasThings(caravan, tdcc.thingDef, tdcc.count)) is { Label: var label })
                return "Outposts.MustHaveInCaravan".Translate(label);
        }

        return null;
    }

    public static string CheckSkill(this IEnumerable<Pawn> pawns, SkillDef skill, int minLevel)
    {
        return pawns.Sum(p => p.skills.GetSkill(skill).Level) < minLevel ? "Outposts.NotSkilledEnough".Translate(skill.skillLabel, minLevel) : null;
    }
}

public class Dialog_RenameOutpost : Dialog_Rename<Outpost>
{
    private readonly Outpost outpost;

    public Dialog_RenameOutpost(Outpost outpost) : base(outpost)
    {
        this.outpost = outpost;
        curName = outpost.Name;
    }
}
