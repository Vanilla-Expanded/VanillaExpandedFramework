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
    public class ScaleCache : DictCache<Pawn, SizeData>
    {
        public static SizeData GetScaleCache(Pawn pawn)
        {
            if (pawn == null || !pawn.RaceProps.Humanlike || (Scribe.mode != LoadSaveMode.Inactive))
                return null;

            return GetCache(pawn);
        }
    }

    
}
