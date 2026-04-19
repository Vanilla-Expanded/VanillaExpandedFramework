using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VEF.Storyteller
{
    public static class QuestUtils
    {
        public static void CreateQuest(this QuestScriptDef questDef)
        {
            var quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, StorytellerUtility.DefaultThreatPointsNow(Find.World));
            if (questDef.sendAvailableLetter)
            {
                QuestUtility.SendLetterQuestAvailable(quest);
            }
        }

        public static List<PawnKindDef> GeneratePawnKindList(Faction faction, float points, Site site)
        {
            var pawnGroupMakerParms = new PawnGroupMakerParms
            {
                groupKind = PawnGroupKindDefOf.Combat,
                tile = site.Tile,
                faction = faction,
                points = points,
                raidStrategy = RaidStrategyDefOf.ImmediateAttack
            };
            var minPoints = faction.def.MinPointsToGeneratePawnGroup(pawnGroupMakerParms.groupKind, pawnGroupMakerParms);
            points = minPoints < float.MaxValue ? Mathf.Max(points, minPoints) : points;
            pawnGroupMakerParms.points = points;
            var generatedPawns = new List<PawnKindDef>();
            while (generatedPawns.Any() is false && points < 99999)
            {
                points += 50f;
                pawnGroupMakerParms.points = points;
                generatedPawns = GeneratePawnKinds(pawnGroupMakerParms, false).ToList();
            }
            return generatedPawns;
        }

        public static IEnumerable<PawnKindDef> GeneratePawnKinds(PawnGroupMakerParms parms, bool warnOnZeroResults = true)
        {
            if (parms.groupKind == null || parms.faction == null || parms.faction.def.pawnGroupMakers.NullOrEmpty()) yield break;
            if (!PawnGroupMakerUtility.TryGetRandomPawnGroupMaker(parms, out var pawnGroupMaker)) yield break;
            foreach (var item in pawnGroupMaker.GeneratePawnKindsExample(parms)) yield return item;
        }

        public static string FormatPawnListToString(List<PawnKindDef> pawns)
        {
            if (pawns == null || !pawns.Any()) return "";
            return pawns.GroupBy(p => p).Select(group => $"{group.Count()} {group.Key.label}").ToCommaList();
        }

        public static T GetAssociatedPart<T>(this MapParent parent) where T : QuestPart_Site
        {
            foreach (var quest in Find.QuestManager.QuestsListForReading.Where(x => x.State == QuestState.Ongoing))
            {
                foreach (var questPart in quest.PartsListForReading.OfType<T>())
                    if (questPart.mapParent == parent || (parent is PocketMapParent pocket && pocket.sourceMap.Parent == questPart.mapParent)) return questPart;
            }
            return null;
        }
    }
}
