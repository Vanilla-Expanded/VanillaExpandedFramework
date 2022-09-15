using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KCSG
{
    public class FallingStructure : DefModExtension
    {
        public List<LayoutCommonality> structures = new List<LayoutCommonality>();
        public List<ThingDef> thingsToSpawnInDropPod = new List<ThingDef>();
        public List<FactionDef> canBeUsedBy = new List<FactionDef>();
        public bool spawnDormantWhenPossible = true;
        public bool needToHaveSettlements = true;
        public Type skyfaller = typeof(KCSG_Skyfaller);

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var err in base.ConfigErrors())
                yield return err;

            if (structures.NullOrEmpty())
                yield return $"FallingStructure defModExtension can't have an empty structures list";

            if (canBeUsedBy.NullOrEmpty())
                yield return $"FallingStructure defModExtension can't have an empty canBeUsedBy list";

        }
    }
}