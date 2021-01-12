using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MVCF.Utilities;
using Verse;

// ReSharper disable InconsistentNaming

namespace MVCF.Harmony
{
//    [HarmonyPatch]
    public class DebugBool
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Verb), "TryStartCastOn",
                new[] {typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(bool), typeof(bool)});
            yield return AccessTools.Method(typeof(Verb_Shoot), "TryCastShot");
            yield return AccessTools.Method(typeof(Verb), "CanHitTarget");
            yield return AccessTools.Method(typeof(Verb), "TryFindShootLineFromTo");
        }

        public static void Prefix(MethodBase __originalMethod, object __instance, int ___ticksToNextBurstShot)
        {
            Log.Message(__instance + "." + __originalMethod.Name + "()");
            if (__instance is Verb verb)
            {
                Log.Message("    Verb:");
                Log.Message("        Label: " + verb.Label());
                Log.Message("        Caster: " + verb.caster);
                Log.Message("        State: " + verb.state);
                Log.Message("        Ticks to next burst shot: " + ___ticksToNextBurstShot);
            }
        }

        public static void Postfix(MethodBase __originalMethod, object __instance, bool __result)
        {
            Log.Message(__instance + "." + __originalMethod.Name + "()" + " -> " + __result);
        }
    }

//    [HarmonyPatch]
    public class DebugVoid
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Pawn), "TryStartAttack");
            yield return AccessTools.Method(typeof(Verb_LaunchProjectile), "WarmupComplete");
        }

        public static void Prefix(MethodBase __originalMethod, object __instance)
        {
            Log.Message(__instance + "." + __originalMethod.Name + "()");
            if (__instance is Verb verb)
            {
                Log.Message("    Verb:");
                Log.Message("        Label: " + verb.Label());
                Log.Message("        Caster: " + verb.caster);
                Log.Message("        State: " + verb.state);
            }

            if (__instance is Verb_LaunchProjectile proj) Log.Message("        Projectile: " + proj.Projectile.label);
        }

        public static void Postfix(MethodBase __originalMethod, object __instance)
        {
            Log.Message(__instance + "." + __originalMethod.Name + "() -> void");
        }
    }

    // [HarmonyPatch(typeof(Verb_LaunchProjectile), "get_Projectile")]
    public class Debug
    {
        public static void Prefix(Verb __instance)
        {
            Log.Message("get_Projectile: " + __instance?.Label());
        }
    }
}