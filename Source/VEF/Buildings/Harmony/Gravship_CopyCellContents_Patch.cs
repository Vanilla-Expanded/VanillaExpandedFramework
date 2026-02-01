using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(Gravship), "CopyCellContents")]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_Gravship_CopyCellContents_Patch
{
    private static bool Prepare(MethodBase method)
    {
        // Only apply the patch if there's any building with multi-cell place worker.
        // Requires late patching.
        return method != null || DefDatabase<ThingDef>.AllDefs.Any(x => x.placeWorkers?.Any(t => t != null && t.SameOrSubclassOf<PlaceWorker_AttachedToWallMultiCell>()) == true);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var intVec3Addition = typeof(IntVec3).DeclaredMethod("op_Addition");
        var matcher = new CodeMatcher(instr);

        // Search for the index to which we set the current IntVec3 from the engineFloors HashSet
        matcher.MatchEndForward(
            // HashSet is loaded using Ldloca, rather than Ldloc, so we need to set "useAddress" to true.
            CodeMatch.LoadsLocal(true),
            CodeMatch.Calls(typeof(HashSet<IntVec3>.Enumerator).DeclaredPropertyGetter("Current")),
            CodeMatch.StoresLocal()
        );

        var realPositionIndex = matcher.Instruction.LocalIndex();

        // Search for the index to which we set the current Thing from the GenConstruct.GetAttachedBuildings results 
        matcher.MatchEndForward(
            // HashSet is loaded using Ldloca, rather than Ldloc, so we need to set "useAddress" to true.
            CodeMatch.LoadsLocal(true),
            CodeMatch.Calls(typeof(List<Thing>.Enumerator).DeclaredPropertyGetter("Current")),
            CodeMatch.StoresLocal()
        );

        var attachmentThingIndex = matcher.Instruction.LocalIndex();

        // Search for the code adding attachments to the things list
        matcher.MatchEndForward(
            CodeMatch.Calls(typeof(Rot4).DeclaredPropertyGetter(nameof(Rot4.FacingCell))),
            // CodeMatch.Calls(() => default(Rot4).FacingCell),
            CodeMatch.Calls(intVec3Addition),
            CodeMatch.Calls(typeof(Gravship).DeclaredMethod("AddThing", [typeof(Thing), typeof(IntVec3)]))
        );
        // Move 1 position back, we want to insert before the "AddThing" method
        matcher.Advance(-1);

        // Call our method (with arguments) and add the result to the offset.
        // We return IntVec3.Zero, which is basically going to be a noop.
        // The game will still try to add this specific thing multiple times, but it checks for duplicates after.
        matcher.InsertAfter(
            CodeInstruction.LoadLocal(attachmentThingIndex), // 12
            CodeInstruction.LoadLocal(realPositionIndex), // 4
            CodeInstruction.Call(() => ExtraOffset),
            new CodeInstruction(OpCodes.Call, intVec3Addition)
        );

        return matcher.Instructions();
    }

    private static IntVec3 ExtraOffset(Thing thing, IntVec3 checkedCell)
    {
        if (!thing.def.PlaceWorkers.OfType<PlaceWorker_AttachedToWallMultiCell>().Any())
            return IntVec3.Zero;

        return thing.Position - (checkedCell + thing.Rotation.Opposite.FacingCell);
    }
}