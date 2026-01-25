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
        var actor = ___refuelIfNeeded.actor;
        var job = actor.CurJob;
        var turret = job.targetA.Thing;

        var extension = turret?.def?.GetModExtension<AutoRefuelMannedTurrets>();
        // Check if extension is null, or we only want to reload a single item
        if (extension is not { reloadsMoreThanSingleItem: true })
            return;

        var fuel = job.targetB;
        if (fuel.Thing == null || fuel.Thing.stackCount == 1)
            return;

        var count = Mathf.Clamp(extension.ModifyRefuelCount((Building)turret, fuel.Thing), 1, fuel.Thing.stackCount);

        // We only need to handle changing count and reserving stuff if count is bigger than 1.
        // Vanilla will handle counts of 1.
        if (count <= 1)
            return;

        job.count = count;
        if (!actor.Reserve(fuel, actor.CurJob, 10, count))
        {
            actor.jobs.EndCurrentJob(JobCondition.Incompletable);
            Messages.Message("MessageOutOfNearbyFuelFor".Translate(actor.LabelShort, turret.Label, actor.Named("PAWN"), turret.Named("GUN"), turret.TryGetComp<CompRefuelable>().Props.fuelFilter.Summary.Named("FUEL")).CapitalizeFirst(), turret, MessageTypeDefOf.NegativeEvent);
        }
    }
}