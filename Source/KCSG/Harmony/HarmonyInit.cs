using HarmonyLib;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            Harmony harmonyInstance = new Harmony("Kikohi.KCSG");
            harmonyInstance.PatchAll();
        }
    }
}