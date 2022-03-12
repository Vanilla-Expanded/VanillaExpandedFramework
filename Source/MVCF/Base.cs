using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MVCF.Features;
using Verse;

namespace MVCF
{
    [StaticConstructorOnStartup]
    public class Base : Mod
    {
        public static string SearchLabel;
        private static readonly HashSet<Patch> appliedPatches = new();
        public static bool Prepatcher;
        public static List<Feature> AllFeatures;
        public static HashSet<string> EnabledFeatures = new();

        [Obsolete] public static FeatureOpts Features = new();

        public static Harmony Harm;

        [Obsolete] public static FeatureOpts IgnoredFeatures = new();

        public static readonly HashSet<string> IgnoredMods = new()
        {
            "Misc. Robots++",
            "Dragon's Descent",
            "[RWY]Dragon's Descent: Void Dwellers",
            "Genetic Rim",
            "Cybernetic Warfare and Special Weapons (Continued)",
            "Cybernetic Warfare and Special Weapons"
        };

        private static Dictionary<Type, Feature> features;

        public Base(ModContentPack content) : base(content)
        {
            Harm = new Harmony("legodude17.mvcf");
            SearchLabel = Harm.Id + Rand.Value;
            Prepatcher = ModLister.HasActiveModWithName("Prepatcher");
            if (Prepatcher) Log.Message("[MVCF] Prepatcher installed, switching");
            LongEventHandler.ExecuteWhenFinished(CollectFeatureData);
            AllFeatures = typeof(Feature).AllSubclassesNonAbstract().Select(type => (Feature) Activator.CreateInstance(type)).ToList();
            features = AllFeatures.ToDictionary(f => f.GetType());
        }

        public static Feature GetFeature<T>() where T : Feature => features[typeof(T)];

        public static bool ShouldIgnore(Thing thing)
        {
            return thing is ThingWithComps twc &&
                   twc.AllComps.Any(comp => comp.GetType().Name.Contains("ToggleFireMode"));
        }

        public static void CollectFeatureData()
        {
            Log.Message("[MVCF] Collecting feature data...");
            foreach (var def in DefDatabase<ModDef>.AllDefs)
            {
                if (def.Features != null)
                {
                    if (def.Features.ApparelVerbs) Features.ApparelVerbs = true;
                    if (def.Features.IndependentFire) Features.IndependentFire = true;
                    if (def.Features.Drawing) Features.Drawing = true;
                    if (def.Features.ExtraEquipmentVerbs) Features.ExtraEquipmentVerbs = true;
                    if (def.Features.HediffVerbs) Features.HediffVerbs = true;
                    if (def.Features.RangedAnimals) Features.RangedAnimals = true;
                    if (def.Features.IntegratedToggle) Features.IntegratedToggle = true;
                    if (def.Features.TickVerbs) Features.TickVerbs = true;
                }

                foreach (var feature in def.ActivateFeatures)
                {
                    Log.Message($"[MVCF] Mod {def.modContentPack.Name} is enabling feature {feature}");
                    EnabledFeatures.Add(feature);
                }

                if (def.IgnoreThisMod)
                {
                    Log.Message($"[MVCF] Ignoring {def.modContentPack.Name}");
                    IgnoredMods.Add(def.modContentPack.Name);
                }
            }

            foreach (var feature in EnabledFeatures.SelectMany(f => AllFeatures.Where(feature => feature.Name == f)))
            {
                Log.Message($"[MVCF] Applying patches for feature {feature.Name}");
                EnableFeature(feature);
            }

            Patch.PrintSummary();
        }

        public static void EnableFeature(Feature feature)
        {
            feature.Enabled = true;
            foreach (var patch in feature.Patches) ApplyPatch(patch);
        }

        public static void DisableFeature(Feature feature)
        {
            feature.Enabled = false;
            foreach (var patch in feature.Patches.Except(AllFeatures.Where(f => f.Enabled).SelectMany(f => f.Patches))) UnapplyPatch(patch);
        }

        public static void ApplyPatch(Patch patch)
        {
            if (appliedPatches.Contains(patch)) return;
            patch.Apply(Harm);
            appliedPatches.Add(patch);
        }

