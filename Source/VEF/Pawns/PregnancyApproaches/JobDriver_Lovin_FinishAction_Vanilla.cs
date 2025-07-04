using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace VEF.Pawns
{
    [HarmonyPatch]
    static class VanillaExpandedFramework_JobDriver_Lovin_FinishAction_Vanilla
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.GetDeclaredMethods(typeof(JobDriver_Lovin))
                .LastOrDefault(x => x.Name.Contains("<MakeNewToils>") && x.ReturnType == typeof(void));
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (code.opcode == OpCodes.Stloc_0 && codes[i - 3].LoadsField(AccessTools.Field(typeof(ThoughtDefOf), "GotSomeLovin")))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(VanillaExpandedFramework_JobDriver_Lovin_FinishAction_Vanilla), nameof(DoLovinResult)));
                }
            }
        }

        public static void DoLovinResult(JobDriver jobDriver, ref Thought_Memory thoughtDef)
        {
            var partner = jobDriver.job.GetTarget(Verse.AI.TargetIndex.A).Pawn;
            if (jobDriver.pawn.relations.GetAdditionalPregnancyApproachData().partners.TryGetValue(partner, out var def))
            {
                def.Worker.PostLovinEffect(jobDriver.pawn, partner);
            }
        }
    }

    [HarmonyPatch]
    static class VanillaExpandedFramework_JobDriver_Lovin_FinishAction_VSIE
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            if (ModsConfig.IsActive("VanillaExpanded.VanillaSocialInteractionsExpanded"))
            {
                methodTarget = FindMethod();
                return methodTarget != null;
            }
            return false;
        }

        private static MethodInfo FindMethod()
        {
            var type = AccessTools.TypeByName("VanillaSocialInteractionsExpanded.JobDriver_LovinOneNightStand");
            if (type != null)
            {
                return AccessTools.GetDeclaredMethods(type).LastOrDefault(x => x.Name.Contains("<MakeNewToils>") && x.ReturnType == typeof(void));
            }
            Log.Error("[VEF] Failed to patch VanillaSocialInteractionsExpanded");
            return null;
        }

        [HarmonyTargetMethod]
        public static MethodBase TargetMethod() => methodTarget;

        public static MethodInfo methodTarget;
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var type = AccessTools.TypeByName("VanillaSocialInteractionsExpanded.VSIE_DefOf");
            var field = AccessTools.Field(type, "VSIE_GotSomeLovin");
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (code.opcode == OpCodes.Stloc_0 && codes[i - 3].LoadsField(field))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(VanillaExpandedFramework_JobDriver_Lovin_FinishAction_Vanilla),
                        nameof(VanillaExpandedFramework_JobDriver_Lovin_FinishAction_Vanilla.DoLovinResult)));
                }
            }
        }
    }

    [HarmonyPatch]
    static class VanillaExpandedFramework_JobDriver_Lovin_FinishAction_Highmates
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            if (ModsConfig.IsActive("vanillaracesexpanded.highmate"))
            {
                methodTarget = FindMethod();
                return methodTarget != null;
            }
            return false;
        }

        private static MethodInfo FindMethod()
        {
            var type = AccessTools.TypeByName("VanillaRacesExpandedHighmate.JobDriver_InitiateLovin");
            if (type != null)
            {
                return AccessTools.GetDeclaredMethods(type).LastOrDefault(x => x.Name.Contains("<MakeNewToils>") && x.ReturnType == typeof(void));
            }
            Log.Error("[VEF] Failed to patch VanillaRacesExpandedHighmate");
            return null;
        }


        [HarmonyTargetMethod]
        public static MethodBase TargetMethod() => methodTarget;

        public static MethodInfo methodTarget;
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                yield return code;
                if (code.opcode == OpCodes.Stloc_1 && codes[i - 3].LoadsField(AccessTools.Field(typeof(ThoughtDefOf), "GotSomeLovin")))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 1);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(VanillaExpandedFramework_JobDriver_Lovin_FinishAction_Vanilla), 
                        nameof(VanillaExpandedFramework_JobDriver_Lovin_FinishAction_Vanilla.DoLovinResult)));
                }
            }
        }
    }
}
