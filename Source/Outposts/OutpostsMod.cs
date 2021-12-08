using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Outposts
{
    public class OutpostsMod : Mod
    {
        public static List<WorldObjectDef> Outposts;
        public static Harmony Harm;
        public static OutpostsSettings Settings;

        public OutpostsMod(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(FindOutposts);
            Settings = GetSettings<OutpostsSettings>();
        }

        private static void FindOutposts()
        {
            Outposts = DefDatabase<WorldObjectDef>.AllDefs.Where(def => typeof(Outpost).IsAssignableFrom(def.worldObjectClass)).ToList();
            Harm = new Harmony("vanillaexpanded.outposts");
            if (Outposts.Any())
            {
                HarmonyPatches.DoPatches();
                Outposts_DefOf.VEF_OutpostDeliverySpot.designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed("Misc");
            }
        }

        public override string SettingsCategory() => Outposts.Any() ? "Outposts.Settings.Title".Translate() : null;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.Label("Outposts.Settings.Multiplier.Production".Translate(Settings.ProductionMultiplier.ToStringPercent()));
            Settings.ProductionMultiplier = listing.Slider(Settings.ProductionMultiplier, 0.1f, 10f);
            listing.Label("Outposts.Settings.Multiplier.Time".Translate(Settings.TimeMultiplier.ToStringPercent()));
            Settings.TimeMultiplier = listing.Slider(Settings.TimeMultiplier, 0.01f, 5f);
            if (listing.ButtonTextLabeled("Outposts.Settings.DeliveryMethod".Translate(), $"Outposts.Settings.DeliveryMethod.{Settings.DeliveryMethod}".Translate()))
                Find.WindowStack.Add(new FloatMenu(Enum.GetValues(typeof(DeliveryMethod)).OfType<DeliveryMethod>().Select(method => new FloatMenuOption(
                    $"Outposts.Settings.DeliveryMethod.{method}".Translate(), () => Settings.DeliveryMethod = method)).ToList()));
            listing.End();
        }
    }

    public class OutpostsSettings : ModSettings
    {
        public DeliveryMethod DeliveryMethod = DeliveryMethod.Teleport;
        public float ProductionMultiplier = 1f;
        public float TimeMultiplier = 1f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ProductionMultiplier, "productionMultiplier", 1f);
            Scribe_Values.Look(ref TimeMultiplier, "timeMultiplier", 1f);
            Scribe_Values.Look(ref DeliveryMethod, "deliveryMethod");
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
}