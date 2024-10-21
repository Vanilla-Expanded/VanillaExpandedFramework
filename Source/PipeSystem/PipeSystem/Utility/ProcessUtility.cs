using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    public static class ProcessUtility
    {
        private static readonly RecipeTooltipLayout processTooltip = new RecipeTooltipLayout();

        /// <summary>
        /// Draw def infos: ingredients and product(s)
        /// </summary>
        /// <param name="processDef"></param>
        /// <param name="processIndex"></param>
        /// <param name="rect"></param>
        public static void DoProcessInfoWindow(this ProcessDef processDef, int processIndex, Rect rect)
        {
            ProcessTooltip(processDef, false);
            if (!processTooltip.Empty)
            {
                var windowRect = Find.WindowStack.currentlyDrawnWindow.windowRect;
                var immRect = new Rect(windowRect.x + rect.xMax + 10f, windowRect.y + rect.y, processTooltip.Size.x, processTooltip.Size.y);
                immRect.x = Mathf.Min(immRect.x, UI.screenWidth - immRect.width);
                immRect.y = Mathf.Min(immRect.y, UI.screenHeight - immRect.height);
                Find.WindowStack.ImmediateWindow(123 * (processIndex + 1), immRect, WindowLayer.Super, delegate
                {
                    var font = Text.Font;
                    Text.Font = GameFont.Small;
                    GUI.BeginGroup(immRect.AtZero());
                    ProcessTooltip(processDef, true);
                    GUI.EndGroup();
                    Text.Font = font;
                });
            }
        }

        /// <summary>
        /// Draw def tooltip
        /// </summary>
        /// <param name="processDef"></param>
        /// <param name="draw">Should draw on screen?</param>
        private static void ProcessTooltip(ProcessDef processDef, bool draw)
        {
            processTooltip.Reset(4f);
            processTooltip.Label("Ingredients".Translate() + ": ", draw);
            processTooltip.Newline();
            // Draw ingredients
            for (int i = 0; i < processDef.ingredients.Count; i++)
            {
                var requirement = processDef.ingredients[i];
                /*if (requirement.filter?.AllowedThingDefs == null)
                {
                    continue;
                }
                IEnumerable<ThingDef> enumerable = ingredient.filter.AllowedThingDefs.Where((ThingDef x) => Widgets.GetIconFor(x) != BaseContent.BadTex && (def.fixedIngredientFilter.AllowedDefCount == 0 || (def.fixedIngredientFilter.Allows(x) && !def.fixedIngredientFilter.IsAlwaysDisallowedDueToSpecialFilters(x))));
                IEnumerable<TextureAndColor> enumerable2 = enumerable.Select((ThingDef x) => ToTextureAndColor(x)).Distinct();*/

                // Draw all thing requirement icons
                if (requirement.thing != null)
                {
                    var icons = new List<TextureAndColor>() { ToTextureAndColor(requirement.thing) };
                    DisplayIngredientIconRow(icons, draw, requirement.countNeeded);
                    // If can also take from net, add or X net
                    if (requirement.pipeNet != null)
                    {
                        if (icons.Count > 9)
                            processTooltip.Gap(4f, 0f);

                        Text.Anchor = TextAnchor.MiddleLeft;
                        processTooltip.Label("PipeSystem_OrNet".Translate(requirement.pipeNet.loweredName), draw);
                        Text.Anchor = TextAnchor.UpperLeft;
                    }
                }
                // Draw category requirement
                else if (requirement.thingCategory != null)
                {
                    var icons = new List<TextureAndColor>() { ToTextureAndColorCategory(requirement.thingCategory) };
                    DisplayIngredientIconRow(icons, draw, requirement.countNeeded);
                    // If can also take from net, add or X net
                    if (requirement.pipeNet != null)
                    {
                        if (icons.Count > 9)
                            processTooltip.Gap(4f, 0f);

                        Text.Anchor = TextAnchor.MiddleLeft;
                        processTooltip.Label("PipeSystem_OrNet".Translate(requirement.pipeNet.loweredName), draw);
                        Text.Anchor = TextAnchor.UpperLeft;
                    }
                }
                // Draw net requirement
                else
                {
                    processTooltip.Gap(8f, 0f);
                    processTooltip.Label(requirement.countNeeded + "x ", draw);
                    // Draw net ui icon if any
                    if (requirement.pipeNet.uiIcon != null)
                    {
                        processTooltip.Icon(requirement.pipeNet.uiIcon, Color.white, Text.LineHeightOf(GameFont.Small), draw);
                    }
                    processTooltip.Gap(4f, 0f);
                    processTooltip.Label("PipeSystem_XFromNet".Translate(requirement.pipeNet.loweredName), draw);
                }
                processTooltip.Newline();
            }

            var resultCount = processDef.results.Count;
            processTooltip.Label((resultCount > 1 ? "Products" : "PipeSystem_Product").Translate() + ": ", draw);
            processTooltip.Newline();
            // Draw products
            for (int i = 0; i < resultCount; i++)
            {
                var result = processDef.results[i];
                if (result.thing != null)
                {
                    DisplayIngredientIconRow(new List<TextureAndColor>() { ToTextureAndColor(result.thing) }, draw, result.count);
                    // Add or X net if can output to net
                    if (result.pipeNet != null)
                    {
                        processTooltip.Gap(4f, 0f);
                        Text.Anchor = TextAnchor.MiddleLeft;
                        processTooltip.Label("PipeSystem_OrNet".Translate(result.pipeNet.loweredName), draw);
                        Text.Anchor = TextAnchor.UpperLeft;
                    }
                }
                else
                {
                    processTooltip.Gap(8f, 0f);
                    processTooltip.Label(result.count + "x ", draw);
                    // Draw net ui icon if any
                    if (result.pipeNet.uiIcon != null)
                    {
                        processTooltip.Icon(result.pipeNet.uiIcon, Color.white, Text.LineHeightOf(GameFont.Small), draw);
                    }
                    processTooltip.Gap(4f, 0f);
                    processTooltip.Label("PipeSystem_OutputToNet".Translate(result.pipeNet.loweredName), draw);
                }
                processTooltip.Newline();
            }

            if (processDef.wastePackToProduce > 0 && ModsConfig.BiotechActive)
            {
                DisplayIngredientIconRow(new List<TextureAndColor>() { ToTextureAndColor(ThingDefOf.Wastepack) }, draw, processDef.wastePackToProduce);
            }
            // Expand window
            processTooltip.Expand(4f, 4f);
        }

        /// <summary>
        /// Draw ingredients icon(s), add net info if any
        /// </summary>
        /// <param name="icons"></param>
        /// <param name="draw">Should draw on screen?</param>
        /// <param name="count"></param>
        private static void DisplayIngredientIconRow(List<TextureAndColor> icons, bool draw, float count)
        {
            int iconCount = icons.Count;
            if (iconCount > 0)
            {
                int num = Mathf.Min(9, iconCount);
                processTooltip.Gap(8f, 0f);
                processTooltip.Label(count + "x ", draw);
                // Draw 9 or less icons
                for (int i = 0; i < num; i++)
                {
                    var textureAndColor = icons[i];
                    processTooltip.Icon(textureAndColor.Texture, textureAndColor.Color, Text.LineHeightOf(GameFont.Small), draw);
                    processTooltip.Gap(4f, 0f);
                }
                // Add etc it more than 9 icons
                if (iconCount > 9)
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    processTooltip.Label(" " + "Etc".Translate() + "...", draw);
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }
        }

        /// <summary>
        /// Create TextureAndColor from ThingDef
        /// </summary>
        /// <param name="td"></param>
        /// <returns></returns>
        private static TextureAndColor ToTextureAndColor(ThingDef td) => new TextureAndColor(Widgets.GetIconFor(td), td.uiIconColor);

        /// <summary>
        /// Create TextureAndColor from ThingCategoryDef
        /// </summary>
        /// <param name="td"></param>
        /// <returns></returns>
        private static TextureAndColor ToTextureAndColorCategory(ThingCategoryDef td) => new TextureAndColor(td.icon, Color.white);
    }
}