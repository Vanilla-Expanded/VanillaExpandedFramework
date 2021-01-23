using MVCF.Utilities;
using RimWorld;
using Verse;

namespace MVCF
{
    public static class DebugActionsMVCF
    {
        [DebugAction("Pawns", "Toggle MVCF Verb Logging", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ToggleVerbLogging(Pawn p)
        {
            var man = p.Manager();
            man.debugOpts.VerbLogging = !man.debugOpts.VerbLogging;
            DebugActionsUtility.DustPuffFrom(p);
            MoteMaker.ThrowText(p.DrawPos, p.Map, p.LabelShort + "\n" + (man.debugOpts.VerbLogging ? "ON" : "OFF"));
        }

        [DebugAction("Pawns", "Toggle MVCF Verb Score Logging", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ToggleScoreLogging(Pawn p)
        {
            var man = p.Manager();
            man.debugOpts.ScoreLogging = !man.debugOpts.ScoreLogging;
            DebugActionsUtility.DustPuffFrom(p);
            MoteMaker.ThrowText(p.DrawPos, p.Map, p.LabelShort + "\n" + (man.debugOpts.ScoreLogging ? "ON" : "OFF"));
        }
    }

    public struct DebugOptions
    {
        public bool VerbLogging;
        public bool ScoreLogging;
    }
}