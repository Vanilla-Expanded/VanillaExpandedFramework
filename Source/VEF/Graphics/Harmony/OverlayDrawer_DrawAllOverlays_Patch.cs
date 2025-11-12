using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace VEF.Graphics;

[HarmonyPatch(typeof(OverlayDrawer), nameof(OverlayDrawer.DrawAllOverlays))]
public static class OverlayDrawer_DrawAllOverlays_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        // Preferably, we'd want one of the later methods to put our overlays after vanilla ones.
        // The issue is if we grab the last one. We can't move all labels from it, as then
        // we'd mess with foreach flow control. So for safety, we grab second last one.
        var nonLastRenderMethod = typeof(OverlayDrawer).DeclaredMethod("RenderRechargineOverlay");
        var ourRenderMethod = typeof(CustomOverlayDrawer).DeclaredMethod(nameof(CustomOverlayDrawer.RenderCustomOverlays));

        var isLastOverlayRender = false;

        foreach (var ci in instr)
        {
            if (isLastOverlayRender)
            {
                isLastOverlayRender = false;

                // Load this, and move all labels from current instruction to this one so this won't get jumped over.
                yield return CodeInstruction.LoadArgument(0).MoveLabelsFrom(ci);
                // Load the "key" (Thing) local
                yield return CodeInstruction.LoadLocal(4);
                // Load the "value" (OverlayTypes) local
                yield return CodeInstruction.LoadLocal(5);
                // Call our custom method
                yield return new CodeInstruction(OpCodes.Call, ourRenderMethod);
            }
            else if (ci.Calls(nonLastRenderMethod))
            {
                isLastOverlayRender = true;
            }

            yield return ci;
        }
    }
}