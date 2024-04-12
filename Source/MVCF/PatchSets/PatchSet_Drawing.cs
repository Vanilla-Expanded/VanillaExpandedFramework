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
        yield return Patch.Postfix(AccessTools.Method(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras)),
            AccessTools.Method(GetType(), nameof(DrawVerbExtras)));
    }

    public static void DrawVerbExtras(Pawn pawn, Vector3 drawPos, Rot4 facing, PawnRenderFlags flags)
    {
        pawn.Manager(false)?.DrawVerbs(pawn, drawPos, facing, flags);
    }
}
