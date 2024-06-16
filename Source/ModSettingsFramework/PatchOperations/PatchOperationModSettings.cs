using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ModSettingsFramework
{
    public abstract class PatchOperationModSettings : PatchOperation
    {
        public int order;
        public string category;
        public List<string> mods;
        public string id;
        public string label;
        public string tooltip;
        public bool showTooltipAsTinyText;
        public int roundToDecimalPlaces = 2;
        public string modPackageSettingsID;
        public ModContentPack modContentPack;

        public bool MatchesModPackageID(string packageID)
        {
            if (category.NullOrEmpty() is false)
            {
                var categoryDef = DefDatabase<ModOptionCategoryDef>.GetNamedSilentFail(category);
                if (categoryDef != null && categoryDef.modPackageSettingsID.NullOrEmpty() is false)
                {
                    return packageID.ToLower() == categoryDef.modPackageSettingsID.ToLower();
                }
            }
            if (modPackageSettingsID.NullOrEmpty() is false)
            {
                return packageID.ToLower() == modPackageSettingsID.ToLower();
            }
            return false;
        }
        private ModSettingsContainer settingContainerCached;
        public ModSettingsContainer SettingsContainer
        {
            get
            {
                if (settingContainerCached is null)
                {
                    if (modContentPack == null)
                    {
                        foreach (var runningMod in LoadedModManager.RunningMods)
                        {
                            if (runningMod.Patches.Contains(this))
                            {
                                modContentPack = runningMod;
                                return settingContainerCached ??= ModSettingsFrameworkSettings.GetModSettingsContainer(runningMod);
                            }
                        }
                    }
                    else
                    {
                        return settingContainerCached ??= ModSettingsFrameworkSettings.GetModSettingsContainer(modContentPack);
                    }
                }
                return settingContainerCached;
            }
            set
            {
                settingContainerCached = value;
            }
        }

        public abstract void DoSettings(ModSettingsContainer container, Listing_Standard list);
        public virtual int SettingsHeight() => (int)scrollHeight;
        public bool CanRun()
        {
            var modsToCheck = new List<string>();
            if (category.NullOrEmpty() is false)
            {
                var def = DefDatabase<ModOptionCategoryDef>.GetNamedSilentFail(category);
                if (def != null && def.mods != null)
                {
                    modsToCheck.AddRange(def.mods);
                }
            }
            if (mods != null)
            {
                modsToCheck.AddRange(mods);
            }

            if (modsToCheck.NullOrEmpty() is false)
            {
                for (int i = 0; i < modsToCheck.Count; i++)
                {
                    if (ModLister.HasActiveModWithName(modsToCheck[i]))
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        public float scrollHeight = 99999999;

        protected void DoCheckbox(Listing_Standard listingStandard, string optionLabel, ref bool field, string explanation)
        {
            Rect rect = new Rect(listingStandard.curX, listingStandard.curY, listingStandard.ColumnWidth, Text.LineHeight);
            CheckboxLabeled(listingStandard, optionLabel, ref field);
            ShowExplanation(listingStandard, explanation, rect);
            listingStandard.Gap(5);
            scrollHeight += 29;
        }

        public static int lineNumber = 0;

        protected void CheckboxLabeled(Listing_Standard listingStandard, string optionLabel, ref bool field)
        {
            Rect rect = new Rect(listingStandard.curX, listingStandard.curY, listingStandard.ColumnWidth, Text.LineHeight);
            lineNumber++;
            if (lineNumber % 2 != 0)
                Widgets.DrawLightHighlight(rect);
            Widgets.DrawHighlightIfMouseover(rect);
            listingStandard.CheckboxLabeled(optionLabel, ref field);
        }

        protected void DoSlider(Listing_Standard listingStandard, string label, ref float value, string valueLabel, float min, float max, string explanation)
        {
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            Rect sliderRect = rect.RightPart(.60f).Rounded();
            Widgets.Label(rect, label);
            scrollHeight += rect.height;
            value = Widgets.HorizontalSlider(sliderRect, (float)value, min, max, true, valueLabel);
            value = (float)Math.Round(value, roundToDecimalPlaces);
            listingStandard.Gap(5);
            scrollHeight += 5;
            ShowExplanation(listingStandard, explanation, rect.LeftPart(0.4f));
        }

        protected void DoSlider(Listing_Standard listingStandard, string label, ref int value, string valueLabel, float min, float max, string explanation)
        {
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            Rect sliderRect = rect.RightPart(.60f).Rounded();
            Widgets.Label(rect, label);
            scrollHeight += rect.height;
            value = (int)Widgets.HorizontalSlider(sliderRect, value, min, max, true, valueLabel);
            listingStandard.Gap(5);
            scrollHeight += 5;
            ShowExplanation(listingStandard, explanation, rect.LeftPart(0.4f));
        }

        protected void DoLabel(Listing_Standard listingStandard, string label, string explanation)
        {
            var rect = listingStandard.Label(label);
            scrollHeight += rect.height;
            ShowExplanation(listingStandard, explanation, rect);
        }

        protected void DoExplanation(Listing_Standard listingStandard, string explanation)
        {
            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            var rect = listingStandard.Label(explanation);
            scrollHeight += rect.height;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            listingStandard.Gap(5);
            scrollHeight += 5;
        }

        protected void DoRadioButton(Listing_Standard listingStandard, string explanation)
        {
            var rect = listingStandard.Label(explanation);
            scrollHeight += rect.height;
            listingStandard.Gap(5);
            scrollHeight += 5;
        }

        private void ShowExplanation(Listing_Standard listingStandard, string explanation, Rect rect)
        {
            if (explanation.NullOrEmpty() is false)
            {
                if (showTooltipAsTinyText)
                {
                    DoExplanation(listingStandard, explanation);
                }
                else
                {
                    TooltipHandler.TipRegion(rect, explanation);
                }
            }
        }
    }
}
