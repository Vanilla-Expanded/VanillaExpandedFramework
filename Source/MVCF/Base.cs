using MVCF.Harmony;
using Verse;

namespace MVCF
{
    public class Base : Mod
    {
        public static string SearchLabel;
        public static bool Prepatcher;
        public FeatureOpts Features;

        public Base(ModContentPack content) : base(content)
        {
            var harm = new HarmonyLib.Harmony("legodude17.mvcf");
            SearchLabel = harm.Id + Rand.Value;
            Prepatcher = ModLister.HasActiveModWithName("Prepatcher");
            if (Prepatcher) Log.Message("[MVCF] Prepatcher installed, switching");
            Compat.ApplyCompat(harm);
            Brawlers.DoPatches(harm);
            Hunting.DoPatches(harm);
            Gizmos.DoPatches(harm);
            MiscPatches.DoPatches(harm);
            VerbPatches.DoPatches(harm);
            Pawn_TryGetAttackVerb.DoPatches(harm);
            Trackers.Apparel(harm);
            Trackers.Equipment(harm);
            Trackers.Hediffs(harm);
            VerbUtilityPatches.DoPatches(harm);
        }
    }

    public struct FeatureOpts
    {
        public bool IndependentFire;
        public bool Drawing;
        public bool HediffVerbs;
        public bool ExtraEquipmentVerbs;
        public bool ApparelVerbs;
        public bool RangedAnimals;
    }
}