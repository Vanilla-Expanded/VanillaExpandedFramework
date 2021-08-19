using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Reloading
{
    public class ReloadingMod : Mod
    {
        private static HarmonyMethod[] patches;

        public ReloadingMod(ModContentPack content) : base(content)
        {
            patches = new[]
            {
                new HarmonyMethod(GetType(), nameof(CheckShots)),
                new HarmonyMethod(GetType(), nameof(TryCastShot_Postfix)),
                new HarmonyMethod(GetType(), nameof(Projectile_Prefix))
            };
        }

        public static bool CheckShots(Verb __instance, ref bool __result)
        {
            var reloadable = __instance.GetReloadable();
            if (reloadable == null || reloadable.ShotsRemaining > 0) return true;
            __result = false;
            return false;
        }

        public static void TryCastShot_Postfix(Verb __instance)
        {
            __instance.GetReloadable()?.Notify_ProjectileFired();
        }

        public static bool Projectile_Prefix(Verb __instance, ref ThingDef __result)
        {
            if (__instance.GetReloadable()?.CurrentProjectile is ThingDef proj)
            {
                __result = proj;
                return false;
            }

            return true;
        }

        public static MethodInfo BestMethod(Type type, string methodName, bool useFirst)
        {
            var list = new List<MethodInfo>();
            while (type != null)
            {
                list.Add(AccessTools.Method(type, methodName));
                type = type.BaseType;
            }

            if (!useFirst) list.Reverse();

            return list.FirstOrDefault(m => m != null && m.IsDeclaredMember() && !m.IsAbstract && m.GetMethodBody() != null);
        }

        public static void RegisterVerb(Type verbType, bool patchFirstFound = false)
        {
            HarmonyPatches.Patch(BestMethod(verbType, "TryCastShot", patchFirstFound), patches[0], patches[1], "TryCastShot", verbType);
            HarmonyPatches.Patch(BestMethod(verbType, "Available", patchFirstFound), patches[0], debug_targetName: "Available", debug_targetType: verbType);
            var method = BestMethod(verbType, "get_Projectile", patchFirstFound);
            if (method != null) HarmonyPatches.Patch(method, patches[2], debug_targetName: "Projectile", debug_targetType: verbType);
        }
    }
}