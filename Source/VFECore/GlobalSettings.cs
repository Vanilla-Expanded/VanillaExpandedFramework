using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            settings = GetSettings<VFEGlobalSettings>();
            ToggablePatchCount = LoadedModManager.RunningMods.ToList().FindAll(mcp => mcp.Patches.ToList().FindAll(p => p is PatchOperationToggable pCasted && pCasted != null).Any()).Count;
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

        private void AddToggablePatchesSettings(Listing_Standard list)
        {
            this.MakePageHead(list);

            Text.Anchor = TextAnchor.MiddleCenter;
            list.Label("NeedRestart".Translate());
            list.Label("XPatchFound".Translate(ToggablePatchCount));
            Text.Anchor = TextAnchor.UpperLeft;
            list.Gap();
            foreach (ModContentPack modContentPack in (from m in LoadedModManager.RunningMods orderby m.OverwritePriority select m).ThenBy((ModContentPack x) => LoadedModManager.RunningModsListForReading.IndexOf(x)))
            {
                this.AddButton(list, modContentPack);
            }
        }

        private void AddButton(Listing_Standard list, ModContentPack modContentPack)
        {
            if (modContentPack != null)
            {
                foreach (PatchOperation patchOperation in modContentPack.Patches)
                {
                    if (patchOperation != null && patchOperation is PatchOperationToggable p && p != null)
                    {
                        bool flag = false;
                        for (int i = 0; i < p.mods.Count; i++)
                        {
                            if (ModLister.HasActiveModWithName(p.mods[i]))
                            {
                                flag = true;
                            }
                            else
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            if (list.ButtonTextLabeled(p.label, p.enabled.ToString()))
                            {
                                XmlDocument xmlDocument = new XmlDocument();
                                xmlDocument.Load(p.sourceFile);

                                string xpath = "Patch/Operation[@Class=\"VFECore.PatchOperationToggable\" and label=\"" + p.label + "\"]/enabled/text()";
                                if (p.enabled) { xmlDocument.SelectSingleNode(xpath).Value = "False"; p.enabled = false; }
                                else { xmlDocument.SelectSingleNode(xpath).Value = "True"; p.enabled = true; }

                                File.WriteAllText(p.sourceFile, GlobalSettingsUtilities.PrettyXml(xmlDocument.OuterXml));
                            }
                        }
                    }
                }
            }
        }

        #endregion Toggable Patches

        #region Faction Discovery

        private int FactionCanBeAddedCount;

        private void AddFactionDiscoverySettings(Listing_Standard list)
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

            Listing_Standard list = new Listing_Standard();
            Widgets.BeginScrollView(inRect, ref scrollPosition, inRect, true);
            list.Begin(inRect);

            #region settings

            if (PageIndex == (int)Pages.FactionDiscovery) this.AddFactionDiscoverySettings(list);
            else if (PageIndex == (int)Pages.PatchOperationToggable) this.AddToggablePatchesSettings(list);

            #endregion settings

            list.End();
            Widgets.EndScrollView();
            settings.Write();
        }
    }

    public class VFEGlobalSettings : ModSettings
    {
        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}