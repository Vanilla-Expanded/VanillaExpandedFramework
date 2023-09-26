using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ModSettingsFramework
{
    public class ModSettingsContainer : IExposable
    {
        public string packageID;
        public ModSettingsFrameworkMod modHandle;
        public Dictionary<string, bool> patchOperationStates = new Dictionary<string, bool>();
        public Dictionary<string, float> patchOperationValues = new Dictionary<string, float>();
        public Dictionary<string, PatchOperationWorker> patchWorkers = new Dictionary<string, PatchOperationWorker>();
        public bool PatchOperationEnabled(string id, bool defaultValue)
        {
            if (!patchOperationStates.TryGetValue(id, out var enabled))
            {
                patchOperationStates[id] = enabled = defaultValue;
            }
            return enabled;
        }

        public float PatchOperationValue(string id, float defaultValue)
        {
            if (!patchOperationValues.TryGetValue(id, out var value))
            {
                patchOperationValues[id] = value = defaultValue;
            }
            return value;
        }

        public T PatchWorker<T>() where T : PatchOperationWorker
        {
            return patchWorkers.Values.OfType<T>().FirstOrDefault();
        }

        private Vector2 scrollPosition = Vector2.zero;
        private float scrollHeight;

        private List<PatchOperationModSettings> _patchOperationMods;
        public List<PatchOperationModSettings> PatchOperationModSettings => _patchOperationMods ??= LoadedModManager.RunningMods.SelectMany(x => x.Patches.OfType<PatchOperationModSettings>())
                        .Where(x => x.SettingsContainer == this && x.CanRun()).ToList();
        public void DoSettingsWindowContents(Rect inRect)
        {
            var listingStandard = new Listing_Standard();
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, scrollHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            scrollHeight = 0;
            listingStandard.Begin(rect2);
            Text.Font = GameFont.Small;
            var curPatches = PatchOperationModSettings.ListFullCopy();
            foreach (var category in DefDatabase<ModOptionCategoryDef>.AllDefs.OrderBy(x => x.order))
            {
                var patchesInCategory = curPatches.Where(x => x.category == category.defName).OrderBy(x => x.order).ToList();
                if (patchesInCategory.Any())
                {
                    var height = patchesInCategory.Sum(x => x.SettingsHeight());
                    var sectionSize = height + 24 + 8;
                    scrollHeight += sectionSize + 12 + 6;
                    var section = listingStandard.BeginSection(sectionSize);
                    section.Label(category.label);
                    section.GapLine(8);
                    foreach (var patch in patchesInCategory)
                    {
                        patch.DoSettings(this, section);
                        if (patch is PatchOperationWorker worker)
                        {
                            worker.CopyValues();
                        }
                    }
                    listingStandard.EndSection(section);
                    listingStandard.Gap();
                    foreach (var patch in patchesInCategory)
                    {
                        curPatches.Remove(patch);
                    }
                }
            }
            foreach (var patch in curPatches.OrderBy(x => x.order))
            {
                patch.DoSettings(this, listingStandard);
                scrollHeight += patch.SettingsHeight();
            }

            listingStandard.End();
            Widgets.EndScrollView();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref packageID, "packageID");
            Scribe_Collections.Look(ref patchOperationStates, "patchOperationStates", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref patchOperationValues, "patchOperationValues", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref patchWorkers, "workers", LookMode.Value, LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                patchOperationStates ??= new Dictionary<string, bool>();
                patchOperationValues ??= new Dictionary<string, float>();
                foreach (var mod in LoadedModManager.RunningMods)
                {
                    foreach (var worker in mod.Patches.OfType<PatchOperationWorker>())
                    {
                        if (worker.modPackageSettingsID != null && worker.modPackageSettingsID.ToLower() == packageID.ToLower()
                            || mod.PackageIdPlayerFacing.ToLower() == packageID.ToLower())
                        {
                            var type = worker.GetType().FullName;
                            if (patchWorkers.TryGetValue(type, out var matchingSavedWorker) && matchingSavedWorker != null)
                            {
                                worker.CopyFrom(matchingSavedWorker);
                            }
                            else
                            {
                                patchWorkers[worker.GetType().FullName] = worker;
                            }
                        }
                    }
                }
            }
        }
    }
}
