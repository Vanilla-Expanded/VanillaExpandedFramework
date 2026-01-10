using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VEF.Weapons;

[HarmonyPatch]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class JobDriver_ManTurret_RefuelIfNeededLambda_Patch
{
    private static bool Prepare() => JobDriver_ManTurret_GunNeedsRefueling_Patch.Prepare();

    private static MethodBase TargetMethod()
    {
        const string refuelToilFieldName = "refuelIfNeeded";
        const string noFuelMessageText = "MessageOutOfNearbyFuelFor";
        const string errorMessage = "Pawns operating mannable turrets will only grab a single piece of ammo to refuel.";

        var innerType = typeof(JobDriver_ManTurret).InnerTypes().FirstOrDefault(x => x.DeclaredField(refuelToilFieldName) != null);

        if (innerType == null)
        {
            Log.Error($"[VEF] Failed to find inner class with \"{refuelToilFieldName}\" field. {errorMessage}");
            return null;
        }

        var method = innerType.FirstMethod(m => PatchProcessor.GetOriginalInstructions(m).Any(ci => ci.LoadsConstant(noFuelMessageText)));
        if (method == null)
            Log.Error($"[VEF] Failed to find a method with \"{noFuelMessageText}\" string. {errorMessage}");

        return method;
    }

    private static void Postfix(Toil ___refuelIfNeeded)
    {
        var job = ___refuelIfNeeded.actor.CurJob;

        var extension = job.targetA.Thing?.def?.GetModExtension<AutoRefuelMannedTurrets>();
        // Check if extension is null, or we only want to reload a single item
        if (extension is not { reloadsMoreThanSingleItem: true })
            return;

        var fuel = job.targetB;
        if (fuel.Thing == null || fuel.Thing.stackCount == 1)
            return;

        job.count = Mathf.Clamp(extension.ModifyRefuelCount((Building)job.targetA.Thing, fuel.Thing), 1, fuel.Thing.stackCount);
    }
}