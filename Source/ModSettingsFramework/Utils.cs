using Verse;

namespace ModSettingsFramework
{
    public static class Utils
    {
        public static bool GetModOptionState(this ModContentPack modContentPack, string patchId)
        {
            var container = ModSettingsFrameworkSettings.GetModSettingsContainer(modContentPack);
            if (container != null && container.patchOperationStates.TryGetValue(patchId, out var state))
            {
                return state;
            }
            return false;
        }
    }
}
