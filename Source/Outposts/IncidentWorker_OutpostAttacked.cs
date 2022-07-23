using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;

namespace Outposts
{
    public class IncidentWorker_OutpostAttacked : IncidentWorker_RaidEnemy
    {
        protected override bool CanFireNowSub(IncidentParms parms) =>
            Find.WorldObjects.AllWorldObjects.Any(wo => wo is Outpost {Faction: {IsPlayer: true}}) && OutpostsMod.Settings.DoRaids;

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!Find.WorldObjects.AllWorldObjects.OfType<Outpost>().TryRandomElement(out var target)) return false;
            LongEventHandler.QueueLongEvent(() =>
            {
                parms.target = GetOrGenerateMapUtility.GetOrGenerateMap(target.Tile, new IntVec3(150, 1, 150), target.def);               
                parms.points = target.ResolveRaidPoints(parms);
                TryGenerateRaidInfo(parms, out var pawns);
                target.raidFaction = parms.faction;
                target.raidPoints = parms.points;
                TaggedString baseLetterLabel = GetLetterLabel(parms);
                TaggedString baseLetterText = GetLetterText(parms, pawns);
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref baseLetterLabel, ref baseLetterText, GetRelatedPawnsInfoLetterText(parms), true);
                SendStandardLetter(baseLetterLabel, baseLetterText, GetLetterDef(), parms, SplitIntoGroups(parms, pawns), Array.Empty<NamedArgument>());
                if (parms.controllerPawn == null || parms.controllerPawn.Faction != Faction.OfPlayer) parms.raidStrategy.Worker.MakeLords(parms, pawns);
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
            }, "GeneratingMapForNewEncounter", false, null);
            return true;
        }

        private List<TargetInfo> SplitIntoGroups(IncidentParms parms, List<Pawn> pawns)
        {
            var result = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                var groups = IncidentParmsUtility.SplitIntoGroups(pawns, parms.pawnGroups);
                var biggest = groups.MaxBy(x => x.Count);
                if (biggest.Any()) result.Add(biggest[0]);

                result.AddRange(groups.Where(group => group != biggest && group.Any()).Select(group => (TargetInfo) group[0]));
            }
            else if (pawns.Any()) result.AddRange(pawns.Select(t => (TargetInfo) t));

            return result;
        }   

    }
}