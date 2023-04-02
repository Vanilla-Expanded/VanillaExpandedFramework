using System.Collections.Generic;
using HarmonyLib;
using MVCF.Utilities;
using UnityEngine;
using Verse;

namespace MVCF.PatchSets;

public class PatchSet_Drawing : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Postfix(AccessTools.Method(typeof(Pawn), nameof(Pawn.DrawAt)), AccessTools.Method(GetType(), nameof(Postfix_Pawn_DrawAt)));
    }

    public static void Postfix_Pawn_DrawAt(Pawn __instance, Vector3 drawLoc, bool flip = false)
    {
        __instance.Manager(false)?.DrawAt(drawLoc);
    }
}
