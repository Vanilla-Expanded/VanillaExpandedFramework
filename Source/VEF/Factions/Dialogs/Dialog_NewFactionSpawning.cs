using System;
using KCSG;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VEF.Factions
{
    public class Dialog_NewFactionSpawning : Window
    {
        private FactionDef factionDef;
        private IEnumerator<FactionDef> factionEnumerator;
        private ForcedFactionData forcedFactionData;
        private bool failedToSpawn = false;
        private static Color colorCoreMod = new Color(125 / 255f, 97 / 255f, 51 / 255f);
        private static Color colorMod = new Color(115 / 255f, 162 / 255f, 47 / 255f);
        private static Color colorRed = new Color(1f, 0.3f, 0.35f);

        public static void OpenDialog(IEnumerator<FactionDef> enumerator, FactionDef faction = null)
        {
            Find.WindowStack.Add(new Dialog_NewFactionSpawning(enumerator, faction));
        }

        private Dialog_NewFactionSpawning(IEnumerator<FactionDef> enumerator, FactionDef faction = null)
        {
            doCloseButton = false;
            forcePause = true;
            absorbInputAroundWindow = true;
            factionEnumerator = enumerator;
            factionDef = faction ?? enumerator.Current;
            forcedFactionData = FactionDefExtension.Get(factionDef).forcedFactionData;

            // Never close on accept automatically - closing is done after a faction is sucesfully spawned.
            closeOnAccept = false;
            // If we're forced to add a faction then disallow the player
            // from closing the dialog at all until the faction was spawned.
            if (forcedFactionData.forcePlayerToAddFactionIfMissing)
                closeOnCancel = closeOnClickedOutside = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect.AtZero());

            // Icon
            if (factionDef.FactionIcon)
            {
                var rectIcon = listing_Standard.GetRect(64);
                var center = rectIcon.center.x;
                rectIcon.xMin = center - 32;
                rectIcon.xMax = center + 32;
                GUI.DrawTexture(rectIcon, factionDef.FactionIcon);
            }

            // Title
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            listing_Standard.Label("VanillaFactionsExpanded.FactionTitle".Translate(new NamedArgument(factionDef.LabelCap, "FactionName")));
            listing_Standard.GapLine();

            // Description
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            var modName = GetModName();
            listing_Standard.Label("VanillaFactionsExpanded.ModInfo".Translate(new NamedArgument(modName, "ModName")));
            if (factionDef.hidden)
            {
                GUI.color = colorCoreMod;
                listing_Standard.Label("VanillaFactionsExpanded.HiddenFactionInfo".Translate());

                if (forcedFactionData.forcePlayerToAddFactionIfMissing)
                {
                    GUI.color = colorRed;
                    listing_Standard.Label(forcedFactionData.GetFactionDiscoveryMessage(factionDef));
                }
                else if (factionDef.requiredCountAtGameStart > 0)
                {
                    listing_Standard.Label("VanillaFactionsExpanded.RequiredFactionInfo".Translate(new NamedArgument(modName, "ModName")));
                }
            }
            else if (forcedFactionData.forcePlayerToAddFactionIfMissing)
            {
                GUI.color = colorRed;
                listing_Standard.Label(forcedFactionData.GetFactionDiscoveryMessage(factionDef));
            }
            if (factionDef.requiredCountAtGameStart <= 0)
            {
                GUI.color = colorRed;
                listing_Standard.Label("VanillaFactionsExpanded.NonSpawningFactionInfo".Translate());
            }
            if (forcedFactionData.forcePlayerToAddFactionIfMissing && failedToSpawn)
            {
                GUI.color = Color.yellow;
                Log.ErrorOnce("Displaying fail message:\n" + forcedFactionData.GetFactionDiscoveryFailedMessage(factionDef), 19278381);
                listing_Standard.Label(forcedFactionData.GetFactionDiscoveryFailedMessage(factionDef));
            }
            GUI.color = Color.white;

            listing_Standard.Gap(40);
            listing_Standard.Label("VanillaFactionsExpanded.FactionSelectOption".Translate());
            listing_Standard.Gap(60);

            // Options
            if (factionDef.hidden || factionDef.GetModExtension<CustomGenOption>()?.canSpawnSettlements == false)
            {
                if (listing_Standard.ButtonText("VanillaFactionsExpanded.FactionButtonAdd".Translate())) SpawnWithoutBases();
            }
            else
            {
                if (listing_Standard.ButtonText("VanillaFactionsExpanded.FactionButtonAddFull".Translate())) SpawnWithBases();
            }

            var forced = forcedFactionData.forcePlayerToAddFactionIfMissing && !failedToSpawn;
            if (forced) GUI.color = Color.gray;
            if (listing_Standard.ButtonText("VanillaFactionsExpanded.FactionButtonSkip".Translate()))
            {
                if (forced)
                    Messages.Message(forcedFactionData.GetFactionDiscoveryMessage(factionDef), MessageTypeDefOf.RejectInput, false);
                else
                    Skip();
            }
            if (!forced) GUI.color = colorRed;
            if (listing_Standard.ButtonText("VanillaFactionsExpanded.FactionButtonIgnore".Translate()))
            {
                if (forced)
                    Messages.Message(forcedFactionData.GetFactionDiscoveryMessage(factionDef), MessageTypeDefOf.RejectInput, false);
                else
                    Ignore();
            }
            
            GUI.color = Color.white;

            listing_Standard.End();
        }

        private void SpawnWithBases()
        {
            Dialog_NewFactionSpawningSettlements.OpenDialog(SpawnCallback, forcedFactionData.factionDiscoveryFactionCountFactor, forcedFactionData.factionDiscoveryMinimumDistanceFromPlayer);

            void SpawnCallback(int amount, int minDistance)
            {
                try
                {
                    var faction = NewFactionSpawningUtility.SpawnWithSettlements(factionDef, amount, minDistance, out var spawned);
                    if (faction == null || spawned == 0)
                    {
                        Messages.Message("VanillaFactionsExpanded.FactionMessageFailedFull".Translate(), MessageTypeDefOf.RejectInput, false);

                        failedToSpawn = true;
                        // If failed to spawn, allow closing the dialog normally by cancelling/clicking outside.
                        closeOnCancel = closeOnClickedOutside = true;
                    }
                    else
                    {
                        Messages.Message("VanillaFactionsExpanded.FactionMessageSuccessFull".Translate(new NamedArgument(faction.GetCallLabel(), "FactionName"), new NamedArgument(spawned, "Amount")), MessageTypeDefOf.TaskCompletion);
                        Close();
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"An error occurred when trying to spawn faction {factionDef?.defName}:\n{e.Message}\n{e.StackTrace}");
                    Messages.Message("VanillaFactionsExpanded.FactionMessageFailedFull".Translate(), MessageTypeDefOf.RejectInput, false);

                    failedToSpawn = true;
                    // If failed to spawn, allow closing the dialog normally by cancelling/clicking outside.
                    closeOnCancel = closeOnClickedOutside = true;
                }
            }
        }

        private void SpawnWithoutBases()
        {
            try
            {
                var faction = NewFactionSpawningUtility.SpawnWithoutSettlements(factionDef);
                Messages.Message("VanillaFactionsExpanded.FactionMessageSuccess".Translate(new NamedArgument(faction.GetCallLabel(), "FactionName")), MessageTypeDefOf.TaskCompletion);
                Close();
            }
            catch (Exception e)
            {
                Log.Error($"An error occurred when trying to spawn faction {factionDef?.defName}:\n{e.Message}\n{e.StackTrace}");
                Messages.Message("VanillaFactionsExpanded.FactionMessageFailed".Translate(), MessageTypeDefOf.RejectInput, false);

                failedToSpawn = true;
            }
        }

        private void Skip()
        {
            Close();
        }

        private void Ignore()
        {
            Find.World.GetComponent<NewFactionSpawningState>().Ignore(factionDef);
            Close();
        }

        public override void PostClose()
        {
            // If the faction is still under required count, repeat. Unless it failed to spawn.
            if (!failedToSpawn && forcedFactionData.forcePlayerToAddFactionIfMissing && forcedFactionData.UnderRequiredGameplayFactionCount(factionDef))
            {
                OpenDialog(factionEnumerator, factionDef);
            }
            // Otherwise continue as normal
            else if (factionEnumerator.MoveNext())
            {
                OpenDialog(factionEnumerator);
            }
            // If there's no more factions, dispose of enumerator
            else
            {
                factionEnumerator.Dispose();
            }
        }

        private string GetModName()
        {
            if (factionDef?.modContentPack == null) return "VanillaFactionsExpanded.AnUnknownMod".Translate();
            if (factionDef.modContentPack.IsCoreMod) return factionDef.modContentPack.Name.Colorize(colorCoreMod);
            return factionDef.modContentPack.Name.Colorize(colorMod);
        }

        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();

            if (factionDef.hidden || factionDef.GetModExtension<CustomGenOption>()?.canSpawnSettlements == false)
                SpawnWithoutBases();
            else
                SpawnWithBases();
        }
    }
}