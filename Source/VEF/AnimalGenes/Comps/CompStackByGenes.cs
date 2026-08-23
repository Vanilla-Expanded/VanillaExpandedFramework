using RimWorld;
using Verse;


namespace VEF.AnimalGenes
{
    public class CompStackByGenes : ThingComp
    {

        public override bool AllowStackWith(Thing other)
        {
            if (!WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(this.parent)) { return base.AllowStackWith(other); }
            CompAnimalGenes compThisEgg = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[this.parent];
            if (compThisEgg is null) { return base.AllowStackWith(other); }

            if (!WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes.ContainsKey(other)) { return base.AllowStackWith(other); }
            CompAnimalGenes compOtherEgg = WorldComponent_AnimalGenes.Instance.pawnToCompAnimalGenes[other];
            if (compOtherEgg is null) { return base.AllowStackWith(other); }

            bool same = true;

            if (compThisEgg.genes.Count != compOtherEgg.genes.Count)
            {
                same = false;
            }
            else
            {
                foreach (var item in compThisEgg.genes)
                {
                    if (!compOtherEgg.genes.Contains(item))
                    {
                        same = false;
                        break;
                    }
                }
            }
            return base.AllowStackWith(other) && same;
        }



    }
}
