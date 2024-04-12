using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace GraphicCustomization
{
    [HotSwappableAttribute]
    [StaticConstructorOnStartup]
    public class Dialog_GraphicCustomization : Window
    {
        public static Texture2D RandomizeIcon = ContentFinder<Texture2D>.Get("UI/Widgets/RandomizeIcon");
        
        public CompGraphicCustomization comp;
        public override Vector2 InitialSize => new Vector2(700, 500);

        public Texture currentTexture;

        public List<TextureVariant> currentVariants;

        public string currentName;

        public CompGeneratedNames compGeneratedName;

        public Pawn pawn;
        
        public Dialog_GraphicCustomization(CompGraphicCustomization comp, Pawn pawn = null)
        {
            Init(comp);
            this.pawn = pawn;
            this.forcePause = true;
        }
        public void Init(CompGraphicCustomization comp)
        {
            this.comp = comp;
            comp.TryInit();
            this.currentVariants = comp.texVariants;
            UpdateTexture();
            compGeneratedName = this.comp.parent.GetComp<CompGeneratedNames>();
            if (compGeneratedName != null)
            {
                currentName = compGeneratedName.Name;
            }
        }

        private void UpdateTexture()
        {
            var texPaths = comp.GetTexPaths(this.currentVariants);
            currentTexture = comp.GetCombinedTexture(texPaths);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = DrawTitle(ref inRect);
            var height = GetScrollHeight();
            Rect outerRect = new Rect(inRect.x, titleRect.yMax + 30, inRect.width, 350);
            Rect viewArea = new Rect(inRect.x, outerRect.y, inRect.width - 16, height);
            Rect itemTextureRect = new Rect(inRect.x + 10, viewArea.y, 250, 250);

            DrawItem(itemTextureRect);
            Widgets.BeginScrollView(outerRect, ref scrollPosition, viewArea, true);
            DrawCustomizationArea(itemTextureRect);
            Widgets.EndScrollView();

            var cancelRect = new Rect((inRect.width / 2f) - 155, inRect.height - 32, 150, 32);
            if (Widgets.ButtonText(cancelRect, "VEF.Cancel".Translate()))
            {
                this.Close();
            }
            var confirmRect = new Rect((inRect.width / 2f) + 5, inRect.height - 32, 150, 32);
            DrawConfirmButton(confirmRect, "Confirm".Translate(), delegate
            {
                this.comp.texVariantsToCustomize = this.currentVariants;
                if (compGeneratedName != null)
                {
                    compGeneratedName.name = currentName;
                }
                this.pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(GraphicCustomization_DefOf.VEF_CustomizeItem, this.comp.parent));
                this.Close();
            });
        }

        protected void DrawItem(Rect itemTextureRect)
        {
            Widgets.DrawMenuSection(itemTextureRect);

            GUI.color = this.comp.parent.DrawColor;
            GUI.DrawTexture(itemTextureRect.ContractedBy(15f), currentTexture);
            GUI.color = Color.white;

            Widgets.InfoCardButton(itemTextureRect.xMax - 60, itemTextureRect.yMax - 30, this.comp.parent);
            var randomizeRect = new Rect(itemTextureRect.xMax - 30, itemTextureRect.yMax - 30, 24, 24);
            if (Widgets.ButtonImage(randomizeRect, RandomizeIcon))
            {
                Randomize();
            }
        }

        public Rect DrawTitle(ref Rect inRect)
        {
            var titleRect = new Rect(inRect.x, inRect.y, inRect.width, 40);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(titleRect, comp.Props.customizationTitle ?? "VEF.CustomizationTitle".Translate(comp.parent.LabelCapNoCount));
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            return titleRect;
        }
        public void DrawConfirmButton(Rect confirmRect, string confirmLabel, Action action)
        {
            if (Widgets.ButtonText(confirmRect, confirmLabel))
            {
                action();
            }
        }

        protected virtual void DrawCustomizationArea(Rect itemTextureRect)
        {
            var position = new Vector2(itemTextureRect.xMax + 25, itemTextureRect.y);
            if (compGeneratedName != null)
            {
                var nameRect = new Rect(position.x, position.y - 3, 350, 25);
                Widgets.Label(nameRect, "VEF.Name".Translate() + ": ");
                position.y += 25;
                var nameInputRect = new Rect(nameRect.x, nameRect.yMax, 350, 32);
                currentName = Widgets.TextField(nameInputRect, currentName);
                position.y += 40;
            }

            foreach (var graphicPart in comp.Props.graphics)
            {
                var nameRect = new Rect(position.x, position.y, 350, 25);
                Widgets.Label(nameRect, graphicPart.name + ": ");
                position.y += 25;
                var floatMenuButtonsRect = new Rect(position.x, position.y, 350, 32);
                var graphicPartVariant = graphicPart.texVariants.First(x => currentVariants.Contains(x));
                var currentVariant = currentVariants.First(x => graphicPart.texVariants.Contains(x));
                var index = graphicPart.texVariants.IndexOf(graphicPartVariant);

                MakeFloatOptionButtons(floatMenuButtonsRect,
                    leftAction: delegate
                    {
                        if (index > 0)
                        {
                            index--;
                        }
                        else
                        {
                            index = graphicPart.texVariants.Count - 1;
                        }
                        graphicPartVariant = graphicPart.texVariants[index];
                        currentVariants.Replace(currentVariant, graphicPartVariant);
                        UpdateTexture();
                    },
                    centerAction: delegate
                    {
                        FloatMenuUtility.MakeMenu(graphicPart.texVariants, entry => entry.texName, (TextureVariant variant) => delegate
                        {
                            currentVariants.Replace(currentVariant, variant);
                            UpdateTexture();
                        });
                    }, centerButtonName: graphicPartVariant.texName,
                    rightAction: delegate
                    {
                        if (index < graphicPart.texVariants.Count - 1)
                        {
                            index++;
                        }
                        else
                        {
                            index = 0;
                        }

                        graphicPartVariant = graphicPart.texVariants[index];
                        currentVariants.Replace(currentVariant, graphicPartVariant);
                        UpdateTexture();
                    });
                position.y += 45;
            }
        }
        protected virtual void Randomize()
        {
            this.currentVariants = comp.GetRandomizedTexVariants();
            if (this.compGeneratedName != null)
            {
                this.currentName = CompGeneratedNames.GenerateName(this.compGeneratedName.Props);
            }
            var texPaths = comp.GetTexPaths(this.currentVariants);
            currentTexture = comp.GetCombinedTexture(texPaths);
        }

        public void MakeFloatOptionButtons(Rect floatMenuButtonsRect, Action leftAction, Action centerAction, string centerButtonName, Action rightAction)
        {
            Widgets.DrawHighlight(floatMenuButtonsRect);
            var btnChangeArrowLeft = new Rect(floatMenuButtonsRect.x, floatMenuButtonsRect.y, 32, 32);
            var btnChangeArrowRight = new Rect(floatMenuButtonsRect.xMax - 32, floatMenuButtonsRect.y, 32, 32);
            var btnChangeSelection = new Rect(floatMenuButtonsRect.x + 32, floatMenuButtonsRect.y, floatMenuButtonsRect.width - 64, 32);

            if (ButtonTextSubtleCentered(btnChangeArrowLeft, "<"))
            {
                leftAction();
            }

            if (ButtonTextSubtleCentered(btnChangeSelection, centerButtonName))
            {
                centerAction();
            }

            if (ButtonTextSubtleCentered(btnChangeArrowRight, ">"))
            {
                rightAction();
            }
        }

        public virtual float GetScrollHeight()
        {
            float num = 0;
            if (this.comp.parent.GetComp<CompGeneratedNames>() != null)
            {
                num += 25 + 45;
            }
            foreach (var def in comp.Props.graphics)
            {
                num += 25 + 45;
            }
            return num;
        }

        public static bool ButtonTextSubtleCentered(Rect rect, string label, Vector2 functionalSizeOffset = default(Vector2))
        {
            Rect rect2 = rect;
            rect2.width += functionalSizeOffset.x;
            rect2.height += functionalSizeOffset.y;
            bool flag = false;
            if (Mouse.IsOver(rect2))
            {
                flag = true;
                GUI.color = GenUI.MouseoverColor;
            }
            Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
            GUI.color = Color.white;
            Rect rect3 = new Rect(rect);
            if (flag)
            {
                rect3.x += 2f;
                rect3.y -= 2f;
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            Text.Font = GameFont.Small;
            Widgets.Label(rect3, label);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = true;
            var result =  Widgets.ButtonInvisible(rect2, false);
            Text.Anchor = TextAnchor.UpperLeft;
            return result;
        }
        
        public static Vector2 scrollPosition = Vector2.zero;
    }
}
