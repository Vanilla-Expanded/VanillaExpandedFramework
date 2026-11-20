using HarmonyLib;
using System.Reflection;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            Harmony harmonyInstance = new Harmony("Kikohi.KCSG");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}