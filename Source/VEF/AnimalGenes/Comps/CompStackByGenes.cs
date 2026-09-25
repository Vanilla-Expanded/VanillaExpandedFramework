using RimWorld;
using Verse;


namespace VEF.AnimalGenes
{
    public class CompStackByGenes : ThingComp
    {

        public override bool AllowStackWith(Thing other)
        {
            if (this.parent?.TryGetComp<CompAnimalGenes>() is not CompAnimalGenes compThisEgg) { return base.AllowStackWith(other); }
          
            if (other?.TryGetComp<CompAnimalGenes>() is not CompAnimalGenes compOtherEgg) { return base.AllowStackWith(other); }
           
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
