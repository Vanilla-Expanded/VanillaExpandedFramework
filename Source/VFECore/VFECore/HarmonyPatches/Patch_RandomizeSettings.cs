using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFECore
{
    [HarmonyPatch]
    public static class Patch_RandomizeSettings
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodInfo> TargetMethods()
        {
            return new []{
                AccessTools.Method(typeof(CompCauseGameCondition_ForceWeather), nameof(CompCauseGameCondition_ForceWeather.RandomizeSettings)),
                AccessTools.Method(typeof(GameCondition_ForceWeather),          nameof(GameCondition_ForceWeather.RandomizeSettings))
            }.SelectMany(GetFilters);
        }

        private static IEnumerable<MethodInfo> GetFilters(MethodInfo from) =>
            PatchProcessor.GetOriginalInstructions(from)
                .Where(instruction => instruction.opcode == OpCodes.Ldftn)
                .Select(instruction => (MethodInfo) instruction.operand);

        [HarmonyPostfix]
        public static void Postfix(WeatherDef __0, ref bool __result)
        {
            if (__result && __0?.GetModExtension<WeatherExtension>() is {canRandomlyGenerate: false}) __result = false;
        }
    }
}
