using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace ModSettingsFramework
{
    public class ModSettingsFrameworkMod : Mod
    {
        public static ModSettingsFrameworkSettings settings;
        public ModContentPack modPackOverride;
        public static bool fakeInit;
        public bool fakeMod;
        public ModSettingsFrameworkMod(ModContentPack pack) : base(pack)
        {
            if (fakeInit is false)
            {
                new Harmony("ModSettingsFrameworkMod").PatchAll();
            }
        }

        public ModSettingsFrameworkSettings LoadSettings()
        {
            if (modSettings != null && modSettings.GetType() != typeof(ModSettingsFrameworkSettings))
            {
                Log.Error($"Mod {Content.Name} attempted to read two different settings classes (was {modSettings.GetType()}, is now ModSettingsFrameworkSettings)");
                return null;
            }
            if (modSettings != null)
            {
                return (ModSettingsFrameworkSettings)modSettings;
            }
            modSettings = ReadModSettings();
            modSettings.Mod = this;
            return modSettings as ModSettingsFrameworkSettings;
        }
        public static ModSettingsFrameworkSettings ReadModSettings()
        {
            string settingsFilename = GetSettingsFilename();
            ModSettingsFrameworkSettings target = null;
            try
            {
                if (File.Exists(settingsFilename))
                {
                    Scribe.loader.InitLoading(settingsFilename);
                    try
                    {
                        Scribe_Deep.Look(ref target, "ModSettings");
                    }
                    finally
                    {
                        Scribe.loader.FinalizeLoading();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"Caught exception while loading mod settings data for ModSettingsFrameworkSettings. Generating fresh settings. The exception was: {ex.ToString()}");
                target = null;
            }
            if (target == null)
            {
                return new ModSettingsFrameworkSettings();
            }
            return target;
        }

        private static string GetSettingsFilename()
        {
            return Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename($"ModSettingsFrameworkMod_Settings.xml"));
        }

        public override void WriteSettings()
        {
            Scribe.saver.InitSaving(GetSettingsFilename(), "SettingsBlock");
            try
            {
                Scribe_Deep.Look(ref settings, "ModSettings");
            }
            finally
            {
                Scribe.saver.FinalizeSaving();
            }

            foreach (var worker in LoadedModManager.RunningMods.SelectMany(x => x.Patches.OfType<PatchOperationWorker>())
                        .Where(x => x.CanRun()).ToList())
            {
                worker.ApplySettings();
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            ModSettingsFrameworkSettings.GetModSettingsContainer(modPackOverride.PackageIdPlayerFacing).DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            if (modPackOverride != null)
            {
                var patches = modPackOverride.Patches.OfType<PatchOperationModSettings>().Where(x => x.CanRun()).ToList();
                if (patches.Any())
                {
                    var workingPatches = new List<PatchOperationModSettings>();
                    foreach (var patch in patches)
                    {
                        if (patch.category.NullOrEmpty() is false)
                        {
                            var category = DefDatabase<ModOptionCategoryDef>.GetNamedSilentFail(patch.category);
                            if (category != null && category.modPackageSettingsID.NullOrEmpty() is false 
                                && category.modPackageSettingsID?.ToLower() != modPackOverride.PackageIdPlayerFacing.ToLower())
                            {
                                continue;
                            }
                            if (patch.modPackageSettingsID.NullOrEmpty() is false 
                                && patch.modPackageSettingsID?.ToLower() != modPackOverride.PackageIdPlayerFacing.ToLower())
                            {
                                continue;
                            }
                            if (category != null && category.modSettingsName.NullOrEmpty() is false)
                            {
                                return category.modSettingsName;
                            }
                        }
                        workingPatches.Add(patch);
                    }
                    if (workingPatches.Any())
                    {
                        return modPackOverride.Name;
                    }
                }
            }
            return null;
        }
    }
}
