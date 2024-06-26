﻿using RimWorld;
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
        public static CachedPawnData GetPawnDataCache(Pawn pawn, bool forceRefresh=false, bool canRefresh = true)
        {
            if (//pawn?.RaceProps.Humanlike == true &&  // Maybe add a setting for this, if it is troublesome.
                // If the needs are null (and it isn't a corpse) then we don't want to generate data for it.
                // It typically means the pawn isn't fully initialized yet or otherwise unsuitable.
                (pawn?.needs != null || pawn?.Dead==true)) 
            {
                return GetCache(pawn, forceRefresh: forceRefresh, canRefresh: canRefresh);
            }
            return null;
        }
    }

    
}
