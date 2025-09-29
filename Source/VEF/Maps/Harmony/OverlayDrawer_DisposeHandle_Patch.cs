using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Maps;

[HarmonyPatch(typeof(OverlayDrawer), nameof(OverlayDrawer.DisposeHandle))]
public static class OverlayDrawer_DisposeHandle_Patch
{
    private static void Postfix(OverlayDrawer __instance, Thing thing) => CustomOverlayDrawer.PostDisposeHandle(__instance, thing);
}