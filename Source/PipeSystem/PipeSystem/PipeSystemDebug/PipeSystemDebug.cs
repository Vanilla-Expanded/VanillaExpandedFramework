using Verse;
using LudeonTK;

namespace PipeSystem
{
    /// <summary>
    /// Debug option for pipes systems
    /// </summary>
    internal class PipeSystemDebug
    {
        public static bool drawResourcesNetGrid = false;

        public static bool debugLogging = false;

        [DebugAction("Pipe System", "Toggle resources grids drawing", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ToggleGridView()
        {
            drawResourcesNetGrid = !drawResourcesNetGrid;
        }

        [DebugAction("Pipe System", "Toggle verbose loging", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ToggleDebugLog()
        {
            debugLogging = !debugLogging;
        }

        public static void Message(string msg)
        {
            if (debugLogging)
                Log.Message($"[PipeSystem] {msg}");
        }
    }
}