using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Outposts;

public class OutpostsMod : Mod
{
    public static List<WorldObjectDef> Outposts;
    public static Harmony Harm;
    public static OutpostsSettings Settings;
    private static Dictionary<Type, List<FieldInfo>> editableFields;
    private float prevHeight = float.MaxValue;
    private Vector2 scrollPos;
    private Dictionary<WorldObjectDef, float> sectionHeights;

    public OutpostsMod(ModContentPack content) : base(content)
    {
        LongEventHandler.ExecuteWhenFinished(FindOutposts);
        Settings = GetSettings<OutpostsSettings>();
        editableFields = new();
        foreach (var type in typeof(Outpost).AllSubclasses()
                    .Concat(typeof(Outpost))
                    .Concat(typeof(OutpostExtension))
                    .Concat(typeof(OutpostExtension).AllSubclasses()))
        {
            editableFields[type] = new();
            foreach (var field in type.GetFields(AccessTools.all))
                if (field.HasAttribute<PostToSetingsAttribute>())
                    editableFields[type].Add(field);
        }
    }

    private void FindOutposts()
    {
        Outposts = DefDatabase<WorldObjectDef>.AllDefs.Where(def => typeof(Outpost).IsAssignableFrom(def.worldObjectClass)).ToList();
        Harm = new("vanillaexpanded.outposts");
        sectionHeights = Outposts.ToDictionary(o => o, _ => float.MaxValue);

        if (Outposts.Any())
        {
            HarmonyPatches.DoPatches();
            Outposts_DefOf.VEF_OutpostDeliverySpot.designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed("Misc");
        }
    }

    public static void Notify_Spawned(Outpost outpost)
    {
        Setup(outpost);
    }

    private static void Setup(Outpost outpost)
    {
        var settings = Settings.SettingsFor(outpost.def.defName);
        foreach (var info in editableFields[outpost.GetType()])
            if (info.TryGetAttribute<PostToSetingsAttribute>(out var attr))
                info.SetValue(outpost,
                    settings.TryGet($"{info.DeclaringType.Name}.{info.Name}", info.FieldType, out var value) ? value : attr.Default ?? info.GetValue(outpost));

        foreach (var info in editableFields[outpost.Ext.GetType()])
            if (info.TryGetAttribute<PostToSetingsAttribute>(out var attr))
                info.SetValue(outpost.Ext,
                    settings.TryGet($"{info.DeclaringType.Name}.{info.Name}", info.FieldType, out var value)
                        ? value
                        : info.GetValue(outpost.Ext) ?? attr.Default);
    }

    public static void Notify_Removed(Outpost outpost) { }

    public override string SettingsCategory() => Outposts.Any() ? "Outposts.Settings.Title".Translate() : null;

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        var viewRect = new Rect(0, 0, inRect.width - 20, prevHeight);
        Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
        var listing = new Listing_Standard();
        listing.Begin(viewRect);
        listing.Label("Outposts.Settings.Multiplier.Production".Translate(Settings.ProductionMultiplier.ToStringPercent()));
        Settings.ProductionMultiplier = listing.Slider(Settings.ProductionMultiplier, 0.1f, 10f);
        listing.Label("Outposts.Settings.Multiplier.Time".Translate(Settings.TimeMultiplier.ToStringPercent()));
        Settings.TimeMultiplier = listing.Slider(Settings.TimeMultiplier, 0.01f, 5f);
        if (listing.ButtonTextLabeled("Outposts.Settings.DeliveryMethod".Translate(),
                $"Outposts.Settings.DeliveryMethod.{Settings.DeliveryMethod}".Translate()))
            Find.WindowStack.Add(new FloatMenu(Enum.GetValues(typeof(DeliveryMethod))
               .OfType<DeliveryMethod>()
               .Select(method => new FloatMenuOption(
                    $"Outposts.Settings.DeliveryMethod.{method}".Translate(), () => Settings.DeliveryMethod = method))
               .ToList()));
        listing.GapLine();

        static void DoSetting(Listing_Standard listing, OutpostsSettings.OutpostSettings settings, FieldInfo info, object obj = null)
        {
            if (info.TryGetAttribute<PostToSetingsAttribute>(out var attr))
            {
                var key = $"{info.DeclaringType.Name}.{info.Name}";
                var current = settings.TryGet(key, info.FieldType, out var value) ? value : obj is null ? attr.Default : info.GetValue(obj);
                attr.Draw(listing, ref current);
                if (current == attr.Default)
                {
                    if (settings.Has(key)) settings.Remove(key);
                }
                else
                    settings.Set(key, current);
            }
        }

        foreach (var outpost in Outposts)
        {
            var section = listing.BeginSection(sectionHeights[outpost]);
            section.Label(outpost.LabelCap);
            var settings = Settings.SettingsFor(outpost.defName);
            foreach (var info in editableFields[outpost.worldObjectClass]) DoSetting(section, settings, info);
            if (outpost.GetModExtension<OutpostExtension>() is { } ext)
                foreach (var info in editableFields[ext.GetType()])
                    DoSetting(section, settings, info, ext);
            sectionHeights[outpost] = section.CurHeight;
            listing.EndSection(section);
            listing.Gap();
        }

