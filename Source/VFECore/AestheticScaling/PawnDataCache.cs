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
        public static CachedPawnData GetPawnDataCache(Pawn pawn, bool forceRefresh=false)
        {
            if (pawn?.RaceProps.Humanlike == true
                // If the needs are null (and it isn't a corpse) then we don't want to generate data for it.
                // It typically means the pawn isn't fully initialized yet or otherwise unsuitable.
                && (pawn.needs != null || pawn.Dead)) 
            {
                return GetCache(pawn, forceRefresh: forceRefresh);
            }
            return null;
        }
    }

    
}
