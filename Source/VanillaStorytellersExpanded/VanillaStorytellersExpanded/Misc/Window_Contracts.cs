using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaStorytellersExpanded
{
    [StaticConstructorOnStartup]
    public class Window_Contracts : Window
    {
        private QuestGiverManager questGiverManager;

        private QuestInfo selected;

        private Vector2 scrollPosition_available;

        private Vector2 selectedQuestScrollPosition;

        private float selectedQuestLastHeight;

        private List<QuestPart> tmpQuestParts = new List<QuestPart>();

        private static readonly Color AcceptanceRequirementsColor = new Color(1f, 0.25f, 0.25f);

        private static readonly Color AcceptanceRequirementsBoxColor = new Color(0.62f, 0.18f, 0.18f);

        private static readonly Color acceptanceRequirementsBoxBgColor = new Color(0.13f, 0.13f, 0.13f);

        private static Texture2D RatingIcon = null;

        private static List<GenUI.AnonymousStackElement> tmpStackElements = new List<GenUI.AnonymousStackElement>();

        private static List<Rect> layoutRewardsRects = new List<Rect>();

        private static List<QuestPart> tmpRemainingQuestParts = new List<QuestPart>();

        private static List<GlobalTargetInfo> tmpLookTargets = new List<GlobalTargetInfo>();

        private static List<GlobalTargetInfo> tmpSelectTargets = new List<GlobalTargetInfo>();
        public override Vector2 InitialSize => new Vector2(1010f, 640f);
        public override void PreOpen()
        {
            base.PreOpen();
            if (RatingIcon == null)
            {
                RatingIcon = ContentFinder<Texture2D>.Get("UI/Icons/ChallengeRatingIcon");
            }
            Select(selected);
        }

        public Window_Contracts(QuestGiverManager questGiverManager)
        {
            this.questGiverManager = questGiverManager;
            this.closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect rect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect availableContractsRect = new Rect(rect.x + 10, rect.y, 400, 30);
            Widgets.Label(availableContractsRect, (questGiverManager.def.windowTitleKey ?? "VEF.AvailableContracts")
                .Translate(this.questGiverManager.AvailableQuests.Count));

            Rect rect2 = rect;
            rect2.yMin += 4f;
            rect2.xMax = rect2.width * 0.5f;
            rect2.yMin += 32f;
            rect2.yMax -= 45f;
            DoQuestsList(rect2);

            Rect rect3 = rect;
            rect3.yMin += 4f;
            rect3.xMin = rect2.xMax + 17f;
            rect3.yMin += 32f;
            rect3.yMax -= 45f;
            DoSelectedQuestInfo(rect3);
        }

        public void Select(QuestInfo questInfo)
        {
            if (questInfo != selected)
            {
                selected = questInfo;
                selectedQuestScrollPosition = default(Vector2);
                selectedQuestLastHeight = 300f;
            }
        }

        private void DoQuestsList(Rect rect)
        {
            Rect rect2 = rect;
            Widgets.DrawMenuSection(rect2);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            if (this.questGiverManager.AvailableQuests.Count != 0)
            {
                Rect rect3 = rect2;
                rect3 = rect3.ContractedBy(10f);
                rect3.xMax += 6f;
                Rect viewRect = new Rect(0f, 0f, rect3.width - 16f, (float)this.questGiverManager.AvailableQuests.Count * 32f);
                Vector2 vector = default(Vector2);

                Widgets.BeginScrollView(rect3, ref scrollPosition_available, viewRect);
                vector = scrollPosition_available;

                float num = 0f;
                for (int i = 0; i < this.questGiverManager.AvailableQuests.Count; i++)
                {
                    float num2 = vector.y - 32f;
                    float num3 = vector.y + rect3.height;
                    if (num > num2 && num < num3)
                    {
                        DoRow(new Rect(0f, num, viewRect.width - 4f, 32f), this.questGiverManager.AvailableQuests[i]);
                    }
                    num += 32f;
                }
                Widgets.EndScrollView();

                Rect rect4 = new Rect(rect.x, rect.yMax + 7, rect.xMax, 40);
                if (selected != null)
                {
                    if (!QuestUtility.CanAcceptQuest(selected.quest))
                    {
                        GUI.color = Color.grey;
                    }
                    if (Widgets.ButtonText(rect4, "AcceptQuest".Translate()))
                    {
                        if (selected.choice != null)
                        {
                            tmpRemainingQuestParts.Clear();
                            tmpRemainingQuestParts.AddRange(selected.quest.PartsListForReading);

                            for (int l = 0; l < selected.quest_Part_choice.choices.Count; l++)
                            {
                                for (int m = 0; m < selected.choice.questParts.Count; m++)
                                {
                                    QuestPart item = selected.choice.questParts[m];
                                    if (!selected.choice.questParts.Contains(item))
                                    {
                                        tmpRemainingQuestParts.Remove(item);
                                    }
                                }
                            }
                            bool requiresAccepter = false;
                            for (int n = 0; n < tmpRemainingQuestParts.Count; n++)
                            {
                                if (tmpRemainingQuestParts[n].RequiresAccepter)
                                {
                                    requiresAccepter = true;
                                    break;
                                }
                            }
                            tmpRemainingQuestParts.Clear();
                            AcceptQuestByInterface(delegate
                            {
                                selected.quest_Part_choice.Choose(selected.choice);
                            }, requiresAccepter);
                        }
                        else
                        {
                            AcceptQuestByInterface(null, selected.quest.RequiresAccepter);
                        }
                    }
                }
                TooltipHandler.TipRegionByKey(rect4, "AcceptQuestForTip");
                GUI.color = Color.white;
            }
            else
            {
                Widgets.NoneLabel(rect2.y + 17f, rect2.width);
            }
        }

        private void DoRow(Rect rect, QuestInfo questInfo)
        {
            Rect rect2 = rect;
            rect2.width -= 200f;

            Rect rect4 = rect;
            rect4.x = rect2.xMax - 4;
            rect4.xMax = rect.xMax;

            Rect selectedRect = rect;
            selectedRect.width += 14;
            if (selected == questInfo)
            {
                Widgets.DrawHighlightSelected(selectedRect);
            }
            Rect challengeRating = rect4;
            challengeRating.width = (4 * 15f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect rect5 = new Rect(rect2.x + 4f, rect2.y, rect2.width - 4f, rect2.height);
            Widgets.Label(rect5, questInfo.quest.name.Truncate(rect5.width));

            for (int i = 0; i < questInfo.quest.challengeRating; i++)
            {
                GUI.DrawTexture(new Rect(rect4.x + (float)(15 * (i + 1)), rect4.y + rect4.height / 2f - 7f, 15f, 15f), RatingIcon);
            }
            if (Mouse.IsOver(challengeRating))
            {
                TooltipHandler.TipRegion(challengeRating, "QuestChallengeRatingTip".Translate());
                Widgets.DrawHighlight(challengeRating);
            }
            var baseFactionIconPosX = rect4.x + 7f + (4 * 15f);
            if (questInfo.currencyInfo is null)
            {
                baseFactionIconPosX += 120;
            }
            var factionIconRect = new Rect(baseFactionIconPosX, rect.y + 2, rect.height - 4, rect.height - 4);
            DrawFactionIconWithTooltip(factionIconRect, questInfo.askerFaction);

            if (questInfo.currencyInfo != null)
            {
                var currencyCostRect = new Rect(factionIconRect.xMax + 10, rect.y, 200, rect.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(currencyCostRect, questInfo.currencyInfo.GetCurrencyInfo());
            }
            GenUI.ResetLabelAlign();
            if (Widgets.ButtonInvisible(rect))
            {
                Select(questInfo);
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
        }

        public void DrawFactionIconWithTooltip(Rect r, Faction faction)
        {
            GUI.color = faction.Color;
            GUI.DrawTexture(r, faction.def.FactionIcon);
            GUI.color = Color.white;
            if (Mouse.IsOver(r))
            {
                TipSignal tip = new TipSignal(() => faction.Name + "\n\n" + faction.def.description, faction.loadID ^ 0x738AC053);
                TooltipHandler.TipRegion(r, tip);
                Widgets.DrawHighlight(r);
            }
        }

        private void DoSelectedQuestInfo(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            if (selected == null)
            {
                Widgets.NoneLabelCenteredVertically(rect, "(" + "NoQuestSelected".Translate() + ")");
                return;
            }
            Rect rect2 = rect.ContractedBy(17f);
            Rect outRect = rect2;
            Rect innerRect = new Rect(0f, 0f, outRect.width, selectedQuestLastHeight);
            Rect rect3 = new Rect(0f, 0f, outRect.width - 16f, selectedQuestLastHeight);
            Rect rect4 = rect3;
            bool flag = rect3.height > rect2.height;
            if (flag)
            {
                rect3.width -= 4f;
                rect4.width -= 16f;
            }
            Widgets.BeginScrollView(outRect, ref selectedQuestScrollPosition, rect3);
            float curY = 0f;
            DoTitle(rect3, ref curY);

            if (selected != null)
            {
                DoDescription(rect3, ref curY);
                DoAcceptanceRequirementInfo(innerRect, flag, ref curY);
                DoRewards(rect3, ref curY);
                DoLookTargets(rect3, ref curY);
                DoSelectTargets(rect3, ref curY);
                float num = curY;
                DoDefHyperlinks(rect3, ref curY);
                float num2 = curY;
                curY = num;
                if (!selected.quest.root.hideInvolvedFactionsInfo)
                {
                    DoFactionInfo(rect4, ref curY);
                }
                if (num2 > curY)
                {
                    curY = num2;
                }
                selectedQuestLastHeight = curY;
            }
            Widgets.EndScrollView();
        }

        private void DoTitle(Rect innerRect, ref float curY)
        {
            Text.Font = GameFont.Medium;
            Rect rect = new Rect(innerRect.x, curY, innerRect.width, 100f);
            Widgets.Label(rect, selected.quest.name.Truncate(rect.width));
            Text.Font = GameFont.Small;
            curY += Text.LineHeight;
            curY += 17f;
        }

        private void DoAcceptanceRequirementInfo(Rect innerRect, bool scrollBarVisible, ref float curY)
        {
            if (selected.quest.EverAccepted)
            {
                return;
            }
            IEnumerable<string> enumerable = ListUnmetAcceptRequirements();
            int num = enumerable.Count();
            if (num != 0)
            {
                bool flag = num > 1;
                string text = "QuestAcceptanceRequirementsDescription".Translate() + (flag ? ": " : " ") + (flag ? ("\n" + enumerable.ToLineList("  - ", capitalizeItems: true)) : (enumerable.First() + "."));
                curY += 17f;
                float num2 = 0f;
                float x = innerRect.x + 8f;
                float num3 = innerRect.width - 16f;
                if (scrollBarVisible)
                {
                    num3 -= 31f;
                }
                Rect rect = new Rect(x, curY, num3, 10000f);
                num2 += Text.CalcHeight(text, rect.width);
                Rect rect2 = new Rect(x, curY, num3, num2).ExpandedBy(8f);
                Widgets.DrawBoxSolid(rect2, acceptanceRequirementsBoxBgColor);
                GUI.color = AcceptanceRequirementsColor;
                Widgets.Label(rect, text);
                GUI.color = AcceptanceRequirementsBoxColor;
                Widgets.DrawBox(rect2, 2);
                curY += num2;
                GUI.color = Color.white;
                new LookTargets(ListUnmetAcceptRequirementCulprits()).TryHighlight(arrow: true, colonistBar: true, circleOverlay: true);
            }
        }

        private IEnumerable<string> ListUnmetAcceptRequirements()
        {
            for (int i = 0; i < selected.quest.PartsListForReading.Count; i++)
            {
                QuestPart_RequirementsToAccept questPart_RequirementsToAccept = selected.quest.PartsListForReading[i] as QuestPart_RequirementsToAccept;
                if (questPart_RequirementsToAccept != null)
                {
                    AcceptanceReport acceptanceReport = questPart_RequirementsToAccept.CanAccept();
                    if (!acceptanceReport.Accepted)
                    {
                        yield return acceptanceReport.Reason;
                    }
                }
            }
        }

        private IEnumerable<GlobalTargetInfo> ListUnmetAcceptRequirementCulprits()
        {
            for (int i = 0; i < selected.quest.PartsListForReading.Count; i++)
            {
                QuestPart_RequirementsToAccept questPart_RequirementsToAccept = selected.quest.PartsListForReading[i] as QuestPart_RequirementsToAccept;
                if (questPart_RequirementsToAccept != null)
                {
                    foreach (GlobalTargetInfo culprit in questPart_RequirementsToAccept.Culprits)
                    {
                        yield return culprit;
                    }
                }
            }
        }

        private void DoDescription(Rect innerRect, ref float curY)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!selected.quest.description.RawText.NullOrEmpty())
            {
                string value = selected.quest.description.Resolve();
                stringBuilder.Append(value);
            }
            tmpQuestParts.Clear();
            tmpQuestParts.AddRange(selected.quest.PartsListForReading);
            tmpQuestParts.SortBy((QuestPart x) => (x is QuestPartActivable) ? ((QuestPartActivable)x).EnableTick : 0);
            for (int i = 0; i < tmpQuestParts.Count; i++)
            {
                QuestPartActivable questPartActivable = tmpQuestParts[i] as QuestPartActivable;
                if (questPartActivable != null && questPartActivable.State != QuestPartState.Enabled)
                {
                    continue;
                }
                string descriptionPart = tmpQuestParts[i].DescriptionPart;
                if (!descriptionPart.NullOrEmpty())
                {
                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine();
                    }
                    stringBuilder.Append(descriptionPart);
                }
            }
            tmpQuestParts.Clear();
            if (stringBuilder.Length != 0)
            {
                curY += 17f;
                Rect rect = new Rect(innerRect.x, curY, innerRect.width, 10000f);
                Widgets.Label(rect, stringBuilder.ToString());
                curY += Text.CalcHeight(stringBuilder.ToString(), rect.width);
            }
        }

        private void DoRewards(Rect innerRect, ref float curY)
        {
            bool flag = selected.quest.State == QuestState.NotYetAccepted;
            bool flag2 = true;
            if (Event.current.type == EventType.Layout)
            {
                layoutRewardsRects.Clear();
            }
            if (selected.choice != null)
            {
                tmpStackElements.Clear();
                float num = 0f;
                for (int k = 0; k < selected.choice.rewards.Count; k++)
                {
                    tmpStackElements.AddRange(selected.choice.rewards[k].StackElements);
                    num += selected.choice.rewards[k].TotalMarketValue;
                }
                if (tmpStackElements.Any())
                {
                    if (num > 0f)
                    {
                        TaggedString totalValueStr = "TotalValue".Translate(num.ToStringMoney("F0"));
                        tmpStackElements.Add(new GenUI.AnonymousStackElement
                        {
                            drawer = delegate (Rect r)
                            {
                                GUI.color = new Color(0.7f, 0.7f, 0.7f);
                                Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), totalValueStr);
                                GUI.color = Color.white;
                            },
                            width = Text.CalcSize(totalValueStr).x + 10f
                        });
                    }
                    if (flag2)
                    {
                        curY += 17f;
                        flag2 = false;
                    }
                    else
                    {
                        curY += 10f;
                    }
                    Rect rect = new Rect(innerRect.x, curY, innerRect.width, 10000f);
                    Rect rect2 = rect.ContractedBy(10f);
                    rect.height = GenUI.DrawElementStack(rect2, 24f, tmpStackElements, delegate (Rect r, GenUI.AnonymousStackElement obj)
                    {
                        obj.drawer(r);
                    }, (GenUI.AnonymousStackElement obj) => obj.width, 4f, 5f, allowOrderOptimization: false).height + 20f;
                    if (Event.current.type == EventType.Layout)
                    {
                        layoutRewardsRects.Add(rect);
                    }
                    curY += rect.height;
                }
            }
            else
            {
                QuestPart_Choice choice = null;
                List<QuestPart> partsListForReading = selected.quest.PartsListForReading;
                for (int i = 0; i < partsListForReading.Count; i++)
                {
                    choice = (partsListForReading[i] as QuestPart_Choice);
                    if (choice != null)
                    {
                        break;
                    }
                }

                if (choice == null)
                {
                    return;
                }
                for (int j = 0; j < choice.choices.Count; j++)
                {
                    tmpStackElements.Clear();
                    float num = 0f;
                    for (int k = 0; k < choice.choices[j].rewards.Count; k++)
                    {
                        tmpStackElements.AddRange(choice.choices[j].rewards[k].StackElements);
                        num += choice.choices[j].rewards[k].TotalMarketValue;
                    }
                    if (!tmpStackElements.Any())
                    {
                        continue;
                    }
                    if (num > 0f)
                    {
                        TaggedString totalValueStr = "TotalValue".Translate(num.ToStringMoney("F0"));
                        tmpStackElements.Add(new GenUI.AnonymousStackElement
                        {
                            drawer = delegate (Rect r)
                            {
                                GUI.color = new Color(0.7f, 0.7f, 0.7f);
                                Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), totalValueStr);
                                GUI.color = Color.white;
                            },
                            width = Text.CalcSize(totalValueStr).x + 10f
                        });
                    }
                    if (flag2)
                    {
                        curY += 17f;
                        flag2 = false;
                    }
                    else
                    {
                        curY += 10f;
                    }
                    Rect rect = new Rect(innerRect.x, curY, innerRect.width, 10000f);
                    Rect rect2 = rect.ContractedBy(10f);
                    if (flag)
                    {
                        rect2.xMin += 100f;
                    }
                    if (j < layoutRewardsRects.Count)
                    {
                        Widgets.DrawBoxSolid(layoutRewardsRects[j], new Color(0.13f, 0.13f, 0.13f));
                        GUI.color = new Color(1f, 1f, 1f, 0.3f);
                        Widgets.DrawHighlightIfMouseover(layoutRewardsRects[j]);
                        GUI.color = Color.white;
                    }
                    rect.height = GenUI.DrawElementStack(rect2, 24f, tmpStackElements, delegate (Rect r, GenUI.AnonymousStackElement obj)
                    {
                        obj.drawer(r);
                    }, (GenUI.AnonymousStackElement obj) => obj.width, 4f, 5f, allowOrderOptimization: false).height + 20f;
                    if (Event.current.type == EventType.Layout)
                    {
                        layoutRewardsRects.Add(rect);
                    }
                    if (flag)
                    {
                        if (!QuestUtility.CanAcceptQuest(selected.quest))
                        {
                            GUI.color = Color.grey;
                        }
                        Rect rect3 = new Rect(rect.x, rect.y, 100f, rect.height);
                        if (Widgets.ButtonText(rect3, "AcceptQuestFor".Translate() + ":"))
                        {
                            tmpRemainingQuestParts.Clear();
                            tmpRemainingQuestParts.AddRange(selected.quest.PartsListForReading);
                            for (int l = 0; l < choice.choices.Count; l++)
                            {
                                if (j == l)
                                {
                                    continue;
                                }
                                for (int m = 0; m < choice.choices[l].questParts.Count; m++)
                                {
                                    QuestPart item = choice.choices[l].questParts[m];
                                    if (!choice.choices[j].questParts.Contains(item))
                                    {
                                        tmpRemainingQuestParts.Remove(item);
                                    }
                                }
                            }
                            bool requiresAccepter = false;
                            for (int n = 0; n < tmpRemainingQuestParts.Count; n++)
                            {
                                if (tmpRemainingQuestParts[n].RequiresAccepter)
                                {
                                    requiresAccepter = true;
                                    break;
                                }
                            }
                            tmpRemainingQuestParts.Clear();
                            QuestPart_Choice.Choice localChoice = choice.choices[j];
                            AcceptQuestByInterface(delegate
                            {
                                choice.Choose(localChoice);
                            }, requiresAccepter);
                        }
                        TooltipHandler.TipRegionByKey(rect3, "AcceptQuestForTip");
                        GUI.color = Color.white;
                    }
                    curY += rect.height;
                    break;
                }

            }
            if (Event.current.type == EventType.Repaint)
            {
                layoutRewardsRects.Clear();
            }
            tmpStackElements.Clear();
        }

        private void DoLookTargets(Rect innerRect, ref float curY)
        {
            List<Map> maps = Find.Maps;
            int num = 0;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].IsPlayerHome)
                {
                    num++;
                }
            }
            tmpLookTargets.Clear();
            tmpLookTargets.AddRange(selected.quest.QuestLookTargets);
            tmpLookTargets.SortBy(delegate (GlobalTargetInfo x)
            {
                if (x.Thing is Pawn)
                {
                    return 0;
                }
                if (x.HasThing)
                {
                    return 1;
                }
                if (!x.IsWorldTarget)
                {
                    return 2;
                }
                return (!(x.WorldObject is Settlement) || ((Settlement)x.WorldObject).Faction != Faction.OfPlayer) ? 3 : 4;
            }, (GlobalTargetInfo x) => x.Label);
            bool flag = false;
            for (int j = 0; j < tmpLookTargets.Count; j++)
            {
                GlobalTargetInfo globalTargetInfo = tmpLookTargets[j];
                if (globalTargetInfo.HasWorldObject)
                {
                    MapParent mapParent = globalTargetInfo.WorldObject as MapParent;
                    if (mapParent != null && (!mapParent.HasMap || !mapParent.Map.IsPlayerHome))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            bool flag2 = false;
            for (int k = 0; k < tmpLookTargets.Count; k++)
            {
                GlobalTargetInfo globalTargetInfo2 = tmpLookTargets[k];
                if (CameraJumper.CanJump(globalTargetInfo2) && (num != 1 || !(globalTargetInfo2 == Find.AnyPlayerHomeMap.Parent) || flag))
                {
                    if (!flag2)
                    {
                        flag2 = true;
                        curY += 17f;
                    }
                    if (Widgets.ButtonText(new Rect(innerRect.x, curY, innerRect.width, 25f), "JumpToTargetCustom".Translate(globalTargetInfo2.Label), drawBackground: false))
                    {
                        CameraJumper.TryJumpAndSelect(globalTargetInfo2);
                        Find.MainTabsRoot.EscapeCurrentTab();
                    }
                    curY += 25f;
                }
            }
        }

        private void DoSelectTargets(Rect innerRect, ref float curY)
        {
            bool flag = false;
            for (int i = 0; i < selected.quest.PartsListForReading.Count; i++)
            {
                QuestPart questPart = selected.quest.PartsListForReading[i];
                tmpSelectTargets.Clear();
                tmpSelectTargets.AddRange(questPart.QuestSelectTargets);
                if (tmpSelectTargets.Count == 0)
                {
                    continue;
                }
                if (!flag)
                {
                    flag = true;
                    curY += 4f;
                }
                if (Widgets.ButtonText(new Rect(innerRect.x, curY, innerRect.width, 25f), questPart.QuestSelectTargetsLabel, drawBackground: false))
                {
                    Map map = null;
                    int num = 0;
                    Vector3 zero = Vector3.zero;
                    Find.Selector.ClearSelection();
                    for (int j = 0; j < tmpSelectTargets.Count; j++)
                    {
                        GlobalTargetInfo target = tmpSelectTargets[j];
                        if (CameraJumper.CanJump(target) && target.HasThing)
                        {
                            Find.Selector.Select(target.Thing);
                            if (map == null)
                            {
                                map = target.Map;
                            }
                            else if (target.Map != map)
                            {
                                num = 0;
                                break;
                            }
                            zero += target.Cell.ToVector3();
                            num++;
                        }
                    }
                    if (num > 0)
                    {
                        CameraJumper.TryJump(new IntVec3(zero / num), map);
                    }
                    Find.MainTabsRoot.EscapeCurrentTab();
                }
                curY += 25f;
            }
        }

        private void DoFactionInfo(Rect rect, ref float curY)
        {
            curY += 15f;
            foreach (Faction involvedFaction in selected.quest.InvolvedFactions)
            {
                if (involvedFaction != null && !involvedFaction.Hidden && !involvedFaction.IsPlayer)
                {
                    FactionUIUtility.DrawRelatedFactionInfo(rect, involvedFaction, ref curY);
                }
            }
        }

        private void DoDefHyperlinks(Rect rect, ref float curY)
        {
            curY += 25f;
            foreach (Dialog_InfoCard.Hyperlink hyperlink in selected.quest.Hyperlinks)
            {
                float num = Text.CalcHeight(hyperlink.Label, rect.width);
                Widgets.HyperlinkWithIcon(new Rect(rect.x, curY, rect.width / 2f, num), hyperlink, "ViewHyperlink".Translate(hyperlink.Label));
                curY += num;
            }
        }

        private void AcceptQuestByInterface(Action preAcceptAction = null, bool requiresAccepter = false)
        {
            if (QuestUtility.CanAcceptQuest(selected.quest))
            {
                if (requiresAccepter)
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
                    {
                        Pawn pLocal;
                        if (QuestUtility.CanPawnAcceptQuest(p, selected.quest))
                        {
                            pLocal = p;
                            string text = "AcceptWith".Translate(p);
                            if (p.royalty != null && p.royalty.AllTitlesInEffectForReading.Any())
                            {
                                text = text + " (" + p.royalty.MostSeniorTitle.def.GetLabelFor(pLocal) + ")";
                            }
                            list.Add(new FloatMenuOption(text, delegate
                            {
                                if (QuestUtility.CanPawnAcceptQuest(pLocal, selected.quest))
                                {
                                    QuestPart_GiveRoyalFavor questPart_GiveRoyalFavor = selected.quest.PartsListForReading.OfType<QuestPart_GiveRoyalFavor>().FirstOrDefault();
                                    if (questPart_GiveRoyalFavor != null && questPart_GiveRoyalFavor.giveToAccepter)
                                    {
                                        IEnumerable<Trait> conceitedTraits = RoyalTitleUtility.GetConceitedTraits(p);
                                        IEnumerable<Trait> traitsAffectingPsylinkNegatively = RoyalTitleUtility.GetTraitsAffectingPsylinkNegatively(p);
                                        bool totallyDisabled = p.skills.GetSkill(SkillDefOf.Social).TotallyDisabled;
                                        bool flag = conceitedTraits.Any();
                                        bool flag2 = !p.HasPsylink && traitsAffectingPsylinkNegatively.Any();
                                        if (totallyDisabled || flag || flag2)
                                        {
                                            NamedArgument arg = p.Named("PAWN");
                                            NamedArgument arg2 = questPart_GiveRoyalFavor.faction.Named("FACTION");
                                            TaggedString t2 = null;
                                            if (totallyDisabled)
                                            {
                                                t2 = "RoyalIncapableOfSocial".Translate(arg, arg2);
                                            }
                                            TaggedString t3 = null;
                                            if (flag)
                                            {
                                                t3 = "RoyalWithConceitedTrait".Translate(arg, arg2, conceitedTraits.Select((Trait t) => t.Label).ToCommaList(useAnd: true));
                                            }
                                            TaggedString t4 = null;
                                            if (flag2)
                                            {
                                                t4 = "RoyalWithTraitAffectingPsylinkNegatively".Translate(arg, arg2, traitsAffectingPsylinkNegatively.Select((Trait t) => t.Label).ToCommaList(useAnd: true));
                                            }
                                            TaggedString text2 = "QuestGivesRoyalFavor".Translate(arg, arg2);
                                            if (totallyDisabled)
                                            {
                                                text2 += "\n\n" + t2;
                                            }
                                            if (flag)
                                            {
                                                text2 += "\n\n" + t3;
                                            }
                                            if (flag2)
                                            {
                                                text2 += "\n\n" + t4;
                                            }
                                            text2 += "\n\n" + "WantToContinue".Translate();
                                            Find.WindowStack.Add(new Dialog_MessageBox(text2, "Confirm".Translate(), AcceptAction, "GoBack".Translate()));
                                        }
                                        else
                                        {
                                            AcceptAction();
                                        }
                                    }
                                    else
                                    {
                                        AcceptAction();
                                    }
                                }
                            }));
                        }
                        void AcceptAction()
                        {
                            SoundDefOf.Quest_Accepted.PlayOneShotOnCamera();
                            if (preAcceptAction != null)
                            {
                                preAcceptAction();
                            }
                            Messages.Message("MessageQuestAccepted".Translate(pLocal, selected.quest.name), pLocal, MessageTypeDefOf.TaskCompletion, historical: false);
                            questGiverManager.ActivateQuest(pLocal, selected);
                            selected = null;
                        }
                    }
                    if (list.Count > 0)
                    {
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                    else
                    {
                        Messages.Message("MessageNoColonistCanAcceptQuest".Translate(Faction.OfPlayer.def.pawnsPlural), MessageTypeDefOf.RejectInput, historical: false);
                    }
                    return;
                }
                SoundDefOf.Quest_Accepted.PlayOneShotOnCamera();
                if (preAcceptAction != null)
                {
                    preAcceptAction();
                }
                questGiverManager.ActivateQuest(null, selected);
                selected = null;
            }
            else
            {
                Messages.Message("MessageCannotAcceptQuest".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            }
        }
    }
}
