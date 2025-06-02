using System.Collections.Generic;
using Verse;
using RimWorld;

namespace VFECore
{
    public class QuestChainState : IExposable
    {
        private List<Pawn> deepSavedPawns = new List<Pawn>();
        private Dictionary<string, Pawn> uniquePawnsByTag = new Dictionary<string, Pawn>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref deepSavedPawns, "deepSavedPawns", LookMode.Deep);
            Scribe_Collections.Look(ref uniquePawnsByTag, "uniquePawnsByTag", 
                LookMode.Value, LookMode.Reference, ref tagKeys, ref pawnValues);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                deepSavedPawns ??= new List<Pawn>();
                uniquePawnsByTag ??= new Dictionary<string, Pawn>();
                deepSavedPawns.RemoveAll(p => p == null);
                uniquePawnsByTag.RemoveAll(pair => pair.Value == null);
            }
        }

        private List<string> tagKeys;
        private List<Pawn> pawnValues;


        public void StoreUniquePawn(string tag, Pawn pawn, bool deepSave)
        {
            if (!uniquePawnsByTag.ContainsKey(tag))
            {
                uniquePawnsByTag[tag] = pawn;
            }
            if (deepSave)
            {
                deepSavedPawns.Add(pawn);
            }
        }

        public void RemoveFromDeepSave(Pawn pawn)
        {
            deepSavedPawns.Remove(pawn);
        }

        public Pawn GetUniquePawn(string tag)
        {
            uniquePawnsByTag.TryGetValue(tag, out var pawn);
            return pawn;
        }
    }
}