        prevHeight = listing.CurHeight;
        listing.End();
        Widgets.EndScrollView();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        if (Find.World?.worldObjects is not null)
            foreach (var outpost in Find.World.worldObjects.AllWorldObjects.OfType<Outpost>())
                Setup(outpost);
    }
}

public class PostToSetingsAttribute : Attribute
{
    public enum DrawMode
    {
        Checkbox,
        IntSlider,
        Slider,
        Percentage,
        Time
    }

    private readonly object ignore;
    private readonly float max;
    private readonly float min;
    private readonly bool shouldIgnore;

    public object Default;

    public string LabelKey;
    public DrawMode Mode;
    public string TooltipKey;

    public PostToSetingsAttribute(string label, DrawMode mode, object value = null, float min = 0f, float max = 0f, string tooltip = null,
        object dontShowAt = null)
    {
        LabelKey = label;
        Mode = mode;
        Default = value;
        this.min = min;
        this.max = max;
        TooltipKey = tooltip;
        ignore = dontShowAt;
        shouldIgnore = dontShowAt is not null;
    }

    public void Draw(Listing_Standard listing, ref object current)
    {
        if (shouldIgnore && Equals(current, ignore)) return;
        switch (Mode)
        {
            case DrawMode.Checkbox:
                var checkState = (bool)current;
                listing.CheckboxLabeled(LabelKey.Translate(), ref checkState, TooltipKey?.Translate());
                if (checkState != (bool)current) current = checkState;
                break;
            case DrawMode.Slider:
                listing.Label(LabelKey.Translate() + ": " + current);
                current = listing.Slider((float)current, min, max);
                break;
            case DrawMode.Percentage:
                listing.Label(LabelKey.Translate() + ": " + ((float)current).ToStringPercent());
                current = listing.Slider((float)current, min, max);
                break;
            case DrawMode.IntSlider:
                listing.Label(LabelKey.Translate() + ": " + current);
                current = (int)listing.Slider((int)current, (int)min, (int)max);
                break;
            case DrawMode.Time:
                listing.Label(LabelKey.Translate() + ": " + ((int)current).ToStringTicksToPeriodVerbose());
                current = (int)listing.Slider((int)current, GenDate.TicksPerHour, GenDate.TicksPerYear);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class OutpostsSettings : ModSettings
{
    public DeliveryMethod DeliveryMethod = DeliveryMethod.Teleport;
    public float ProductionMultiplier = 1f;
    public Dictionary<string, OutpostSettings> SettingsPerOutpost = new();
    public float TimeMultiplier = 1f;


    public OutpostSettings SettingsFor(string defName)
    {
        SettingsPerOutpost ??= new();
        if (!SettingsPerOutpost.TryGetValue(defName, out var setting) || setting is null) SettingsPerOutpost.SetOrAdd(defName, setting = new());
        return setting;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ProductionMultiplier, "productionMultiplier", 1f);
        Scribe_Values.Look(ref TimeMultiplier, "timeMultiplier", 1f);
        Scribe_Values.Look(ref DeliveryMethod, "deliveryMethod");
        Scribe_Collections.Look(ref SettingsPerOutpost, "settingsPerOutpost", LookMode.Value, LookMode.Deep);
    }

    public class OutpostSettings : IExposable
    {
        private Dictionary<string, string> dictionary = new();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref dictionary, "keysToValues", LookMode.Value, LookMode.Value);
        }

        public bool Has(string key) => dictionary.ContainsKey(key);
        public void Remove(string key) => dictionary.Remove(key);

        public bool TryGet(string key, Type type, out object value)
        {
            dictionary ??= new();
            if (Has(key))
            {
                value = ParseHelper.FromString(dictionary[key], type);
                return true;
            }

            value = null;
            return false;
        }

        public void Set(string key, object value)
        {
            dictionary.SetOrAdd(key, value.ToString());
        }
    }
}

[DefOf]
public class Outposts_DefOf
{
    public static ThingDef VEF_OutpostDeliverySpot;
    public static DutyDef VEF_DropAllInInventory;
    public static ResearchProjectDef TransportPod;
}

public enum DeliveryMethod
{
    Teleport,
    PackAnimal,
    Store,
    ForcePods,
    PackOrPods
}

[StaticConstructorOnStartup]
public static class TexOutposts
{
    public static readonly Texture2D PackTex = ContentFinder<Texture2D>.Get("UI/Gizmo/AbandonOutpost");
    public static readonly Texture2D AddTex = ContentFinder<Texture2D>.Get("UI/Gizmo/AddToOutpost");
    public static readonly Texture2D RemoveTex = ContentFinder<Texture2D>.Get("UI/Gizmo/RemovePawnFromOutpost");
    public static readonly Texture2D StopPackTex = ContentFinder<Texture2D>.Get("UI/Gizmo/CancelAbandonOutpost");
    public static readonly Texture2D RemoveItemsTex = ContentFinder<Texture2D>.Get("UI/Gizmo/RemoveItemsFromOutpost");
    public static readonly Texture2D CreateTex = ContentFinder<Texture2D>.Get("UI/Gizmo/SetUpOutpost");
}
