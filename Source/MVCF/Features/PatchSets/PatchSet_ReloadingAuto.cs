namespace MVCF.Features.PatchSets
{
    // public class PatchSet_ReloadingAuto : PatchSet
    // {
    //     private static readonly MethodInfo[] patches =
    //     {
    //         AccessTools.Method(typeof(PatchSet_ReloadingAuto), nameof(CheckShots)),
    //         AccessTools.Method(typeof(PatchSet_ReloadingAuto), nameof(TryCastShot_Postfix)),
    //         AccessTools.Method(typeof(PatchSet_ReloadingAuto), nameof(Projectile_Prefix))
    //     };
    //
    //     public static HashSet<Patch> Patches;
    //
    //     public static bool CheckShots(Verb __instance, ref bool __result)
    //     {
    //         var reloadable = __instance.GetReloadable();
    //         if (reloadable == null || reloadable.ShotsRemaining > 0) return true;
    //         __result = false;
    //         return false;
    //     }
    //
    //     public override IEnumerable<Patch> GetPatches() => Patches;
    //
    //     public static void TryCastShot_Postfix(Verb __instance)
    //     {
    //         __instance.GetReloadable()?.Notify_ProjectileFired();
    //     }
    //
    //     public static bool Projectile_Prefix(Verb __instance, ref ThingDef __result)
    //     {
    //         if (__instance.GetReloadable()?.CurrentProjectile is ThingDef proj)
    //         {
    //             __result = proj;
    //             return false;
    //         }
    //
    //         return true;
    //     }
    //
    //     public static MethodInfo BestMethod(Type type, string methodName, bool useFirst)
    //     {
    //         var list = new List<MethodInfo>();
    //         while (type != null)
    //         {
    //             list.Add(AccessTools.Method(type, methodName));
    //             type = type.BaseType;
    //         }
    //
    //         if (!useFirst) list.Reverse();
    //
    //         return list.FirstOrDefault(m => m != null && m.IsDeclaredMember() && !m.IsAbstract && m.GetMethodBody() != null);
    //     }
    //
    //     public static void RegisterVerb(Type verbType, bool patchFirstFound = false)
    //     {
    //         Patches.Add(new Patch(BestMethod(verbType, "TryCastShot", patchFirstFound), patches[0], patches[1]));
    //         Patches.Add(new Patch(BestMethod(verbType, "Available", patchFirstFound), patches[0]));
    //         var method = BestMethod(verbType, "get_Projectile", patchFirstFound);
    //         if (method != null) Patches.Add(new Patch(method, patches[2]));
    //     }
    // }
}