using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore
{
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
                if (items.FirstOrDefault(ti => ti.Tag == item.Tag) is T existingItem)
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

        public static void RemoveTagItem(ILoadReferenceable obj, string tag)
        {
            if (obj is null) return;
            if (TaggedItems.TryGetValue(obj, out var items))
            {
                var item = items.FirstOrDefault(ti => ti.Tag == tag);
                if (item != null)
                {
                    items.Remove(item);
                }
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

        public static bool HasTag(ILoadReferenceable obj, string tag) => GetTagItem(obj, tag) != null;
        public static bool HasDefTag(Def def, string tag) => GetDefTagItem(def, tag) != null;

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

}
