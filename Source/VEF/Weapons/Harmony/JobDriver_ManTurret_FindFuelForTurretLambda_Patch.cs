using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VEF.Weapons;

[HarmonyPatch]
public static class JobDriver_ManTurret_FindFuelForTurretLambda_Patch
{
    private static bool Prefix() => JobDriver_ManTurret_GunNeedsRefueling_Patch.Prepare();

    private static MethodBase TargetMethod()
    {
        var method = typeof(JobDriver_ManTurret).FindIncludingInnerTypes(t =>
        {
            // Only search for inner types
            if (t == typeof(JobDriver_ManTurret))
                return null;

            // Make sure the type has a "pawn" field
            var field = t.Field("pawn");
            if (field == null || !field.FieldType.SameOrSubclassOf<Pawn>())
                return null;

            // Make sure the type has a "refuelableComp" field
            field = t.Field("refuelableComp");
            if (field == null || !field.FieldType.SameOrSubclassOf<CompRefuelable>())
                return null;

            foreach (var method in t.GetMethods(AccessTools.all))
            {
                // Search for inner method called FuelValidator for FindFuelForTurret method.
                // Make sure it returns bool, and has a "Thing t" argument
                if (method.Name.Contains("FindFuelForTurret") &&
                    method.Name.Contains("FuelValidator") &&
                    method.ReturnType == typeof(bool) &&
                    method.GetParameters().Any(p => p.Name == "t" && p.ParameterType.SameOrSubclassOf<Thing>()))
                {
                    return method;
                }
            }

            return null;
        });

        if (method == null)
            Log.Error("[VEF] Failed to find a fuel validator for JobDriver_ManTurret:FindFuelForTurret. Reservations for pawns operating mannable turrets may break.");

        return method;
    }

    private static void Postfix(Pawn ___pawn, CompRefuelable ___refuelableComp, Thing t, ref bool __result)
    {
        // If we're not allowed (result is false), skip. Also, a few sanity checks.
        if (!__result || ___pawn == null || t == null || ___refuelableComp?.parent == null)
            return;

        // Make sure we have an extension, and it's set to reload more than 1 item.
        var extension = ___refuelableComp.parent.def.GetModExtension<AutoRefuelMannedTurrets>();
        if (extension is not { reloadsMoreThanSingleItem: true })
            return;

        var count = Mathf.Clamp(extension.ModifyRefuelCount((Building)___refuelableComp.parent, t), 1, t.stackCount);

        // Only check if count is bigger than 1, since the vanilla code just checked for count of 1.
        if (count > 1)
            __result = ___pawn.CanReserve(t, 10, count);
    }
}