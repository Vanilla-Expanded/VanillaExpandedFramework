using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Graphics;

[HarmonyPatch(typeof(OverlayDrawer), nameof(OverlayDrawer.DisposeHandle))]
public static class OverlayDrawer_DisposeHandle_Patch
{
    private static void Postfix(OverlayDrawer __instance, Thing thing) => CustomOverlayDrawer.PostDisposeHandle(__instance, thing);
}