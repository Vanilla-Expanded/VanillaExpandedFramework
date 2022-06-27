using HarmonyLib;
using Verse;

namespace PipeSystem
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            Harmony harmonyInstance = new Harmony("Kikohi.PipeSystem");
            harmonyInstance.PatchAll();
        }
    }
}