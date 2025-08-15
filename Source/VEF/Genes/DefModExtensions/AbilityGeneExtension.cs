using RimWorld;
using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace VEF.Genes
{
    //Basically a hack for VRE Lycanthropes. Not for general use. Will make an ability not be removed even when the gene is removed

    public class AbilityGeneExtension : DefModExtension
    {
        public bool dontModifyAbilityOnGeneRemoval = false;

       
    }
}