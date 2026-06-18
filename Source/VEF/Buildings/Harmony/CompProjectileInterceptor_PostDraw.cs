using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(CompProjectileInterceptor), nameof(CompProjectileInterceptor.PostDraw))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_CompProjectileInterceptor_PostDraw_Patch
{
    public static bool patchActive = false;

    private static bool Prepare(MethodBase baseMethod) => patchActive;

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var matcher = new CodeMatcher(instr);
        var totalPatched = 0;

        // Include a limit of repeats in case of infinite loop errors
        for (var i = 0; i < 15; i++)
        {
            // Match "this.Props.color", should be matched twice
            matcher.MatchEndForward(
                CodeMatch.IsLdarg(0),
                CodeMatch.Calls(typeof(CompProjectileInterceptor).DeclaredPropertyGetter(nameof(CompProjectileInterceptor.Props))),
                CodeMatch.LoadsField(typeof(CompProperties_ProjectileInterceptor).DeclaredField(nameof(CompProperties_ProjectileInterceptor.color)))
            );

            if (matcher.IsValid)
            {
                matcher.InsertAfter(
                    // Insert "this"
                    CodeInstruction.LoadArgument(0),
                    // Insert our wrapper method
                    CodeInstruction.Call(() => ColorWrapper)
                );

                totalPatched++;
            }
            else
            {
                break;
            }
        }

        const int expectedPatched = 2;
        if (totalPatched != expectedPatched)
            Log.Error($"[VEF] Patched incorrect amount of instructions for {nameof(CompProjectileInterceptor)}.{nameof(CompProjectileInterceptor.PostDraw)}. Expected: {expectedPatched}, patched: {totalPatched}.");

        return matcher.Instructions();
    }

    private static Color ColorWrapper(Color color, CompProjectileInterceptor interceptor)
    {
        var extension = interceptor.parent.def.GetModExtension<ProjectileInterceptorExtension>();
        if (extension == null || extension.healthColorPoints.NullOrEmpty())
            return color;
        
        var healthPercent = (float)interceptor.currentHitPoints / interceptor.HitPointsMax;
        
        var upperPoint = extension.healthColorPoints.FirstOrDefault(p => p.healthPercent >= healthPercent);
        var lowerPoint = extension.healthColorPoints.LastOrDefault(p => p.healthPercent < healthPercent);
        
        if (upperPoint == null)
        {
            // Shouldn't be the case... but may as well be 100% safe
            if (lowerPoint == null)
                return color;
            return Color.Lerp(lowerPoint.color, color, Mathf.InverseLerp(lowerPoint.healthPercent, 1f, healthPercent));
        }
        
        if (lowerPoint == null)
            return upperPoint.color;
        return Color.Lerp(lowerPoint.color, upperPoint.color, Mathf.InverseLerp(lowerPoint.healthPercent, upperPoint.healthPercent, healthPercent));
    }
}