        public static void UnapplyPatch(Patch patch)
        {
            if (!appliedPatches.Contains(patch)) return;
            patch.Unapply(Harm);
            appliedPatches.Remove(patch);
        }

        public static bool IsIgnoredMod(string name) => name != null && IgnoredMods.Contains(name);
    }

    public class ModDef : Def
    {
        public List<string> ActivateFeatures = new();
        [Obsolete] public FeatureOpts Features;

        [Obsolete] public FeatureOpts IgnoredFeatures;

        public bool IgnoreThisMod;

        public override IEnumerable<string> ConfigErrors()
        {
            if (Features is not null) yield return "<Features> is deprecated, use <ActivateFeatures>";
            if (IgnoredFeatures is not null) yield return "<IgnoredFeatures> is deprecated";
        }

        public override void PostLoad()
        {
            base.PostLoad();

            #region BackCompatability

            if (Features is not null)
            {
                if (Features.ApparelVerbs) ActivateFeatures.Add("ApparelVerbs");
                if (Features.Drawing) ActivateFeatures.Add("Drawing");
                if (Features.ExtraEquipmentVerbs) ActivateFeatures.Add("ExtraEquipmentVerbs");
                if (Features.HediffVerbs) ActivateFeatures.Add("HediffVerbs");
                if (Features.IndependentFire) ActivateFeatures.Add("IndependentFire");
                if (Features.IntegratedToggle) ActivateFeatures.Add("IntegratedToggle");
                if (Features.RangedAnimals) ActivateFeatures.Add("RangedAnimals");
                if (Features.TickVerbs) ActivateFeatures.Add("TickVerbs");
            }

            #endregion
        }
    }

    [Obsolete]
    public class FeatureOpts
    {
        public bool ApparelVerbs;
        public bool Drawing;
        public bool ExtraEquipmentVerbs;
        public bool HediffVerbs;
        public bool IndependentFire;
        public bool IntegratedToggle;
        public bool RangedAnimals;
        public bool TickVerbs;
        public bool EnabledAtAll => HediffVerbs || ExtraEquipmentVerbs || ApparelVerbs || RangedAnimals;
        public bool HumanoidVerbs => HediffVerbs || ExtraEquipmentVerbs || ApparelVerbs;
    }

    public abstract class PatchSet
    {
        public abstract IEnumerable<Patch> GetPatches();
    }

    public struct Patch
    {
        private static int numPatches;
        private readonly MethodBase target;
        private readonly MethodInfo prefix;
        private readonly MethodInfo postfix;
        private readonly MethodInfo transpiler;

        public override string ToString() =>
            $"{target.FullDescription()} with:\nPrefix: {prefix.FullDescription()}\nPostfix: {postfix.FullDescription()}\nTranspiler: {transpiler.FullDescription()}";

        public static void PrintSummary()
        {
            Log.Message($"MVCF successfully applied {numPatches} patches");
        }

        public static Patch Prefix(MethodBase target, MethodInfo prefix) => new(target, prefix);
        public static Patch Postfix(MethodBase target, MethodInfo postfix) => new(target, postfix: postfix);
        public static Patch Transpiler(MethodBase target, MethodInfo transpiler) => new(target, transpiler: transpiler);

        public Patch(MethodBase target, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            this.target = target;
            this.prefix = prefix;
            this.postfix = postfix;
            this.transpiler = transpiler;
        }

        public void Apply(Harmony harm)
        {
            if (Prefs.DevMode) Log.Message($"[MVCF] Patching {this}");
            harm.Patch(target,
                prefix is null ? null : new HarmonyMethod(prefix),
                postfix is null ? null : new HarmonyMethod(postfix),
                transpiler is null ? null : new HarmonyMethod(transpiler));
            numPatches++;
        }

        public void Unapply(Harmony harm)
        {
            if (Prefs.DevMode) Log.Message($"[MVCF] Unpatching {this}");
            if (prefix is not null) harm.Unpatch(target, prefix);

            if (postfix is not null) harm.Unpatch(target, postfix);

            if (transpiler is not null) harm.Unpatch(target, transpiler);
        }
    }
}