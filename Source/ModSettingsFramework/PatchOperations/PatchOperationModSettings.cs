using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ModSettingsFramework
{
    [HotSwappable]
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
            Rect rect = new Rect(listingStandard.curX, listingStandard.curY, 
                listingStandard.ColumnWidth, Text.LineHeight);
            lineNumber++;
            rect.y -= 3;
            rect.height += 6;
            if (lineNumber % 2 != 0)
            {
                Widgets.DrawLightHighlight(rect);
            }
            Widgets.DrawHighlightIfMouseover(rect);

            CheckboxLabeledInner(listingStandard, optionLabel, ref field);
        }

        public void CheckboxLabeledInner(Listing_Standard listingStandard, string label, ref bool checkOn, string tooltip = null, float height = 0f, float labelPct = 1f)
        {
            float height2 = ((height != 0f) ? height : Text.CalcHeight(label, listingStandard.ColumnWidth * labelPct));
            Rect rect = listingStandard.GetRect(height2, labelPct);
            rect.width = Math.Min(rect.width + 24f, listingStandard.ColumnWidth);
            if (!listingStandard.BoundingRectCached.HasValue || rect.Overlaps(listingStandard.BoundingRectCached.Value))
            {
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(rect))
                    {
                        Widgets.DrawHighlight(rect);
                    }
                    TooltipHandler.TipRegion(rect, tooltip);
                }
                CheckboxLabeled(rect, label, ref checkOn);
            }
            listingStandard.Gap(listingStandard.verticalSpacing);
        }

        public static void CheckboxLabeled(Rect rect, string label, ref bool checkOn, bool disabled = false, Texture2D texChecked = null, Texture2D texUnchecked = null, bool placeCheckboxNearText = false, bool paintable = false)
        {
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (placeCheckboxNearText)
            {
                rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + 24f + 10f);
            }
            Rect rect2 = rect;
            rect2.xMax -= 24f;
            Widgets.Label(rect2, label);
            if (!disabled)
            {
                if (Widgets.ButtonInvisible(rect, true))
                {
                    checkOn = !checkOn;
                    if (checkOn)
                    {
                        SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                    }
                    else
                    {
                        SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                    }
                }
            }
            Widgets.CheckboxDraw(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, checkOn, disabled, 24f, texChecked, texUnchecked);
            Text.Anchor = anchor;
        }

        protected void DoSlider(Listing_Standard listingStandard, string label, ref float value, string valueLabel, 
            float min, float max, string explanation, float sliderYOffset = 0)
        {
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            Rect sliderRect = rect.RightPart(.60f).Rounded();
            Widgets.Label(rect, label);
            scrollHeight += rect.height;
            sliderRect.y += sliderYOffset;
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
