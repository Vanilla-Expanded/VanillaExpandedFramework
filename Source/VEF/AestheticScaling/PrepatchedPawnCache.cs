using Prepatcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VEF.AestheticScaling
{
    public static class CachedPawnDataExtensions
    {
        public static bool prepatched = false;

        [ThreadStatic]
        private static CachedPawnData _placeholderCache;
        [PrepatcherField]
        [ValueInitializer(nameof(GetDefaultCache))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref CachedPawnData GetCachePrePatched(this Pawn pawn)
        {
            _placeholderCache = PawnDataCache.GetCacheUltraSpeed(pawn, canRefresh: false);
            return ref _placeholderCache;
        }
        private static CachedPawnData GetDefaultCache() => CachedPawnData.GetDefaultCache();
    }
}
