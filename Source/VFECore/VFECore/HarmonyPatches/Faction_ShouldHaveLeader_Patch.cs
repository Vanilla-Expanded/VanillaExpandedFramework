using RimWorld;
using HarmonyLib;

namespace VFECore
{
    [HarmonyPatch(typeof(Faction), "ShouldHaveLeader", MethodType.Getter)]
    public static class Faction_ShouldHaveLeader_Patch
    {
        public static void Postfix(Faction __instance, ref bool __result)
        {
            if (__result)
            {
                var extension = __instance.def.GetModExtension<FactionDefExtension>();
                if (extension != null && !extension.shouldHaveLeader)
                {
                    __result = false;
                }
            }
        }
    }
}
