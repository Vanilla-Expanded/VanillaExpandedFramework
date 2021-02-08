using System.Collections.Generic;
using MVCF.Harmony;
using Verse;

namespace MVCF
{
    [StaticConstructorOnStartup]
    public class Base : Mod
    {
        public static string SearchLabel;
        public static bool Prepatcher;
        public static FeatureOpts Features = new FeatureOpts();
        public static HarmonyLib.Harmony Harm;
        public static FeatureOpts IgnoredFeatures = new FeatureOpts();

        public static readonly List<string> IgnoredMods = new List<string>
        {
            "Adeptus Mechanicus: Armoury",
            "Dragon's Descent",
            "[RWY]Dragon's Descent: Void Dwellers",
            "Genetic Rim",
            "Cybernetic Warfare and Special Weapons (Continued)",
            "Cybernetic Warfare and Special Weapons"
        };

        public Base(ModContentPack content) : base(content)
        {
            Harm = new HarmonyLib.Harmony("legodude17.mvcf");
            SearchLabel = Harm.Id + Rand.Value;
            Prepatcher = ModLister.HasActiveModWithName("Prepatcher");
            if (Prepatcher) Log.Message("[MVCF] Prepatcher installed, switching");
            Trackers.Apparel(Harm);
            Trackers.Equipment(Harm);
            Trackers.Hediffs(Harm);
            LongEventHandler.ExecuteWhenFinished(CollectFeatureData);
            if (ModLister.HasActiveModWithName("Prosthetic Combat Framework")) IgnoredFeatures.HediffVerbs = true;
        }


        public static void CollectFeatureData()
        {
            foreach (var def in DefDatabase<ModDef>.AllDefs)
            {
                if (def.Features.ApparelVerbs) Features.ApparelVerbs = true;
                if (def.Features.IndependentFire) Features.IndependentFire = true;
                if (def.Features.Drawing) Features.Drawing = true;
                if (def.Features.ExtraEquipmentVerbs) Features.ExtraEquipmentVerbs = true;
                if (def.Features.HediffVerbs) Features.HediffVerbs = true;
                if (def.Features.RangedAnimals) Features.RangedAnimals = true;
                if (def.Features.IntegratedToggle) Features.IntegratedToggle = true;
            }

            ApplyPatches();
        }

        public static bool IsIgnoredMod(string name)
        {
            return name != null && IgnoredMods.Contains(name);
        }

        public static void ApplyPatches()
        {
            if (Features.EnabledAtAll)
            {
                Compat.ApplyCompat(Harm);
                Pawn_TryGetAttackVerb.DoPatches(Harm);
                VerbUtilityPatches.DoPatches(Harm);
                MiscPatches.DoLogPatches(Harm);
                VerbPatches.DoPatches(Harm);
            }

            if (Features.HumanoidVerbs)
            {
                Brawlers.DoPatches(Harm);
                Hunting.DoPatches(Harm);
                Gizmos.DoHumanoidPatches(Harm);
            }

            if (Features.ExtraEquipmentVerbs) Gizmos.DoExtraEquipmentPatches(Harm);

            if (Features.RangedAnimals)
            {
                Gizmos.DoAnimalPatches(Harm);
                MiscPatches.DoAnimalPatches(Harm);
            }

            if (Features.IntegratedToggle) Gizmos.DoIntegratedTogglePatches(Harm);

            if (Features.IndependentFire)
            {
                VerbPatches.DoIndependentPatches(Harm);
                MiscPatches.DoIndependentPatches(Harm);
            }

            if (Features.Drawing) MiscPatches.DoDrawPatches(Harm);
        }
    }

    public class ModDef : Def
    {
        public FeatureOpts Features;
    }

    public class FeatureOpts
    {
        public bool ApparelVerbs;
        public bool Drawing;
        public bool ExtraEquipmentVerbs;
        public bool HediffVerbs;
        public bool IndependentFire;
        public bool IntegratedToggle;
        public bool RangedAnimals;
        public bool EnabledAtAll => HediffVerbs || ExtraEquipmentVerbs || ApparelVerbs || RangedAnimals;
        public bool HumanoidVerbs => HediffVerbs || ExtraEquipmentVerbs || ApparelVerbs;
    }
}