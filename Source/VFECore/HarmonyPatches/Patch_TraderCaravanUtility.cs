using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_TraderCaravanUtility
    {

        [HarmonyPatch(typeof(TraderCaravanUtility), nameof(TraderCaravanUtility.GetTraderCaravanRole))]
        public static class GetTraderCaravanRole
        {

            public static void Postfix(Pawn p, ref TraderCaravanRole __result)
            {
                if (PawnKindDefExtension.Get(p.kindDef).countAsSlave)
                    __result = TraderCaravanRole.Chattel;
            }

        }

    }

}
