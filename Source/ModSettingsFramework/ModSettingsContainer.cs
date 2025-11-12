using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ModSettingsFramework
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
    [HotSwappable]
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

        private List<(ModSettingsContainer container, PatchOperationModSettings patch)> _patchOperationMods;
        public List<(ModSettingsContainer container, PatchOperationModSettings patch)> PatchOperationModSettings
        {
            get
            {
                if (_patchOperationMods is null)
                {
                    _patchOperationMods = new List<(ModSettingsContainer container, PatchOperationModSettings patch)>();
                    foreach (var patch in ModContentPack_LoadPatches_Patch.allPatches.Values.SelectMany(x => x))
                    {
                        if (patch.SettingsContainer == this)
                        {
                            _patchOperationMods.Add((this, patch));
                        }
                        else if (patch.MatchesModPackageID(packageID))
                        {
                            _patchOperationMods.Add((patch.SettingsContainer, patch));
                        }
                    }
                }
                return _patchOperationMods;
            }
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var listingStandard = new Listing_Standard();
            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, scrollHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            scrollHeight = 30;
            listingStandard.Begin(rect2);
            Text.Font = GameFont.Small;
            var curPatches = PatchOperationModSettings.ListFullCopy();
            foreach (var category in DefDatabase<ModOptionCategoryDef>.AllDefs.OrderBy(x => x.order))
            {
                var patchesInCategory = curPatches.Where(x => x.patch.category 
                == category.defName).OrderBy(x => x.patch.order).ToList();
                if (patchesInCategory.Any())
                {
                    var patchesHeight = patchesInCategory.Sum(x => x.patch.SettingsHeight());
                    var gapLine = 8;
                    var gapAfterSection = 12f;
                    var sectionBorder = 4;
                    var bottomBorder = 4;
                    var categoryLabelHeight = Text.CalcHeight(category.label, rect2.width);
                    var sectionHeight = categoryLabelHeight
                        + gapLine
                        + patchesHeight;
                    var section = listingStandard.BeginSection(sectionHeight, sectionBorder, bottomBorder);
                    scrollHeight += sectionHeight + sectionBorder + bottomBorder;
                    section.Label(category.label);
                    section.GapLine(gapLine);
                    foreach (var patch in patchesInCategory)
                    {
                        patch.patch.scrollHeight = 0;
                        patch.patch.DoSettings(patch.container, section);
                        if (patch.patch is PatchOperationWorker worker)
                        {
                            worker.CopyValues();
                        }
                    }
                    listingStandard.EndSection(section);
                    listingStandard.Gap(gapAfterSection);
                    scrollHeight += gapAfterSection;
                    foreach (var patch in patchesInCategory)
                    {
                        curPatches.Remove(patch);
                    }
                }
            }
            foreach (var patch in curPatches.OrderBy(x => x.patch.order))
            {
                patch.patch.scrollHeight = 0;
                patch.patch.DoSettings(patch.container, listingStandard);
                scrollHeight += patch.patch.SettingsHeight();
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
                        if (mod.PackageIdPlayerFacing.ToLower() == packageID.ToLower())
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
