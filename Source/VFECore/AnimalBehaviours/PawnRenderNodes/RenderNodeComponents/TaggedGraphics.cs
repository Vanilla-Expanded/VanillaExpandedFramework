using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Xml;
using UnityEngine;
using UnityEngine.Assertions;
using Verse;

namespace VFECore
{
    // This file needs to be split into multiple files.

    public interface ITaggedItem
    {
        string Tag { get; }
    }
    public abstract class TaggedItem<T> : IExposable, IEquatable<TaggedItem<T>>, ITaggedItem
    {
        public string tag;
        public T value;
        public string Tag => tag;

        public TaggedItem() { }

        public TaggedItem(string tag, T value)
        {
            this.tag = tag;
            this.value = value;
        }

        public bool Equals(TaggedItem<T> other)
        {
            return tag == other.tag && EqualityComparer<T>.Default.Equals(value, other.value);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref tag, "tag");
            Scribe_Values.Look(ref value, "value");
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            tag = xmlRoot.Name;
            value = ParseHelper.FromString<T>(xmlRoot.InnerText);
        }

        public string GetUniqueLoadID()
        {
            return tag + value.GetHashCode();
        }

        public override string ToString()
        {
            return $"TaggedItem<{typeof(T)}> {tag}: {value}";
        }
    }

    public class TaggedColor : TaggedItem<Color>
    {
        public TaggedColor() { }
        public TaggedColor(string tag, Color value) : base(tag, value) { }
    }
    public class TaggedText : TaggedItem<string>
    {
        public TaggedText() { }
        public TaggedText(string tag, string value) : base(tag, value) { }
    }

    public class TaggedDefProperties : DefModExtension
    {
        public List<TaggedColor> taggedColors = [];
        public List<TaggedText> taggedStrings = [];

        public override IEnumerable<string> ConfigErrors()
        {
            // Check so no tags or their entries are null.
            if (taggedColors.Any(tc => tc.tag.NullOrEmpty() || tc.value == null))
                yield return "TaggedColor has null or empty tag or value.";
            if (taggedStrings.Any(tp => tp.tag.NullOrEmpty() || tp.value.NullOrEmpty()))
                yield return "TaggedPath has null or empty tag or value.";
        }

        public List<T> GetTaggedItems<T>() where T : ITaggedItem
        {
            if (typeof(T) == typeof(TaggedColor))
                return taggedColors as List<T>;
            else if (typeof(T) == typeof(TaggedText))
                return taggedStrings as List<T>;
            else 
                throw new NotImplementedException($"Attempted to fetch tagged item for unsuported type: {typeof(T)}");
        }
    }

    /// <summary>
    /// For some reason Ludeon doesn't expose HashSets it seems?
    /// Feel free to replace this if they do support them somehow and I just didn't find it. /Red.
    /// </summary>
    public class ExposableHashSet<T> : IExposable
    {
        public HashSet<T> items = [];
        private List<T> iExposableItems = [];
        public ExposableHashSet() { }
        public ExposableHashSet(IEnumerable<T> items) => this.items = items.ToHashSet();
        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                iExposableItems = [.. items];
            }
            Scribe_Collections.Look(ref iExposableItems, "items", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                items = [.. iExposableItems];
            }
        }
        public void Add(T item) => items.Add(item);
        public bool Remove(T item) => items.Remove(item);
        public bool Contains(T item) => items.Contains(item);
        public int Count => items.Count;
        public bool TryGetValue(T item, out T value) => items.TryGetValue(item, out value);
        public T FirstOrDefault(Func<T, bool> predicate) => items.FirstOrDefault(predicate);

    }

    public abstract class TaggedPropManagerBase<T> : GameComponent where T : class, ITaggedItem
    {
        public static TaggedPropManagerBase<T> instance = null;
        private static readonly Dictionary<Def, HashSet<T>> taggedDefItems = [];

        private Dictionary<ILoadReferenceable, ExposableHashSet<T>> taggedItems = [];

        private static Dictionary<ILoadReferenceable, ExposableHashSet<T>> TaggedItems => instance.taggedItems;

        public TaggedPropManagerBase(Game game)
        {
            instance = this;
        }

        public static bool TryGetTagItem(ILoadReferenceable obj, string tag, out T item) => (item = GetTagItem(obj, tag)) != null;
        public static bool TryGetDefTagItem(Def def, string tag, out T item) => (item = GetDefTagItem(def, tag)) != null;

        public static void SetTagItem(ILoadReferenceable obj, T item)
        {
            if (obj is null) return;
            if (TaggedItems.TryGetValue(obj, out var items))
            {
                if (items.TryGetValue(item, out var existingItem))
                {
                    items.Remove(existingItem);
                }
                items.Add(item);
            }
            else
            {
                TaggedItems[obj] = new ExposableHashSet<T>([item]);
            }
        }

        public static T GetTagItem(ILoadReferenceable obj, string tag)
        {
            if (obj is null) return null;
            if (TaggedItems.TryGetValue(obj, out var items))
            {
                return items.GetTaggedItem(tag);
            }
            return null;
        }

        public static T GetDefTagItem(Def def, string tag)
        {
            if (def is null) return null;
            if (taggedDefItems.TryGetValue(def, out var items))
            {
                return items.GetTaggedItem(tag);
            }
            else
            {
                HashSet<T> newValues = [];
                foreach (var taggedProps in def.GetModExtensions<TaggedDefProperties>())
                {
                    var tItems = taggedProps.GetTaggedItems<T>();
                    newValues.UnionWith(tItems);
                }
                taggedDefItems[def] = newValues;
                return newValues.GetTaggedItem(tag);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref taggedItems, "taggedItems", LookMode.Reference, LookMode.Deep, ref taggedItemsKeys, ref taggedItemsValues);
        }

        private List<ILoadReferenceable> taggedItemsKeys;
        private List<ExposableHashSet<T>> taggedItemsValues;
    }

    public class TaggedColorPropManager(Game game) : TaggedPropManagerBase<TaggedColor>(game) { }
    public class TaggedTextPropManager(Game game) : TaggedPropManagerBase<TaggedText>(game) { }

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

        public static TaggedColor ColorByTag(this Faction faction, string tag)
        {
            if (TaggedColorPropManager.TryGetTagItem(faction, tag, out var taggedClr))
                return taggedClr;
            if (TaggedColorPropManager.TryGetDefTagItem(faction?.def, tag, out taggedClr))
                return taggedClr;
            return null;
        }

        public static TaggedText StringByTag(this Faction faction, string tag)
        {
            if (tag.NullOrEmpty()) return null;
            if (tag.Contains('+'))
            {
                string fullPath = "";
                foreach (var tagPart in tag.Split('+'))
                {
                    if (faction.StringByTag(tagPart) is TaggedText partialTag) fullPath += partialTag.value;
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

        public static TaggedColor GetColorByTag(this Pawn pawn, string tag)
        {
            // Get Instance-level override, if any.
            if (TaggedColorPropManager.TryGetTagItem(pawn, tag, out var taggedClr))
                return taggedClr;
            if (TaggedColorPropManager.TryGetTagItem(pawn.Faction, tag, out taggedClr))
                return taggedClr;
            if (TaggedColorPropManager.TryGetTagItem(pawn.Ideo, tag, out taggedClr))
                return taggedClr;

            // Get Def-level override, if any.
            if (TaggedColorPropManager.TryGetDefTagItem(pawn.kindDef, tag, out taggedClr))
                return taggedClr;
            if (TaggedColorPropManager.TryGetDefTagItem(pawn.def, tag, out taggedClr))
                return taggedClr;
            if (TaggedColorPropManager.TryGetDefTagItem(pawn.Faction?.def, tag, out taggedClr))
                return taggedClr;

            return null;
        }

        public static TaggedText GetStringByTag(this Pawn pawn, string tag)
        {
            if (tag.NullOrEmpty()) return null;
            if (tag.Contains('+'))
            {
                string fullPath = "";
                foreach (var tagPart in tag.Split('+'))
                {
                    if (pawn.GetStringByTag(tagPart) is TaggedText partialTag) fullPath += partialTag.value;
                    else fullPath += tagPart;
                }
                return new TaggedText(tag, fullPath);
            }

            // Get Instance-level override, if any.
            if (TaggedTextPropManager.TryGetTagItem(pawn, tag, out var taggedPath))
                return taggedPath;
            if (TaggedTextPropManager.TryGetTagItem(pawn.Faction, tag, out taggedPath))
                return taggedPath;
            if (TaggedTextPropManager.TryGetTagItem(pawn.Ideo, tag, out taggedPath))
                return taggedPath;

            // Get Def-level override, if any.
            if (TaggedTextPropManager.TryGetDefTagItem(pawn.kindDef, tag, out taggedPath))
                return taggedPath;
            if (TaggedTextPropManager.TryGetDefTagItem(pawn.def, tag, out taggedPath))
                return taggedPath;
            if (TaggedTextPropManager.TryGetDefTagItem(pawn.Faction?.def, tag, out taggedPath))
                return taggedPath;

            return null;
        }

        public static bool HasTagged(this Pawn pawn, string tag) => pawn.GetColorByTag(tag) != null || pawn.GetStringByTag(tag) != null;
    }
}
