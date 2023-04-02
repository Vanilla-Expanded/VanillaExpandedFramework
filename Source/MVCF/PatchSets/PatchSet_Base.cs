using System.Collections.Generic;
using HarmonyLib;
using MVCF.Utilities;
using Verse;

namespace MVCF.PatchSets;

public class PatchSet_Base : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Postfix(AccessTools.Method(typeof(Pawn), nameof(Pawn.ExposeData)), AccessTools.Method(GetType(), nameof(PostExposeDataPawn)));
        yield return Patch.Postfix(AccessTools.Method(typeof(Verb), nameof(Verb.ExposeData)), AccessTools.Method(GetType(), nameof(PostExposeDataVerb)));
        yield return Patch.Postfix(AccessTools.Method(typeof(VerbTracker), "InitVerb"), AccessTools.Method(GetType(), nameof(PostInitVerb)));
    }

    public static void PostExposeDataPawn(Pawn __instance)
    {
        __instance.SaveManager();
    }

    public static void PostExposeDataVerb(Verb __instance)
    {
        __instance.SaveManaged();
    }

    public static void PostInitVerb(VerbTracker __instance, Verb verb)
    {
        verb.InitializeManaged(__instance);
    }
}
