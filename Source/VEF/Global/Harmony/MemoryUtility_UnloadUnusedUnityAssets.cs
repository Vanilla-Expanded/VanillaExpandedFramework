using HarmonyLib;
using VEF.CacheClearing;
using Verse;
using Verse.Profile;

namespace VEF;

[HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.UnloadUnusedUnityAssets))]
public static class VanillaExpandedFramework_MemoryUtility_UnloadUnusedUnityAssets
{
    private static void Postfix() => ClearCaches.ClearCache();
}

[HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.ClearAllMapsAndWorld))]
public static class VanillaExpandedFramework_MemoryUtility_ClearAllMapsAndWorld
{
    private static void Postfix() => ClearCaches.ClearCache();
}
