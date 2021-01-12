using System.Reflection;
using Verse;

namespace MVCF
{
    public class Base : Mod
    {
        public static string SearchLabel;
        public static bool Prepatcher;

        public Base(ModContentPack content) : base(content)
        {
            var harm = new HarmonyLib.Harmony("legodude17.mvcf");
            harm.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("Applied patches for " + harm.Id);
            SearchLabel = harm.Id + Rand.Value;
            Prepatcher = ModLister.HasActiveModWithName("Prepatcher");
            Log.Message("[MVCF] Prepatcher installed: " + Prepatcher);
        }
    }
}