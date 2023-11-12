using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace VFECore
{
    [HarmonyPatch]
    public static class JobDriver_Lovin_Patch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(JobDriver_Lovin).GetMethods(AccessTools.all).FirstOrFallback(x => x.Name.Contains("<MakeNewToils>b__12_1"));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var setTicks = AccessTools.Field(typeof(JobDriver_Lovin), "ticksLeft");
            bool patched = false;
            foreach (var code in instructions)
            {
                if (patched is false && code.StoresField(setTicks))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JobDriver_Lovin_Patch), "SetLovinDuration"));
                    patched = true;
                }
                yield return code;
            }
        }

        public static int SetLovinDuration(int ticksLeft, JobDriver_Lovin jobDriver_Lovin)
        {
            var partner = jobDriver_Lovin.job.GetTarget(TargetIndex.A).Pawn;
            if (jobDriver_Lovin.pawn.relations.GetAdditionalPregnancyApproachData().partners.TryGetValue(partner, out var def))
            {
                ticksLeft = (int)(ticksLeft * def.lovinDurationMultiplier);
            }
            return ticksLeft;
        }
    }
}
