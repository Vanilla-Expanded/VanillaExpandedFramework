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

        [DebugAction("Pawns", "Log MVCF ManagedVerbs", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void LogManagedVerbs(Pawn p)
        {
            var man = p.Manager();
            if (man == null) return;
            DebugActionsUtility.DustPuffFrom(p);
            Log.Message("All ManagedVerbs for " + p.LabelCap);
            foreach (var verb in man.ManagedVerbs)
            {
                Log.Message($"  {verb.Verb} ({verb.Verb.loadID}, {verb.GetUniqueLoadID()}): {(verb.Enabled ? "Enabled" : "Disabled")}{(verb.AllComps.Any() ? " Comps:" : "")}");
                foreach (var comp in verb.AllComps) Log.Message($"    {comp}");
            }
        }
    }

    public struct DebugOptions
    {
        public bool VerbLogging;
        public bool ScoreLogging;
    }
}