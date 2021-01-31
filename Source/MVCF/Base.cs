using System.Reflection;
using MVCF.Harmony;
using Verse;

namespace MVCF
{
    public class Base : Mod
    {
        public static string SearchLabel;
        public static bool Prepatcher;
        public static bool LimitedMode;

        public Base(ModContentPack content) : base(content)
        {
            var harm = new HarmonyLib.Harmony("legodude17.mvcf");
            SearchLabel = harm.Id + Rand.Value;
            Prepatcher = ModLister.HasActiveModWithName("Prepatcher");
            if (Prepatcher) Log.Message("[MVCF] Prepatcher installed, switching");
            Compat.ApplyCompat(harm);
            if (!LimitedMode) harm.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}