using HarmonyLib;
using RimWorld;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(GenStep_Power))]
    [HarmonyPatch("Generate", MethodType.Normal)]
    public class Prefix_GenStep_Power
    {
        [HarmonyPrefix]
        public static bool Prefix(Map map)
        {
            if (map.ParentFaction != null && map.ParentFaction.def.HasModExtension<CustomGenOption>())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}