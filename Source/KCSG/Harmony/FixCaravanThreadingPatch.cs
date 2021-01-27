using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using UnityEngine;
using RimWorld;
using HarmonyLib;

namespace KCSG
{
    [HarmonyPatch(typeof(MapPawns))]
    [HarmonyPatch("AllPawns", MethodType.Getter)]
    public class FixCaravanThreadingPatch
    {
        public static void Postfix(ref List<Pawn> __result)
        {
            __result = __result.ListFullCopy();
        }
    }
}
