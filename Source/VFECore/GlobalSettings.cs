using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore
{
    public class VFEGlobal : Mod
    {
        public static VFEGlobalSettings settings;
        private Vector2 scrollPosition = Vector2.zero;
        protected readonly Vector2 ButtonSize = new Vector2(120f, 40f);
        private readonly float buttonOffset = 20f;

        public VFEGlobal(ModContentPack content) : base(content)
        {
            settings           = GetSettings<VFEGlobalSettings>();
            foreach (ModContentPack mod in LoadedModManager.RunningMods)
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
        }

        public override string SettingsCategory() => "Vanilla Framework Expanded";

        #region Pages

        private enum Pages // Add pages here
        {
            FactionDiscovery = 1,
            PatchOperationToggable = 2
        }

        private enum PagesHeadTitle // Add language data here, in the right order
        {
            FDTitle = 1,
            TPTitle = 2
        }

        private readonly int MaxIndex = Enum.GetNames(typeof(Pages)).Length;
        private int PageIndex = 1;

        #endregion Pages

        #region Page Head

        private void MakePageHead(Listing_Standard list)
        {
            list.Gap(20);
            var title = (PagesHeadTitle)PageIndex;
            Text.Font = GameFont.Medium;
            list.Label(title.ToString().Translate());
            Text.Font = GameFont.Small;
            list.Gap();
            // list.GapLine();
        }

        #endregion Page Head

        #region Toggable Patches

        private int ToggablePatchCount;
        private int ModUsingToggablePatchCount = 0;

        private void AddToggablePatchesSettings(Listing_Standard list)
        {
            this.MakePageHead(list);

            Text.Anchor = TextAnchor.MiddleCenter;
            list.Label("NeedRestart".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            list.Gap();

            foreach (ModContentPack modContentPack in (from m in LoadedModManager.RunningMods orderby m.OverwritePriority select m).ThenBy((ModContentPack x) => LoadedModManager.RunningModsListForReading.IndexOf(x)))
            {
                if (modContentPack?.Patches != null && modContentPack.Patches.Any(p => p is PatchOperationToggableSequence pt && pt.ModsFound()))
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    list.Label(modContentPack.Name);
                    Text.Anchor = TextAnchor.UpperLeft;
                    this.AddButton(list, modContentPack);
                }
            }
        }

        private void AddButton(Listing_Standard list, ModContentPack modContentPack)
        {
            foreach (PatchOperation patchOperation in modContentPack.Patches)
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
                            if (settings.toggablePatch.NullOrEmpty()) settings.toggablePatch = new Dictionary<string, bool>();
                            settings.toggablePatch.Add(pLabelSmall, !p.enabled);
                        }
                    }
                }
            }
        }

        #endregion Toggable Patches

        #region Faction Discovery / KCSG

        private int FactionCanBeAddedCount;

        private void AddFSKCSGSettings(Listing_Standard list)
        {
            this.MakePageHead(list);

            if (Current.Game != null)
            {
                FactionCanBeAddedCount = DefDatabase<FactionDef>.AllDefs.Where(ValidatorAnyFactionLeft).Count();
                list.Label("CanAddXFaction".Translate(FactionCanBeAddedCount));
                if (FactionCanBeAddedCount > 0 && list.ButtonText("AskForPopUp".Translate(), "AskForPopUpExplained".Translate()))
                {
                    Current.Game.World.GetComponent<NewFactionSpawningState>().ignoredFactions.Clear();
                    IEnumerator<FactionDef> factionEnumerator = DefDatabase<FactionDef>.AllDefs.Where(Patch_GameComponentUtility.LoadedGame.Validator).GetEnumerator();
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
            // KCSG
            list.Gap(20);
            Text.Font = GameFont.Medium;
            list.Label("Custom Structure Generation");
            Text.Font = GameFont.Small;
            list.Gap();
            list.CheckboxLabeled("Verbose logging", ref settings.enableVerboseLogging);

            // General
            list.Gap(20);
            Text.Font = GameFont.Medium;
            list.Label("General Settings");
            Text.Font = GameFont.Small;
            list.Gap();
            list.CheckboxLabeled("Disable Texture Caching", ref settings.disableCaching, "Warning: Enabling this might cause performance issues.");
            
            
        }

        private bool ValidatorAnyFactionLeft(FactionDef faction)
        {
            if (faction == null) return false;
            if (faction.isPlayer) return false;
            if (!faction.canMakeRandomly && faction.hidden && faction.maxCountAtGameStart <= 0) return false;
            if (Find.FactionManager.AllFactions.Count(f => f.def == faction) > 0) return false;
            if (NewFactionSpawningUtility.NeverSpawn(faction)) return false;
            return true;
        }

        #endregion Faction Discovery

        #region Texture Variations

       
        private void AddTextureVariations(Listing_Standard list)
        {
           
            // Texture Variations
           
            list.Gap(20);
            Text.Font = GameFont.Medium;
            list.Label("Texture Variations");
            Text.Font = GameFont.Small;
            list.Gap();
            list.CheckboxLabeled("VFE_RandomOrSequentially".Translate(), ref settings.isRandomGraphic, null);
            list.Gap(12f);
            list.CheckboxLabeled("VFE_HideRandomizeButton".Translate(), ref settings.hideRandomizeButton, null);
            list.Gap(12f);



        }



        #endregion Texture Variations

        private void AddPageButtons(Rect rect)
        {
            Rect leftButtonRect = new Rect(rect.width / 2f - this.ButtonSize.x / 2f - this.ButtonSize.x - buttonOffset, rect.height + this.ButtonSize.y + 2, this.ButtonSize.x, this.ButtonSize.y);
            if (Widgets.ButtonText(leftButtonRect, "Previous Page"))
            {
                SoundDefOf.Click.PlayOneShot(null);
                if (PageIndex - 1 >= 1) PageIndex--;
            }

            Rect rightButtonRect = new Rect(rect.width / 2f + this.ButtonSize.x / 2f + buttonOffset, rect.height + this.ButtonSize.y + 2, this.ButtonSize.x, this.ButtonSize.y);
            if (Widgets.ButtonText(rightButtonRect, "Next Page"))
            {
                SoundDefOf.Click.PlayOneShot(null);
                if (PageIndex + 1 <= MaxIndex) PageIndex++;
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            this.AddPageButtons(inRect);

            Rect viewRect = new Rect(inRect);
            if (PageIndex == (int)Pages.PatchOperationToggable)
            {
                viewRect.height = 120f;
                viewRect.height += (ToggablePatchCount + ModUsingToggablePatchCount) * 20f;
                viewRect.width -= 20f;
            }

            Listing_Standard list = new Listing_Standard();
            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect, true);
            list.Begin(viewRect);

            #region settings
            switch (PageIndex)
            {
                case (int)Pages.FactionDiscovery:
                    this.AddFSKCSGSettings(list);
                    this.AddTextureVariations(list);
                    break;
                case (int)Pages.PatchOperationToggable:
                    this.AddToggablePatchesSettings(list);
                    break;
                default:
                    break;
            }

            #endregion settings
            list.End();
            Widgets.EndScrollView();
            settings.Write();
        }
    }

    public class VFEGlobalSettings : ModSettings
    {
        public Dictionary<string, bool> toggablePatch = new Dictionary<string, bool>();
        public bool enableVerboseLogging;
        public bool disableCaching;
        public  bool isRandomGraphic = true;
        public  bool hideRandomizeButton = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref toggablePatch, "toggablePatch", LookMode.Value);
            Scribe_Values.Look(ref enableVerboseLogging,  "enableVerboseLogging", false);
            Scribe_Values.Look(ref this.disableCaching, "disableCaching", true);
            Scribe_Values.Look(ref isRandomGraphic, "isRandomGraphic", true, true);
            Scribe_Values.Look(ref hideRandomizeButton, "hideRandomizeButton", false, true);
        }
    }
}