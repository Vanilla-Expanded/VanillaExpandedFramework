using System.Collections.Generic;
using Verse;

namespace VFECore
{
    public class PregnancyApproachData : IExposable
    {
        public Dictionary<Pawn, PregnancyApproachDef> partners = new();
        public void ExposeData()
        {
            Scribe_Collections.Look(ref partners, "partners", LookMode.Reference, LookMode.Def, ref pawnKeys, ref defValues);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                partners ??= new Dictionary<Pawn, PregnancyApproachDef>();
            }
        }

        private List<Pawn> pawnKeys;
        private List<PregnancyApproachDef> defValues;
    }
}
