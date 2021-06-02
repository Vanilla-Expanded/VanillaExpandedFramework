using HarmonyLib;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    public static class Init
    {
        static Init()
        {
            Harmony harmonyInstance = new Harmony("Kikohi.KCSG");
            harmonyInstance.PatchAll();
        }
    }
}