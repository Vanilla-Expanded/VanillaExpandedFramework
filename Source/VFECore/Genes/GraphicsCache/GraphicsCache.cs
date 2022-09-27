using System;
using UnityEngine;
using Verse;

namespace VanillaGenesExpanded
{
    [StaticConstructorOnStartup]
    public static class GraphicsCache
    {

        public static readonly CachedTexture GeneBackground_Xenogene = new CachedTexture("UI/Icons/Genes/GeneBackground_Xenogene");
        public static readonly CachedTexture GeneBackground_Endogene = new CachedTexture("UI/Icons/Genes/GeneBackground_Endogene");

    }
}
