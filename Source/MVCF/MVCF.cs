using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MVCF.Features;
using UnityEngine;
using Verse;

namespace MVCF;

[StaticConstructorOnStartup]
// ReSharper disable once InconsistentNaming
public class MVCF : Mod
{
    private static readonly HashSet<Patch> appliedPatches = new();
    public static List<Feature> AllFeatures;
    public static HashSet<string> EnabledFeatures = new();

    public static Harmony Harm;

    public static readonly HashSet<string> IgnoredMods = new()
    {
        "Misc. Robots++",
        "Cybernetic Warfare and Special Weapons (Continued)",
        "Cybernetic Warfare and Special Weapons"
    };

    private static Dictionary<Type, Feature> features;

    public static bool DebugMode;
    public static LogLevel LogLevel = LogLevel.Important;

    public MVCF(ModContentPack content) : base(content)
    {
        Harm = new Harmony("legodude17.mvcf");
        LongEventHandler.ExecuteWhenFinished(CollectFeatureData);
        AllFeatures = typeof(Feature).AllSubclassesNonAbstract().Select(type => (Feature)Activator.CreateInstance(type)).ToList();
        features = AllFeatures.ToDictionary(f => f.GetType());
        GetSettings<MVCFSettings>();
    }

    public override string SettingsCategory() => "MVCF.Setting".Translate();

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        listing.CheckboxLabeled("MVCF.Settings.Debug".Translate(), ref DebugMode);
        if (DebugMode)
        {
            if (listing.ButtonTextLabeled("MVCF.Settings.LogLevel".Translate(), $"MVCF.Settings.LogLevel.{LogLevel}".Translate()))
                Find.WindowStack.Add(new FloatMenu(Enum.GetValues(typeof(LogLevel))
                   .Cast<LogLevel>()
                   .Select(level => new FloatMenuOption(
                        $"MVCF.Settings.LogLevel.{level}".Translate(), () => LogLevel = level))
                   .ToList()));

            var features = listing.BeginSection(AllFeatures.Count * 33f);
            foreach (var feature in AllFeatures)
            {
                var enabled = feature.Enabled;
                features.CheckboxLabeled(feature.Name, ref enabled);
                if (enabled != feature.Enabled)
                {
                    if (enabled) EnableFeature(feature);
                    else DisableFeature(feature);
                }
            }

            listing.EndSection(features);
        }

        listing.End();
    }

    public static void Log(string message, LogLevel level = LogLevel.Verbose)
    {
        if (DebugMode && level <= LogLevel) Verse.Log.Message("[MVCF] " + message);
    }

    public static void LogFormat(FormattableString message, LogLevel level = LogLevel.Verbose)
    {
        if (DebugMode && level <= LogLevel) Verse.Log.Message("[MVCF] " + message);
    }

    public static T GetFeature<T>() where T : Feature => (T)features[typeof(T)];

    public static bool ShouldIgnore(Thing thing)
    {
        return thing is ThingWithComps twc &&
               twc.AllComps.Any(comp => comp.GetType().Name.Contains("ToggleFireMode"));
    }

    public static void CollectFeatureData()
    {
        Log("Collecting feature data...", LogLevel.Important);
        foreach (var def in DefDatabase<ModDef>.AllDefs)
        {
            foreach (var feature in def.ActivateFeatures)
            {
                LogFormat($"Mod {def.modContentPack.Name} is enabling feature {feature}", LogLevel.Important);
                EnabledFeatures.Add(feature);
            }

            if (def.IgnoreThisMod)
            {
                LogFormat($"Ignoring {def.modContentPack.Name}", LogLevel.Important);
                IgnoredMods.Add(def.modContentPack.Name);
            }
        }

        LongEventHandler.ExecuteWhenFinished(() =>
        {
            foreach (var feature in EnabledFeatures.SelectMany(f => AllFeatures.Where(feature => feature.Name == f)))
            {
                LogFormat($"Applying patches for feature {feature.Name}", LogLevel.Important);
                EnableFeature(feature);
            }

            Patch.PrintSummary();
        });
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

    public bool IgnoreThisMod;

    public override IEnumerable<string> ConfigErrors()
    {
#pragma warning disable CS0612
        if (Features is not null) yield return "<Features> is deprecated, use <ActivateFeatures>";
        if (IgnoredFeatures is not null) yield return "<IgnoredFeatures> is deprecated";
#pragma warning restore CS0612
    }

    public override void PostLoad()
    {
        base.PostLoad();

        #region BackCompatability

#pragma warning disable CS0612

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

#pragma warning restore CS0612

        #endregion
    }

#pragma warning disable CS0612
    [Obsolete] public FeatureOpts Features;

    [Obsolete] public FeatureOpts IgnoredFeatures;
#pragma warning restore CS0612
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
}

public enum LogLevel
{
    // ReSharper disable once UnusedMember.Global
    None = 0,
    Important = 1,
    Info = 2,
    Verbose = 3,
    Silly = 4,
    Tick = 5
}

public class MVCFSettings : ModSettings
{
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref MVCF.DebugMode, "debugMode");
        Scribe_Values.Look(ref MVCF.LogLevel, "logLevel", LogLevel.Important);
    }
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
        MVCF.LogFormat($"Patching {this}", LogLevel.Silly);
        harm.Patch(target,
            prefix is null ? null : new HarmonyMethod(prefix),
            postfix is null ? null : new HarmonyMethod(postfix),
            transpiler is null ? null : new HarmonyMethod(transpiler));
        numPatches++;
    }

    public void Unapply(Harmony harm)
    {
        if (Prefs.DevMode) Log.Message($"Unpatching {this}");
        if (prefix is not null) harm.Unpatch(target, prefix);

        if (postfix is not null) harm.Unpatch(target, postfix);

        if (transpiler is not null) harm.Unpatch(target, transpiler);
        numPatches--;
    }
}
