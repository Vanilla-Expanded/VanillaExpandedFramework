using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Outposts
{
    public static class Utils
    {
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
            count = Mathf.RoundToInt(count * OutpostsMod.Settings.ProductionMultiplier);
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

        public static string CheckSkill(this IEnumerable<Pawn> pawns, SkillDef skill, int minLevel)
        {
            return pawns.Sum(p => p.skills.GetSkill(skill).Level) < minLevel ? "Outposts.NotSkilledEnough".Translate(skill.skillLabel, minLevel) : null;
        }
    }

    public class Dialog_RenameOutpost : Dialog_Rename
    {
        private readonly Outpost outpost;

        public Dialog_RenameOutpost(Outpost outpost)
        {
            this.outpost = outpost;
            curName = outpost.Name;
        }

        protected override void SetName(string name)
        {
            outpost.Name = name;
        }
    }
}