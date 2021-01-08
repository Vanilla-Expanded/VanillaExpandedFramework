using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
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
        private float buttonOffset = 20f;

        public VFEGlobal(ModContentPack content) : base(content)
        {
            settings = GetSettings<VFEGlobalSettings>();
        }

        public override string SettingsCategory() => "Vanilla Framework Expanded";

        #region Pages
        private enum Pages // Add pages here
        {
            PatchOperationToggable = 1
        }

        private enum PagesHeadTitle // Add language data here, in the right order
        {
            TPTitle = 1
        }

        private int MaxIndex = Enum.GetNames(typeof(Pages)).Length;
        private int PageIndex = 1;
        #endregion

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
        #endregion

        #region Toggable Patches
        private void AddToggablePatchesSettings(Listing_Standard list)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            list.Label("NeedRestart".Translate());
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
        #endregion

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

            this.MakePageHead(list);
            #region settings
            if (PageIndex == (int)Pages.PatchOperationToggable) this.AddToggablePatchesSettings(list);
            #endregion

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
