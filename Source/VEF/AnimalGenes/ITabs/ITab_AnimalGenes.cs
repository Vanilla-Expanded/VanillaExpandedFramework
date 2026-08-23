using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VEF.AnimalGenes
{
    public class ITab_AnimalGenes : ITab
    {
        protected Vector2 scrollPosition;

        protected const float TopPadding = 20f;

        public const float GeneSize = 90f;

        public const float GeneGap = 6f;

        public const int MaxGenesHorizontal = 7;

        public const float InitialWidth = 736f;

        protected const float InitialHeight = 550f;

        private static float scrollHeight;
        private static float genesHeight;

        public override bool IsVisible => CanShowGenesTab();

        private static readonly CachedTexture GeneBackground_Awful = new CachedTexture("UI/AnimalGenes/AnimalGeneBackground_Awful");
        private static readonly CachedTexture GeneBackground_Poor = new CachedTexture("UI/AnimalGenes/AnimalGeneBackground_Poor");
        private static readonly CachedTexture GeneBackground_Baseline = new CachedTexture("UI/AnimalGenes/AnimalGeneBackground_Average");
        private static readonly CachedTexture GeneBackground_Good = new CachedTexture("UI/AnimalGenes/AnimalGeneBackground_Good");
        private static readonly CachedTexture GeneBackground_Excellent = new CachedTexture("UI/AnimalGenes/AnimalGeneBackground_Perfect");
        private static readonly CachedTexture Stability = new CachedTexture("UI/VRE_Stability");

        protected Thing SelPawnForGenes => ThingForGenes(SelThing);

        public ITab_AnimalGenes()
        {
            size = new Vector2(Mathf.Min(736f, UI.screenWidth), 550f);
            labelKey = "VRE_TabAnimalGenes";
        }

        protected override void FillTab()
        {
            DrawGenesInfo(new Rect(0f, 20f, size.x, size.y - 20f), Find.Selector.SingleSelectedThing, 550f, ref size, ref scrollPosition);
        }

        private static Thing ThingForGenes(Thing thing)
        {
            Pawn pawn = thing as Pawn;
            if (pawn != null)
            {
                return pawn;
            }
            Corpse corpse = thing as Corpse;
            if (corpse != null)
            {
                return corpse.InnerPawn;
            }
            if (thing.HasThingCategory(ThingCategoryDefOf.EggsFertilized))
            {
                return thing;
            }
            return null;
        }


        public static bool CanShowGenesTab()
        {

            Thing thing = ThingForGenes(Find.Selector.SingleSelectedThing);

            if (thing != null && WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(thing))
            {
                return true;
            }

            return false;
        }

        public static void DrawGenesInfo(Rect rect, Thing target, float initialHeight, ref Vector2 size, ref Vector2 scrollPosition, GeneSet pregnancyGenes = null)
        {
            Thing sourcePawn = ThingForGenes(target);
            if (sourcePawn == null || !WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(sourcePawn))
            {
                return;
            }
            CompAnimalGenes comp = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[sourcePawn];
            if (comp is null)
            {
                return;
            }
            int totalStability = AnimalGeneUtility.GetTotalStability(comp);

            Rect rect2 = rect;
            Rect position = rect2.ContractedBy(10f);
            GUI.BeginGroup(position);
            float num = Text.LineHeight * 3f;
            Rect rect3 = new Rect(0f, 0f, position.width, position.height - num - 12f);
            DrawAnimalGeneSections(rect3, target, comp, ref scrollPosition);
            Rect rect4 = new Rect(0f, rect3.yMax + 6f, position.width - 140f - 4f, num);
            rect4.yMax = rect3.yMax + num + 6f;
            Rect rect5 = new Rect(0f, rect3.yMax + 6f, position.width - 440f, num);
            TryDrawStability(rect5, totalStability);
            Rect rect6 = new Rect(rect5.xMax, rect3.yMax + 6f, 250, num);
            TryDrawLifespanFactor(rect6, comp);
            TryDrawFeratype(target, rect4.xMax + 4f, rect4.y + Text.LineHeight / 2f, comp);
            GUI.EndGroup();
        }

        public static void TryDrawStability(Rect rect, int totalStability)
        {

            Rect rect2 = rect;
            Rect position = rect2.ContractedBy(10f);
            Widgets.DrawHighlightIfMouseover(position);
            TaggedString taggedString = "VRE_StabilityDesc".Translate();
            TooltipHandler.TipRegion(position, taggedString);
            GUI.BeginGroup(rect);

            GUI.DrawTexture(new Rect(16, 16f, 16, 16), Stability.Texture);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(32, 8f, 100, 32), "VRE_Stability".Translate().CapitalizeFirst());
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(200, 8f, 90f, 32), totalStability.ToString());
            GUI.EndGroup();
        }

        public static void TryDrawLifespanFactor(Rect rect, CompAnimalGenes comp)
        {
            float totalLifespanFactor = comp.LifeSpanFactor;
            Rect rect2 = rect;
            Rect position = rect2.ContractedBy(10f);
            Widgets.DrawHighlightIfMouseover(position);
            TaggedString taggedString = "VRE_LifespanFactorDesc".Translate();
            TooltipHandler.TipRegion(position, taggedString);
            GUI.BeginGroup(rect);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(32, 8f, 100, 32), "VRE_LifespanFactor".Translate().CapitalizeFirst() + ":");

            Widgets.Label(new Rect(90, 8f, 90f, 32), "x" + totalLifespanFactor.ToStringPercent());
            GUI.EndGroup();
        }

        private static void TryDrawFeratype(Thing target, float x, float y, CompAnimalGenes comp)
        {

            Rect rect = new Rect(x, y, 140f, Text.LineHeight);
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, comp.feratype.label.CapitalizeFirst());
            Text.Anchor = TextAnchor.UpperLeft;
            Rect position = new Rect(rect.center.x - 17f, rect.yMax + 4f, 34f, 34f);
            GUI.color = XenotypeDef.IconColor;
            GUI.DrawTexture(position, comp.feratype.Icon);
            GUI.color = Color.white;
            rect.yMax = position.yMax;
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, () => ("VRE_Feratype".Translate() + ": " + comp.feratype.label.CapitalizeFirst()).Colorize(ColoredText.TipSectionTitleColor).CapitalizeFirst() + "\n\n" + FeratypeDef.FeratypeDescWithExtra(comp.feratype) + "\n\n"
                + "VRE_FeratypeFamily".Translate().Colorize(ColoredText.TipSectionTitleColor).CapitalizeFirst() + ": " + comp.feratype.feratypeFamily.LabelCap, 883938493);
            }
            if (Widgets.ButtonInvisible(rect))
            {
                Find.WindowStack.Add(new Dialog_InfoCard(comp.feratype));
            }
        }


        private static void DrawAnimalGeneSections(Rect rect, Thing target, CompAnimalGenes comp, ref Vector2 scrollPosition)
        {

            GUI.BeginGroup(rect);
            Rect rect2 = new Rect(0f, 0f, rect.width - 16f, scrollHeight);
            float curY = 0f;
            Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, rect2);
            Rect containingRect = rect2;
            containingRect.y = scrollPosition.y;
            containingRect.height = rect.height;

            List<AnimalGeneDef> reorderedGenes = comp.genes.OrderBy(x => x.familyTag.order).ToList();

            DrawSection(rect, reorderedGenes.Count, ref curY, ref genesHeight, delegate (int i, Rect r)
            {
                DrawGene(reorderedGenes[i], r);
            }, containingRect);
            curY += 12f;

            if (Event.current.type == EventType.Layout)
            {
                scrollHeight = curY;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private static void DrawSection(Rect rect, int count, ref float curY, ref float sectionHeight, Action<int, Rect> drawer, Rect containingRect)
        {
            Widgets.Label(10f, ref curY, rect.width, "VRE_AnimalGenes".Translate(), "VRE_AnimalGenesDesc".Translate());
            float num = curY;
            Rect rect2 = new Rect(rect.x, curY, rect.width, sectionHeight);

            Widgets.DrawMenuSection(rect2);
            float num2 = (rect.width - 12f - 630f - 36f) / 2f;
            curY += num2;
            int num3 = 0;
            int num4 = 0;
            for (int i = 0; i < count; i++)
            {
                if (num4 >= 6)
                {
                    num4 = 0;
                    num3++;
                }
                else if (i > 0)
                {
                    num4++;
                }
                Rect rect3 = new Rect(num2 + (float)num4 * 90f + (float)num4 * 6f, curY + (float)num3 * 90f + (float)num3 * 6f, 90f, 90f);
                if (containingRect.Overlaps(rect3))
                {
                    drawer(i, rect3);
                }
            }
            curY += (float)(num3 + 1) * 90f + (float)num3 * 6f + num2;

            if (Event.current.type == EventType.Layout)
            {
                sectionHeight = curY - num;
            }
        }
        public static void DrawGene(AnimalGeneDef gene, Rect geneRect, bool doBackground = true, bool clickable = true)
        {
            DrawGeneBasics(gene, geneRect, doBackground, clickable);
            if (Mouse.IsOver(geneRect))
            {
                string text = gene.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + gene.DescriptionFull;

                if (clickable)
                {
                    text = text + "\n\n" + "ClickForMoreInfo".Translate().ToString().Colorize(ColoredText.SubtleGrayColor);
                }
                TooltipHandler.TipRegion(geneRect, text);
            }
        }

        private static void DrawGeneBasics(AnimalGeneDef gene, Rect geneRect, bool doBackground, bool clickable)
        {
            GUI.BeginGroup(geneRect);
            Rect rect = geneRect.AtZero();
            if (doBackground)
            {
                Widgets.DrawHighlight(rect);
                GUI.color = new Color(1f, 1f, 1f, 0.05f);
                Widgets.DrawBox(rect);
                GUI.color = Color.white;
            }
            float num = rect.width - Text.LineHeight;
            Rect rect2 = new Rect(geneRect.width / 2f - num / 2f, 0f, num, num);
            Color iconColor = gene.IconColor;

            CachedTexture cachedTexture = GetBackGround(gene.stability);

            GUI.DrawTexture(rect2, cachedTexture.Texture);
            GUI.color = gene.IconColor;
            Widgets.DrawTextureFitted(rect2, gene.Icon, 0.9f);
            GUI.color = Color.white;

            Text.Font = GameFont.Tiny;
            float num2 = Text.CalcHeight(gene.LabelCap, rect.width);
            Rect rect3 = new Rect(0f, rect.yMax - num2, rect.width, num2);
            GUI.DrawTexture(new Rect(rect3.x, rect3.yMax - num2, rect3.width, num2), TexUI.GrayTextBG);
            Text.Anchor = TextAnchor.LowerCenter;

            if (doBackground && num2 < (Text.LineHeight - 2f) * 2f)
            {
                rect3.y -= 3f;
            }
            Widgets.Label(rect3, gene.LabelCap);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            if (clickable)
            {
                if (Widgets.ButtonInvisible(rect))
                {
                    Find.WindowStack.Add(new Dialog_InfoCard(gene));
                }
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                }
            }
            GUI.EndGroup();
        }

        public static CachedTexture GetBackGround(int stability)
        {
            switch (stability)
            {
                case 2:
                    return GeneBackground_Awful;

                case 1:
                    return GeneBackground_Poor;

                case -1:
                    return GeneBackground_Good;

                case -2:
                    return GeneBackground_Excellent;

            }
            return GeneBackground_Baseline;
        }

    }
}
