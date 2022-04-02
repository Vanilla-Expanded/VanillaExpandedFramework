using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public class CompProperties_SpillWhenDamaged : CompProperties
    {
        public CompProperties_SpillWhenDamaged()
        {
            compClass = typeof(CompSpillWhenDamaged);
        }

        public int spillEachTicks = 250;
        public float startAtHitPointsPercent = 0f;
        public float amountToDraw = 0f;
        public List<ThingDef> chooseFilthFrom = new List<ThingDef>();
        public int filthMaxSpawnRadius = 5;
        public int filthAmountPerSpawn = 1;
        public List<FleckDef> chooseFleckFrom = new List<FleckDef>();

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;

            if (parentDef.GetCompProperties<CompProperties_Resource>() == null)
                yield return "CompProperties_SpillWhenDamaged cannot be used on a thing without CompProperties_Resource";
        }
    }
}
