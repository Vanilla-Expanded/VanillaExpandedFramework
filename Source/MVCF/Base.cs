using System.Reflection;
using MVCF.Harmony;
using Verse;

namespace MVCF
{
    public class Base : Mod
    {
        public static string SearchLabel;
        public static bool Prepatcher;

        public Base(ModContentPack content) : base(content)
        {
            HarmonyLib.Harmony.DEBUG = true;
            var harm = new HarmonyLib.Harmony("legodude17.mvcf");
            harm.PatchAll(Assembly.GetExecutingAssembly());
            SearchLabel = harm.Id + Rand.Value;
            Prepatcher = ModLister.HasActiveModWithName("Prepatcher");
            if (Prepatcher) Log.Message("[MVCF] Prepatcher installed, switching");
            Compat.ApplyCompat(harm);
        }
    }
}