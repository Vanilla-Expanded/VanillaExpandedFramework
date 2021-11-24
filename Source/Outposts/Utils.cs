using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace Outposts
{
    public static class Utils
    {
        public static bool SatisfiedBy(this List<AmountBySkill> minSkills, IEnumerable<Pawn> pawns) =>
            minSkills.All(abs => pawns.Sum(p => p.skills.GetSkill(abs.Skill).Level) >= abs.Count);

        public static List<Pawn> HumanColonists(this Caravan caravan) => caravan.PawnsListForReading.Where(p => p.RaceProps.Humanlike && p.IsColonistPlayerControlled).ToList();

        public static IEnumerable<TResult> SelectOrEmpty<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
            source is null ? Enumerable.Empty<TResult>() : source.Select(selector);

        public static IEnumerable<TResult> SelectManyOrEmpty<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) =>
            source is null ? Enumerable.Empty<TResult>() : source.SelectMany(selector);

        public static string Line(this string input) => input.NullOrEmpty() ? "" : "\n" + input;
    }
}