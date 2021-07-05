using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace VWEMakeshift 
{
    public class MakeshiftProperties : DefModExtension
    {
        public IntRange shots = new IntRange(0, 1);
    }
}
