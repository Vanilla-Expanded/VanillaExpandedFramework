using System;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace VanillaGenesExpanded
{
    [StaticConstructorOnStartup]
    public static class GraphicsCache
    {

        static Type cachedTextureType = AccessTools.TypeByName("Verse.CachedTexture");
       
        public static readonly object GeneBackground_Xenogene = Activator.CreateInstance(cachedTextureType, "UI/Icons/Genes/GeneBackground_Xenogene");
        public static readonly object GeneBackground_Endogene = Activator.CreateInstance(cachedTextureType, "UI/Icons/Genes/GeneBackground_Endogene");
        public static readonly object GeneBackground_Archite = Activator.CreateInstance(cachedTextureType, "UI/Icons/Genes/GeneBackground_ArchiteGene");
    }
}
