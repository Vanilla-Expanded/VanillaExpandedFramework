using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class AdvancedColor
    {
        // Color sources
        public bool hairColor = false;
        public bool skinColor = false;
        public bool apparelStuff = false;
        public bool hostilityStatus = false;
        public bool factionColor = false;
        public bool primaryFactionIdeoColor = false;
        public bool ideoColor = false;
        public bool favoriteColor = false;
        public Color? color = null;

        public string taggedColor = null;

        // Color modifiers
        public float? saturation = null;
        public float? setHue = null;
        public float? hueRotate = null;
        public float? brightness = null;
        public bool invertBrightness = false;
        public float? invertValueIfBelow = null;
        public float? invertValueIfAbove = null;
        public float? minBrightness = 0;
        public float? maxBrightness = 1;

        public static readonly Color playerClr = new(0.6f, 0.6f, 1f);
        public static readonly Color enemyClr = new(1f, 0.2f, 0.2f);
        public static readonly Color neutralClr = new(0.45f, 0.8f, 1f);
        public static readonly Color slaveClr = new(1f, 0.9f, 0.4f);

        public bool AnyInvertBrightness => invertBrightness || invertValueIfAbove != null || invertValueIfBelow != null;
        public Color GetColor(PawnRenderNode renderNode, Color oldClr)
        {
            var pawn = renderNode.tree.pawn;
            List<Color> colorsAdded = [];
            var faction = pawn?.Faction;
            ColorFromBasic(colorsAdded);
            if (taggedColor is string tag)
            {
                if (pawn.GetColorByTag(tag) is TaggedColor tColor)
                {
                    colorsAdded.Add(tColor.value);
                }
            }
            if (pawn?.story != null)
            {
                if (hairColor)
                {
                    colorsAdded.Add(pawn.story.HairColor);
                }
                if (skinColor)
                {
                    colorsAdded.Add(pawn.story.SkinColor);
                }
            }
            if (apparelStuff && renderNode.apparel.Stuff is ThingDef stuff)
            {
                colorsAdded.Add(stuff.stuffProps.color);
            }
            if (faction != null)
            {
                ColorFromFaction(colorsAdded, faction);
            }

            if (ideoColor)
            {
                if (ModsConfig.IdeologyActive && pawn?.Ideo != null)
                {
                    colorsAdded.Add(pawn.Ideo.Color);
                }
                else if (faction != null) SetFactionColor(faction, colorsAdded);
            }

            if (favoriteColor && pawn.story?.favoriteColor is Color favoriteClr)
            {
                colorsAdded.Add(favoriteClr);
            }
            if (hostilityStatus)
            {
                GetHostilityStatus(pawn, ref colorsAdded);
            }

            Color finalClr = TransformAndFinalizeColor(oldClr, colorsAdded);
            return finalClr;
        }

        /// <summary>
        /// Called from VFEMedieval 2
        /// </summary>
        /// <returns>Color based on whatever is defined on the faction or it's def, if any.</returns>
        public Color GetColor(Faction faction)
        {
            var oldClr = Color.white;
            List<Color> colorsAdded = [];
            ColorFromBasic(colorsAdded);
            if (taggedColor is string tag)
            {
                if (faction.ColorByTag(tag) is TaggedColor tColor)
                {
                    colorsAdded.Add(tColor.value);
                }
            }
            ColorFromFaction(colorsAdded, faction);
            Color finalClr = TransformAndFinalizeColor(oldClr, colorsAdded);
            return finalClr;
        }

        private Color TransformAndFinalizeColor(Color oldClr, List<Color> colorsAdded)
        {
            Color finalClr;
            if (colorsAdded.Any())
            {
                float r = 0, g = 0, b = 0;
                foreach (var color in colorsAdded)
                {
                    r += color.r;
                    g += color.g;
                    b += color.b;
                }
                int count = colorsAdded.Count;
                finalClr = new Color(r / count, g / count, b / count);
            }
            else
            {
                finalClr = oldClr;
            }
            if (saturation != null || setHue != null || hueRotate != null || brightness != null ||
                minBrightness != null || maxBrightness != null || AnyInvertBrightness)
            {
                Color.RGBToHSV(finalClr, out float hue, out float sat, out float val);

                // Perceptual brightness.
                float pBright = 0.21f * finalClr.r + 0.72f * finalClr.g + 0.07f * finalClr.b;
                float iPBright = 1.0f - pBright;

                if (invertBrightness)
                {
                    if (pBright < 0.55f)
                    {
                        MakeBright(ref sat, ref val);
                    }
                    else
                    {
                        MarkDark(pBright, iPBright, ref sat, ref val);
                    }
                }
                else if (invertValueIfAbove != null && invertValueIfBelow < pBright)
                {
                    MakeBright(ref sat, ref val);
                }
                else if (invertValueIfAbove != null && invertValueIfAbove > pBright)
                {
                    MarkDark(pBright, iPBright, ref sat, ref val);
                }

                if (setHue != null)
                    hue = setHue.Value;
                if (hueRotate != null)
                    hue = Mathf.Repeat(hue + hueRotate.Value, 1f);

                if (saturation != null)
                    sat *= saturation.Value;

                if (brightness != null)
                    val *= brightness.Value;
                if (minBrightness != null)
                    val = Mathf.Max(minBrightness.Value, val);
                if (maxBrightness != null)
                    val = Mathf.Min(maxBrightness.Value, val);

                sat = Mathf.Clamp01(sat);
                val = Mathf.Clamp01(val);

                finalClr = Color.HSVToRGB(hue, sat, val);
            }

            return finalClr;
        }

        private void ColorFromBasic(List<Color> colorsAdded)
        {
            if (color != null)
            {
                colorsAdded.Add(color.Value);
            }
        }

        private void ColorFromFaction(List<Color> colorsAdded, Faction faction)
        {
            if (factionColor)
            {
                SetFactionColor(faction, colorsAdded);
            }
            if (primaryFactionIdeoColor)
            {
                if (faction?.FactionOrIdeoColor() is Color ideoClr)
                {
                    colorsAdded.Add(ideoClr);
                }
            }
        }

        private static void MarkDark(float pBright, float iPBright, ref float sat, ref float val)
        {
            val = Mathf.Min(val * iPBright / pBright, Mathf.Lerp(val, 0.3f, 0.65f));
            sat = Mathf.Lerp(sat, Mathf.Min(1, sat * 1.4f), 0.5f); // Saturate so it lookss less greyed/shadowed.
        }

        private static void MakeBright(ref float sat, ref float val)
        {
            val = Mathf.Lerp(val, 1.0f, 0.85f);
            sat *= 0.78f; // Desaturate to wash it out a bit to avoid oversaturation.
        }

        private static void SetFactionColor(Faction faction, List<Color> colorsAdded)
        {
            colorsAdded.Add(faction.Color);
        }

        static void GetHostilityStatus(Pawn pawn, ref List<Color> finalClr)
        {
            var pStatus = pawn.GuestStatus;
            if (pStatus == GuestStatus.Prisoner)
            {
                finalClr.Add(slaveClr);
            }
            else if (pStatus == GuestStatus.Slave)
            {
                finalClr.Add(slaveClr);
            }
            else if (pStatus == GuestStatus.Guest)
            {
                finalClr.Add(neutralClr);
            }
            else if (pawn.HostileTo(Faction.OfPlayer))
            {
                finalClr.Add(enemyClr);
            }
            else if (pawn.Faction != Faction.OfPlayer)
            {
                finalClr.Add(playerClr);
            }
        }
    }
}
