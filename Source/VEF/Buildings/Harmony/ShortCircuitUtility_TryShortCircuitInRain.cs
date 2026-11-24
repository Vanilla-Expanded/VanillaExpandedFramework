using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(ShortCircuitUtility), nameof(ShortCircuitUtility.TryShortCircuitInRain))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_ShortCircuitUtility_TryShortCircuitInRain_Patch
{
    public static bool patchActive = false;

    private static bool Prepare() => patchActive;

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator generator)
    {
        var inst = instr.ToList();
        var local = generator.DeclareLocal(typeof(CompCustomShortCircuitInRain));
        var firstInstr = generator.DefineLabel();
        inst[0].labels.Add(firstInstr);

        // Load thing argument
        yield return CodeInstruction.LoadArgument(0);
        // Get our method to grab our interface
        yield return CodeInstruction.Call(typeof(ThingCompUtility), nameof(ThingCompUtility.TryGetComp), [typeof(Thing)], [typeof(CompCustomShortCircuitInRain)]);
        // Store our value
        yield return CodeInstruction.StoreLocal(local.LocalIndex);
        // Load our value
        yield return CodeInstruction.LoadLocal(local.LocalIndex);
        // Jump to the normal first instruction if null
        yield return new CodeInstruction(OpCodes.Brfalse_S, firstInstr);
        // Load our value again
        yield return CodeInstruction.LoadLocal(local.LocalIndex);
        // Call our method to check if we have custom handling of the short circuit
        yield return new CodeInstruction(OpCodes.Callvirt, typeof(CompCustomShortCircuitInRain).DeclaredPropertyGetter(nameof(CompCustomShortCircuitInRain.VanillaShortCircuit)));
        // If vanilla short circuit, jump over custom code
        yield return new CodeInstruction(OpCodes.Brtrue_S, firstInstr);
        // Load our value yet again
        yield return CodeInstruction.LoadLocal(local.LocalIndex);
        // Call our method to trigger a custom short circuit
        yield return new CodeInstruction(OpCodes.Callvirt, typeof(CompCustomShortCircuitInRain).DeclaredMethod(nameof(CompCustomShortCircuitInRain.TriggerCustomShortCircuit)));
        // Return with the value we were given
        yield return new CodeInstruction(OpCodes.Ret);

        CodeInstruction branchInstr = null;
        var patched = new List<CodeInstruction>();

        foreach (var ci in inst)
        {
            if (ci.opcode == OpCodes.Ldloc_0 && branchInstr == null)
            {
                var nextInstrLabel = generator.DefineLabel();
                ci.labels.Add(nextInstrLabel);
                // Load our value
                patched.Add(CodeInstruction.LoadLocal(local.LocalIndex));
                // Jump over our next instructions if null
                patched.Add(new CodeInstruction(OpCodes.Brfalse_S, nextInstrLabel));
                // Load our value
                patched.Add(CodeInstruction.LoadLocal(local.LocalIndex));
                // Call our property
                patched.Add(new CodeInstruction(OpCodes.Callvirt, typeof(CompCustomShortCircuitInRain).PropertyGetter(nameof(CompCustomShortCircuitInRain.CanVanillaShortCircuitNow))));
                // Jump to the short circuit code (we add label later)
                branchInstr = new CodeInstruction(OpCodes.Brtrue_S);
                patched.Add(branchInstr);
                // Return false (nothing triggered)
                patched.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                patched.Add(new CodeInstruction(OpCodes.Ret));
            }
            else if (ci.opcode == OpCodes.Ldstr && ci.operand is "ShortCircuitRain" && branchInstr is { operand: null })
            {
                var shortCircuitLabel = generator.DefineLabel();
                ci.labels.Add(shortCircuitLabel);
                branchInstr.operand = shortCircuitLabel;
            }

            patched.Add(ci);
        }

        // If this part failed, skip related patches but let custom short circuit code in.
        if (branchInstr?.operand == null)
            Log.Error("[VEF] Failed patching ShortCircuitUtility.TryShortCircuitInRain. Custom short circuits (VanillaShortCircuit == false) will still work, but CanVanillaShortCircuitNow will be ignored.");
        // If everything patched fine, use all patched instructions
        else
            inst = patched;

        foreach (var codeInstruction in inst)
            yield return codeInstruction;
    }
}