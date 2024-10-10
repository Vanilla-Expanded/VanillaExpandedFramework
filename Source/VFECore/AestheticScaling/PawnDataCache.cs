using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace VFECore
{
    public class PawnDataCache : DictCache<Pawn, CachedPawnData>
    {
        public struct PerThreadMiniCache
        {
            public Pawn pawn;
            public CachedPawnData cache;
        }
        [ThreadStatic]
        static PerThreadMiniCache threadStaticCache;

        /// <summary>
        /// Fetches the cache, like very, extremely fast.
        /// If the pawn trying to fetch the cache is the same as last time, it will reuse the previous cache,
        /// saving the diciotnary lookup call.
        /// 
        /// Set canRefresh to False if running from a thread.
        /// In case of errors, you're probably calling from a thread. Consider turning off canRefresh.
        /// </summary>
        public static CachedPawnData GetCacheUltraSpeed(Pawn pawn, bool canRefresh = true)
        {
            if (pawn == null) return CachedPawnData.defaultCache;
            if (threadStaticCache.pawn == pawn)
            {
                return threadStaticCache.cache;
            }
            else if (canRefresh) return GetPawnDataCache(pawn, canRefresh: canRefresh);
            else
            {
                threadStaticCache.cache = GetPawnDataCache(pawn, canRefresh: false);
                threadStaticCache.pawn = pawn;
                return threadStaticCache.cache;
            }
        }

        public static CachedPawnData GetPawnDataCache(Pawn pawn, bool forceRefresh=false, bool canRefresh = true)
        {
            if (pawn?.needs != null || pawn?.Dead==true)
            {
                return GetCache(pawn, forceRefresh: forceRefresh, canRefresh: canRefresh);
            }
            return null;
        }
    }

    
}
