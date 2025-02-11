using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static System.Collections.Specialized.BitVector32;

namespace VFECore
{
    public static class TaggedItemHelper
    {
        public static T GetTaggedItem<T>(this HashSet<T> taggedItems, string tag) where T : class, ITaggedItem
        {
            return taggedItems?.FirstOrDefault(ti => ti.Tag == tag);
        }

        public static T GetTaggedItem<T>(this ExposableHashSet<T> taggedItems, string tag) where T : class, ITaggedItem
        {
            return taggedItems?.FirstOrDefault(ti => ti.Tag == tag);
        }

        public static TaggedColor GetTaggedColorOnDef(this Def def, string tag) =>
            TaggedColorPropManager.GetDefTagItem(def, tag);

        public static TaggedText GetTaggedPathOnDef(this Def def, string tag) =>
            TaggedTextPropManager.GetDefTagItem(def, tag);

        public static TaggedColor GetColorByTag(this ILoadReferenceable target, string tag)
        {
            if (target is Faction faction) return faction.GetColorByTag(tag);
            else if (target is Thing thing) return thing.GetColorByTag(tag);
            else return null;
        }


        public static TaggedText GetStringByTag(this ILoadReferenceable target, string tag, Func<TaggedText, bool> predicate= null)
        {
            var result = target is Faction faction
                ? faction.GetStringByTag(tag) : target is Thing thing
                ? thing.GetStringByTag(tag) : null;

            if (result != null && predicate != null && !predicate(result)) return null;

            return result;
        }

        private static TaggedColor GetColorByTag(this Faction faction, string tag)
        {
            if (TaggedColorPropManager.TryGetTagItem(faction, tag, out var taggedClr))
                return taggedClr;
            if (TaggedColorPropManager.TryGetDefTagItem(faction?.def, tag, out taggedClr))
                return taggedClr;
            return null;
        }

        private static TaggedText GetStringByTag(this Faction faction, string tag)
        {
            if (tag.NullOrEmpty()) return null;
            if (tag.Contains('+'))
            {
                string fullPath = "";
                foreach (var tagPart in tag.Split('+'))
                {
                    if (faction.GetStringByTag(tagPart) is TaggedText partialTag) fullPath += partialTag.value;
                    else fullPath += tagPart;
                }
                return new TaggedText(tag, fullPath);
            }
            if (TaggedTextPropManager.TryGetTagItem(faction, tag, out var taggedPath))
                return taggedPath;
            if (TaggedTextPropManager.TryGetDefTagItem(faction?.def, tag, out taggedPath))
                return taggedPath;
            return null;
        }

        private static TaggedColor GetColorByTag(this Thing thing, string tag)
        {
            var pawn = thing as Pawn;

            // Get Instance-level override, if any.
            if (TaggedColorPropManager.TryGetTagItem(thing, tag, out var taggedClr))
                return taggedClr;
            if (TaggedColorPropManager.TryGetTagItem(thing.Faction, tag, out taggedClr))
                return taggedClr;
            if (pawn != null && TaggedColorPropManager.TryGetTagItem(pawn.Ideo, tag, out taggedClr))
                return taggedClr;

            // Get Def-level override, if any.
            if (pawn != null && TaggedColorPropManager.TryGetDefTagItem(pawn.kindDef, tag, out taggedClr))
                return taggedClr;
            if (TaggedColorPropManager.TryGetDefTagItem(thing.def, tag, out taggedClr))
                return taggedClr;
            if (TaggedColorPropManager.TryGetDefTagItem(thing.Faction?.def, tag, out taggedClr))
                return taggedClr;

            return null;
        }

        private static TaggedText GetStringByTag(this Thing thing, string tag)
        {
            if (tag.NullOrEmpty()) return null;
            if (tag.Contains('+'))
            {
                string fullPath = "";
                foreach (var tagPart in tag.Split('+'))
                {
                    if (thing.GetStringByTag(tagPart) is TaggedText partialTag) fullPath += partialTag.value;
                    else fullPath += tagPart;
                }
                return new TaggedText(tag, fullPath);
            }
            var pawn = thing as Pawn;

            // Get Instance-level override, if any.
            if (TaggedTextPropManager.TryGetTagItem(thing, tag, out var taggedPath))
                return taggedPath;
            if (TaggedTextPropManager.TryGetTagItem(thing.Faction, tag, out taggedPath))
                return taggedPath;
            if (pawn != null && TaggedTextPropManager.TryGetTagItem(pawn.Ideo, tag, out taggedPath))
                return taggedPath;

            // Get Def-level override, if any.
            if (pawn != null && TaggedTextPropManager.TryGetDefTagItem(pawn.kindDef, tag, out taggedPath))
                return taggedPath;
            if (TaggedTextPropManager.TryGetDefTagItem(thing.Faction?.def, tag, out taggedPath))
                return taggedPath;
            if (TaggedTextPropManager.TryGetDefTagItem(thing.def, tag, out taggedPath))
                return taggedPath;

            return null;
        }

        public static bool HasTagged(this Thing thing, string tag) => thing.GetColorByTag(tag) != null || thing.GetStringByTag(tag) != null;

        public static bool HasTaggedDirect(this ILoadReferenceable thing, string tag) => TaggedColorPropManager.HasTag(thing, tag) || TaggedTextPropManager.HasTag(thing, tag);

        /// <summary>
        /// Set a color tag on an object.
        /// </summary>
        /// <param name="obj">The object to set the tag on. Supported types are Pawn, Faction, Ideology.</param>
        /// <param name="tag">The name (string) of the tag. CaseSensitive.</param>
        /// <param name="color">Color value to set</param>
        public static void SetColorTag(this ILoadReferenceable obj, string tag, Color color) =>
            TaggedColorPropManager.SetTagItem(obj, new TaggedColor(tag, color));

        /// <summary>
        /// Set a string tag on an object.
        /// </summary>
        /// <param name="obj">The object to set the tag on. Supported types are Pawn, Faction, Ideology.</param>
        /// <param name="tag">The name (string) of the tag. CaseSensitive.</param>
        /// <param name="value">String to set. For graphics this is usually a path.</param>
        public static void SetStringTag(this ILoadReferenceable obj, string tag, string value) =>
            TaggedTextPropManager.SetTagItem(obj, new TaggedText(tag, value));

        public static void RemoveColorTag(this ILoadReferenceable obj, string tag) =>
            TaggedColorPropManager.RemoveTagItem(obj, tag);

        public static void RemoveStringTag(this ILoadReferenceable obj, string tag) =>
            TaggedTextPropManager.RemoveTagItem(obj, tag);
    }
}
