using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VEF.AnimalGenes
{
    public class HediffComp_GameteGenes : HediffComp
    {

        public List<AnimalGeneDef> genes = new List<AnimalGeneDef>();

        protected HediffCompProperties_GameteGenes Props => (HediffCompProperties_GameteGenes)props;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Collections.Look(ref genes, "storedGameteAnimalGenes", LookMode.Def);
        }


    }
}