using System.Collections.Generic;
using RimWorld;
using Verse;
namespace VanillaMemesExpanded
{
    public class PlaceWorker_DisabledByMeme : PlaceWorker
    {

        public override bool IsBuildDesignatorVisible(BuildableDef def)
        {
            List<MemeDef> list = Current.Game?.World?.factionManager?.OfPlayer?.ideos?.PrimaryIdeo?.memes;
            if (list != null)
            {
                foreach (MemeDef meme in list)
                {
                    ExtendedMemeProperties extendedMemeProps = meme.GetModExtension<ExtendedMemeProperties>();
                    if (extendedMemeProps != null)
                    {
                        if (extendedMemeProps.removedDesignators != null)
                        {
                            foreach (ThingDef thing in extendedMemeProps.removedDesignators)
                            {
                                if (thing == def)
                                {
                                    return false;
                                }
                            }

                         }
                    }


                }

            }
            return true;
        }

    }
}
