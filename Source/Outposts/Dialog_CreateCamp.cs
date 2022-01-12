using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Outposts
{
    public class Dialog_CreateCamp : Window
    {
        private const float LINE_HEIGHT = 100f;
        private readonly Caravan creator;
        private readonly Dictionary<WorldObjectDef, Pair<string, string>> validity;
        private float? prevHeight;
        private Vector2 scrollPosition = new(0, 0);

        public Dialog_CreateCamp(Caravan creator)
        {
            doCloseButton = true;
            doCloseX = true;
            doWindowBackground = true;
            this.creator = creator;
            validity = new Dictionary<WorldObjectDef, Pair<string, string>>();
            foreach (var outpost in OutpostsMod.Outposts)
            {
                var method = outpost.worldObjectClass.GetMethod("CanSpawnOnWith", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var method2 = outpost.worldObjectClass.GetMethod("RequirementsString", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var ext = outpost.GetModExtension<OutpostExtension>();
                var valid = ext?.CanSpawnOnWithExt(creator.Tile, creator.HumanColonists()) ??
                            (string) method?.Invoke(null, new object[] {creator.Tile, creator.HumanColonists()});
                var tip = (ext?.RequirementsStringBase(creator.Tile, creator.HumanColonists()) ?? "" +
                    ((string) method2?.Invoke(null, new object[] {creator.Tile, creator.HumanColonists()}) ?? "")).TrimEndNewlines();

                validity.Add(outpost, new Pair<string, string>(valid, tip));
            }
        }

        public override Vector2 InitialSize => new(800, Mathf.Min(1000f, UI.screenHeight - 200f));

        public override void DoWindowContents(Rect inRect)
        {
            var outRect = inRect.ContractedBy(5f);
            outRect.height -= 45f;
            var viewRect = new Rect(0, 0, outRect.width - 50f, prevHeight ?? OutpostsMod.Outposts.Count * (LINE_HEIGHT + 10));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            var rect = new Rect(10, 0, viewRect.width, LINE_HEIGHT);
            foreach (var outpost in OutpostsMod.Outposts)
            {
                DoOutpostDisplay(ref rect, outpost);
                rect.y += rect.height + 5f;
                Widgets.DrawLineHorizontal(rect.x, rect.y, rect.width);
                rect.y += 5f;
            }

            prevHeight = rect.y;

            Widgets.EndScrollView();
        }

        private void DoOutpostDisplay(ref Rect inRect, WorldObjectDef outpostDef)
        {
            var font = Text.Font;
            var anchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            inRect.height = Text.CalcHeight(outpostDef.description, inRect.width - 90f) + 60f;
            var image = inRect.LeftPartPixels(50f);
            var text = inRect.RightPartPixels(inRect.width - 60f);
            var tex = outpostDef.ExpandingIconTexture;
            GUI.color = creator.Faction.Color;
            Widgets.DrawTextureFitted(image, tex, 1f, new Vector2(tex.width, tex.height), new Rect(0f, 0f, 1f, 1f));
            GUI.color = Color.white;
            Text.Font = GameFont.Medium;
            Widgets.Label(text.TopPartPixels(30f), outpostDef.label.CapitalizeFirst(outpostDef));
            var button = text.BottomPartPixels(30f).LeftPartPixels(100f);
            var errorText = text.BottomPartPixels(30f).RightPartPixels(text.width - 120f);
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(text.x, text.y + 30f, text.width, text.height - 60f), outpostDef.description);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(errorText, validity[outpostDef].First);
            Text.Font = font;
            Text.Anchor = anchor;
            if (Widgets.ButtonText(button, "Outposts.Dialog.Create".Translate()))
            {
                if (validity[outpostDef].First.NullOrEmpty())
                {
                    var outpost = (Outpost) WorldObjectMaker.MakeWorldObject(outpostDef);
                    outpost.Name = NameGenerator.GenerateName(creator.Faction.def.settlementNameMaker, Find.WorldObjects.AllWorldObjects.OfType<Outpost>().Select(o => o.Name));
                    outpost.Tile = creator.Tile;
                    outpost.SetFaction(creator.Faction);
                    Find.WorldObjects.Add(outpost);
                    foreach (var pawn in creator.PawnsListForReading.ListFullCopy()) outpost.AddPawn(pawn);
                    Close();
                    Find.WorldSelector.Select(outpost);
                }
                else
                    Messages.Message(validity[outpostDef].First, MessageTypeDefOf.RejectInput, false);
            }

            TooltipHandler.TipRegion(inRect, validity[outpostDef].Second);
        }
    }
}