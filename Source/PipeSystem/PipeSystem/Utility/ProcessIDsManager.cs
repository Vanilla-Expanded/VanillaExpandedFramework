using Verse;

namespace PipeSystem
{
    public class ProcessIDsManager : IExposable
    {
        private int nextProcessID;

        private bool wasLoaded;

        public void ExposeData()
        {
            Scribe_Values.Look(ref nextProcessID, "nextProcessID", 0);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                wasLoaded = true;
            }
        }

        public int GetNextProcessID(Map map) => GetNextID(map, ref nextProcessID);

        private static int GetNextID(Map map, ref int nextID)
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars && !CachedAdvancedProcessorsManager.GetFor(map).ProcessIDsManager.wasLoaded)
            {
                Log.Warning("Getting next unique ID during LoadingVars before UniqueIDsManager was loaded. Assigning a random value.");
                return Rand.Int;
            }
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Log.Warning("Getting next unique ID during saving This may cause bugs.");
            }
            int result = nextID;
            nextID++;
            if (nextID == int.MaxValue)
            {
                Log.Warning("Next ID is at max value. Resetting to 0. This may cause bugs.");
                nextID = 0;
            }
            return result;
        }
    }
}