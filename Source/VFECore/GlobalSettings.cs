using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class VFEGlobal : Mod
    {
        public static VFEGlobalSettings settings;
        private Vector2 scrollPosition = Vector2.zero;
        protected readonly Vector2 ButtonSize = new(120f, 40f);

        public VFEGlobal(ModContentPack content) : base(content)
        {
            settings = GetSettings<VFEGlobalSettings>();
        }

        public override string SettingsCategory()
        {
            return "Vanilla Expanded Framework";
        }

        private int PageIndex = 0;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var tabRect = new Rect(inRect)
            {
                y = inRect.y + 40f
            };
            var mainRect = new Rect(inRect)
            {
                height = inRect.height - 40f,
                y = inRect.y + 40f
            };

            Widgets.DrawMenuSection(mainRect);
            var tabs = new List<TabRecord>
            {
                new TabRecord("VEF_GeneralTitle".Translate(), () =>
                {
                    PageIndex = 0;
                    WriteSettings();

                }, PageIndex == 0),
                new TabRecord("VEF_TPTitle".Translate(), () =>
                {
                    PageIndex = 1;
                    WriteSettings();

                }, PageIndex == 1),
                new TabRecord("VEF.WeatherDamages".Translate(), () =>
                {
                    PageIndex = 2;
                    WriteSettings();
                }, PageIndex == 2)
            };
            TabDrawer.DrawTabs(tabRect, tabs);

            switch (PageIndex)
            {
                case 0:
                    GeneralSettings(mainRect.ContractedBy(15f));
                    break;
                case 1:
                    ToggablePatchesSettings(mainRect.ContractedBy(15f));
                    break;
                case 2:
                    WeatherDamageSettings(mainRect.ContractedBy(15f));
                    break;
                default:
                    break;
            }
        }

        // General settings

        private int FactionCanBeAddedCount;

        private void GeneralSettings(Rect rect)
        {
            var list = new Listing_Standard();
            list.Begin(rect);

            Text.Font = GameFont.Small;
            list.Label("VEF_FactionDiscovery".Translate());
            if (Current.Game != null)
            {
                FactionCanBeAddedCount = DefDatabase<FactionDef>.AllDefs.Where(ValidatorAnyFactionLeft).Count();
                list.Label("CanAddXFaction".Translate(FactionCanBeAddedCount));
                if (FactionCanBeAddedCount > 0 && list.ButtonText("AskForPopUp".Translate(), "AskForPopUpExplained".Translate()))
                {
                    Current.Game.World.GetComponent<NewFactionSpawningState>().ClearIgnored();
                    var factionEnumerator = DefDatabase<FactionDef>.AllDefs.Where(Patch_GameComponentUtility.LoadedGame.Validator).GetEnumerator();
                    if (factionEnumerator.MoveNext())
                    {
                        // Only one dialog can be stacked at a time, so give it the list of all factions
                        Dialog_NewFactionSpawning.OpenDialog(factionEnumerator);
                    }
                }
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                list.Label("NeedToBeInGame".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
            }
            list.GapLine(12);

            // KCSG
            list.Gap(12);
            list.Label("VEF_CustomStructureGeneration".Translate());
            list.Gap(5);
            list.CheckboxLabeled("VEF_VerboseLogging".Translate(), ref settings.enableVerboseLogging);
            list.GapLine(12);

            // Texture Variations
            list.Gap(12);
            list.Label("VEF_TextureVariations".Translate());
            list.Gap(5);
            list.CheckboxLabeled("VFE_RandomBuildingsDontStartRandom".Translate(), ref settings.randomStartsAsRandom, null);
            list.Gap(5);
            list.CheckboxLabeled("VFE_HideRandomizeButton".Translate(), ref settings.hideRandomizeButtons, null);
            list.GapLine(12);

            // General
            list.Gap(12);
            list.CheckboxLabeled("VEF_DisableTextureCaching".Translate(), ref settings.disableCaching, "Warning: Enabling this might cause performance issues.");
            list.CheckboxLabeled("VEF.DisableModSourceReport".Translate(), ref settings.disableModSourceReport);
            list.CheckboxLabeled("VEF_EnablePipeSystemNoStorageAlert".Translate(), ref settings.enablePipeSystemNoStorageAlert);
           

            list.End();
        }

       

        private bool ValidatorAnyFactionLeft(FactionDef faction)
        {
            if (faction == null)
            {
                return false;
            }

            if (faction.isPlayer)
            {
                return false;
            }

            if (!faction.canMakeRandomly && faction.hidden && faction.maxCountAtGameStart <= 0)
            {
                return false;
            }

            if (Find.FactionManager.AllFactions.Count(f => f.def == faction) > 0)
            {
                return false;
            }

            if (NewFactionSpawningUtility.NeverSpawn(faction))
            {
                return false;
            }

            return true;
        }

        // Toggable patches settings

        private int ToggablePatchCount;
        private int ModUsingToggablePatchCount = 0;

        private bool initialized;
        private void ToggablePatchesSettings(Rect rect)
        {
            if (!initialized)
            {
                initialized = true;
                // Toggable patches
                foreach (var mod in LoadedModManager.RunningMods)
                {
                    try
                    {
                        if (mod.Patches != null)
                        {
                            int modPatchesCount = mod.Patches.ToList().FindAll(p => p is PatchOperationToggableSequence pt && pt.ModsFound()).Count;
                            if (modPatchesCount > 0)
                            {
                                ModUsingToggablePatchCount++;
                                ToggablePatchCount += modPatchesCount;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(mod.Name + " produced an error with XML patches: " + ex.ToString());
                    }
                }
            }
            var viewRect = new Rect(rect)
            {
                height = 110f + ((ToggablePatchCount + ModUsingToggablePatchCount) * 32f),
                width = rect.width - 20f,
            };

            var list = new Listing_Standard();
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect, true);
            list.Begin(viewRect);

            Text.Anchor = TextAnchor.MiddleCenter;
            list.Label("NeedRestart".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            list.Gap();

            foreach (var modContentPack in (from m in LoadedModManager.RunningMods orderby m.OverwritePriority select m).ThenBy((ModContentPack x) => LoadedModManager.RunningModsListForReading.IndexOf(x)))
            {
                if (modContentPack?.Patches != null && modContentPack.Patches.Any(p => p is PatchOperationToggableSequence pt && pt.ModsFound()))
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    list.Label(modContentPack.Name);
                    Text.Anchor = TextAnchor.UpperLeft;
                    AddButtons(list, modContentPack);
                }
            }

            list.End();
            Widgets.EndScrollView();
        }

        private void AddButtons(Listing_Standard list, ModContentPack modContentPack)
        {
            foreach (var patchOperation in modContentPack.Patches)
            {
                if (patchOperation is PatchOperationToggableSequence p && p.ModsFound())
                {
                    string pLabelSmall = p.label.Replace(" ", "");
                    string bLabel = !settings.toggablePatch.NullOrEmpty() && settings.toggablePatch.ContainsKey(pLabelSmall) ? settings.toggablePatch[pLabelSmall].ToString() : p.enabled.ToString();
                    if (list.ButtonTextLabeled(p.label, bLabel))
                    {
                        if (!settings.toggablePatch.NullOrEmpty() && settings.toggablePatch.ContainsKey(pLabelSmall)) // Already in, we remove it
                        {
                            settings.toggablePatch.Remove(pLabelSmall);
                        }
                        else // Add to toggablePatch with the inverse value
                        {
                            if (settings.toggablePatch.NullOrEmpty())
                            {
                                settings.toggablePatch = new Dictionary<string, bool>();
                            }

                            settings.toggablePatch.Add(pLabelSmall, !p.enabled);
                        }
                    }
                }
            }
        }

        // Weather damage settings
        private void WeatherDamageSettings(Rect rect)
        {
            var list = new Listing_Standard();
            list.Begin(rect);

            Text.Font = GameFont.Small;
            foreach (string key in settings.weatherDamagesOptions.Keys.ToList())
            {
                var weatherDef = DefDatabase<WeatherDef>.GetNamedSilentFail(key);
                if (weatherDef != null)
                {
                    var extension = weatherDef.GetModExtension<WeatherEffectsExtension>();
                    if (extension != null)
                    {
                        bool value = settings.weatherDamagesOptions[key];
                        list.CheckboxLabeled("VEF.EnableWeatherDamage".Translate(weatherDef.LabelCap), ref value);
                        settings.weatherDamagesOptions[key] = value;
                        list.Gap(5);
                    }
                }
            }
            list.End();
        }
    }

    [StaticConstructorOnStartup]
    public static class ModSettingsHandler
    {
        static ModSettingsHandler()
        {
            VFEGlobal.settings.weatherDamagesOptions ??= new Dictionary<string, bool>();
            foreach (var weatherDef in DefDatabase<WeatherDef>.AllDefs)
            {
                var extension = weatherDef.GetModExtension<WeatherEffectsExtension>();
                if (extension != null)
                {
                    if (!VFEGlobal.settings.weatherDamagesOptions.ContainsKey(weatherDef.defName))
                    {
                        VFEGlobal.settings.weatherDamagesOptions[weatherDef.defName] = true;
                    }
                }
            }
        }
    }

    public class VFEGlobalSettings : ModSettings
    {
        public Dictionary<string, bool> toggablePatch = new();
        public bool enableVerboseLogging;
        public bool disableCaching;
        public bool randomStartsAsRandom = false;
        public bool hideRandomizeButtons = false;
        public bool enablePipeSystemNoStorageAlert = true;
       
        

        public Dictionary<string, bool> weatherDamagesOptions = new();
        public bool disableModSourceReport;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref toggablePatch, "toggablePatch", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref enableVerboseLogging, "enableVerboseLogging", false);
            Scribe_Values.Look(ref disableCaching, "disableCaching", true);
            Scribe_Values.Look(ref randomStartsAsRandom, "randomStartsAsRandom", false, false);
            Scribe_Values.Look(ref hideRandomizeButtons, "hideRandomizeButtons", false, true);
            Scribe_Values.Look(ref disableModSourceReport, "disableModSourceReport");
            Scribe_Values.Look(ref enablePipeSystemNoStorageAlert, "enablePipeSystemNoStorageAlert", true);
            Scribe_Collections.Look(ref weatherDamagesOptions, "weatherDamagesOptions", LookMode.Value, LookMode.Value);
           

           

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                weatherDamagesOptions ??= new Dictionary<string, bool>();
                toggablePatch ??= new Dictionary<string, bool>();
            }
        }
    }
}