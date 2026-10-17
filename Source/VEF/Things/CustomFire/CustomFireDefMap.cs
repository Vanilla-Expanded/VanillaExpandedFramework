using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VEF.Things;

public class CustomFireDefMap<V> : IExposable, IEnumerable<KeyValuePair<CustomFireDef, V>> where V : new()
{
    private List<V> values;

    public int Count => values.Count;

    public V this[CustomFireDef def]
    {
        get => values[def.fireIndex];
        set => values[def.fireIndex] = value;
    }

    public V this[int index]
    {
        get => values[index];
        set => values[index] = value;
    }

    public CustomFireDefMap()
    {
        var count = DefDatabase<CustomFireDef>.DefCount;
        if (count == 0)
            throw new Exception($"Constructed {nameof(CustomFireDefMap<>)}<{typeof(V)}> without defs being initialized. Try constructing it in ResolveReferences instead of the constructor.");

        values = new List<V>(count);
        for (var i = 0; i < count; i++)
            values.Add(new V());
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref values, "vals");

        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            var defCount = DefDatabase<CustomFireDef>.DefCount;
            for (var i = values.Count; i < defCount; i++)
                values.Add(new V());
            while (values.Count > defCount)
                values.RemoveLast();
        }
    }

    public void SetAll(V val)
    {
        for (var i = 0; i < values.Count; i++)
            values[i] = val;
    }

    public IEnumerator<KeyValuePair<CustomFireDef, V>> GetEnumerator() => DefDatabase<CustomFireDef>.AllDefsListForReading.Select(d => new KeyValuePair<CustomFireDef,V>(d, this[d])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}