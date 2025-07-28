using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VEF.Pawns;

[HarmonyPatch]
public static class VanillaExpandedFramework_JobDriver_Lovin_MoveNext_Patch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.EnumeratorMoveNext(typeof(JobDriver_Lovin).DeclaredMethod("MakeNewToils"));

        if (ModsConfig.IsActive("VanillaExpanded.VanillaSocialInteractionsExpanded"))
        {
            var type = AccessTools.TypeByName("VanillaSocialInteractionsExpanded.JobDriver_LovinOneNightStand");
            var method = AccessTools.EnumeratorMoveNext(type.DeclaredMethod("MakeNewToils"));
            if (method == null)
                Log.Error("[VEF] Failed to patch VanillaSocialInteractionsExpanded");
            else
                yield return method;
        }

        if (ModsConfig.IsActive("vanillaracesexpanded.highmate"))
        {
            var type = AccessTools.TypeByName("VanillaRacesExpandedHighmate.JobDriver_InitiateLovin");
            var method = AccessTools.EnumeratorMoveNext(type.DeclaredMethod("MakeNewToils"));
            if (method == null)
                Log.Error("[VEF] Failed to patch VanillaRacesExpandedHighmate");
            else
                yield return method;
        }
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method)
    {
        var targetToilMethod = method.DeclaringType?.Namespace == "VanillaRacesExpandedHighmate"
            ? AccessTools.DeclaredMethod(typeof(Toils_General), nameof(Toils_General.Wait)) // Highmates initiale lovin' uses wait toil
            : AccessTools.DeclaredMethod(typeof(Toils_LayDown), nameof(Toils_LayDown.LayDown));

        var patched = false;
        var foundMethod = false;

        foreach (var ci in instructions)
        {
            if (!patched)
            {
                if (foundMethod)
                {
                    if (ci.opcode == OpCodes.Ret)
                    {
                        patched = true;

                        yield return CodeInstruction.LoadArgument(0);
                        yield return CodeInstruction.LoadField(method.DeclaringType, "<>4__this");
                        yield return CodeInstruction.LoadArgument(0);
                        yield return CodeInstruction.LoadField(method.DeclaringType, "<>2__current");
                        yield return CodeInstruction.Call(typeof(VanillaExpandedFramework_JobDriver_Lovin_MoveNext_Patch), nameof(ModifyToil));
                    }
                }
                else if (ci.Calls(targetToilMethod))
                {
                    foundMethod = true;
                }
            }

            yield return ci;
        }

        if (!patched)
        {
            var mod = method.DeclaringType?.Namespace switch
            {
                nameof(RimWorld) => "vanilla",
                "VanillaSocialInteractionsExpanded" => "VanillaSocialInteractionsExpanded",
                "VanillaRacesExpandedHighmate" => "VanillaRacesExpandedHighmate",
                _ => "unknown mod"
            };

            Log.Error($"[VEF] Failed to patch {mod}");
        }
    }

    private static void ModifyToil(JobDriver jobDriver, Toil toil)
    {
        var partner = jobDriver.job.GetTarget(TargetIndex.A).Pawn;
        if (jobDriver.pawn.relations.GetAdditionalPregnancyApproachData().partners.TryGetValue(partner, out var def))
        {
            def.Worker.ModifyLovinToil(toil, jobDriver.pawn, partner);
        }
    }
}