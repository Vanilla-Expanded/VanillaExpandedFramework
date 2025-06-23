using Prepatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VEF.AestheticScaling
{
    public static class CachedPawnDataExtensions
    {
        [ThreadStatic]
        private static CachedPawnData _placeholderCache;
        [PrepatcherField]
        [ValueInitializer(nameof(CachedPawnData.GetDefaultCache))]
        public static ref CachedPawnData GetCachePrePatched(this Pawn pawn)
        {
            _placeholderCache = PawnDataCache.GetCacheUltraSpeed(pawn, canRefresh: false);
            return ref _placeholderCache;
        }
    }
}
