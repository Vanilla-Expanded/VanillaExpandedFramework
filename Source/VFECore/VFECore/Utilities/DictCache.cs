using RimWorld;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace VFECore
{
    public interface ICacheable
    {
        bool RegenerateCache();
    }

    /// <summary>
    /// A quick method for making a cache without having to rewrite the same verbose code over and over.
    /// </summary>
    /// <typeparam name="T">The value you want to act as the key of the dictionary</typeparam>
    /// <typeparam name="V">A class whcih implements the ICachable Interface</typeparam>
    public abstract class DictCache<T, V> where V : ICacheable
    {
        public static ConcurrentDictionary<T, V> Cache { get; set; } = new();

        // JunkCache is for default data in case we could not regenerate.
        protected static readonly ConcurrentDictionary<T, V> JunkCache = new();

        public static V GetCache(T key, bool forceRefresh = false, bool canRefresh = true)
        {
            if (key == null)
                return default;
            if (Cache.TryGetValue(key, out V data))
            {
                // Check if the cache has timed out
                if (forceRefresh)
                {
                    data.RegenerateCache();
                    return data;
                }
                else
                {
                    return data;
                }
            }
            if (!forceRefresh && JunkCache.TryGetValue(key, out V junkData))
            {
                return junkData;
            }
            else
            {
                V newData = (V)Activator.CreateInstance(typeof(V), key);
                // If the cache is valid and was successfully regenerated add Dictionary. Otherwise just return a class with the default values.
                if (canRefresh && newData.RegenerateCache()) 
                {
                    Cache.TryAdd(key, newData);
                }
                else
                {
                    JunkCache[key] = newData;
                }
                return newData;
            }
        }
    }
}
