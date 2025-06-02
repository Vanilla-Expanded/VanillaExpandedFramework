using System.Collections.Generic;
using Verse;
using RimWorld;

namespace VFECore
{
    public class QuestChainState : IExposable
    {
        public Dictionary<string, Pawn> uniquePawnsByTag = new Dictionary<string, Pawn>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref uniquePawnsByTag, "uniquePawnsByTag", LookMode.Value, LookMode.Reference);
        }
        public Pawn GetUniquePawn(string tag)
        {
            uniquePawnsByTag.TryGetValue(tag, out var pawn);
            return pawn;
        }
    }
}