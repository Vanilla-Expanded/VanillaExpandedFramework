using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore
{
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
}
