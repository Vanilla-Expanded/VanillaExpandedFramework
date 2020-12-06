using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaFurnitureExpanded
{

    public class ShowBlueprintExtension : DefModExtension
    {

        //A simple mod extension accesed by a Harmont patch to show a building's
        //blueprint when it is in ghost (placement) mode

        private static readonly ShowBlueprintExtension DefaultValues = new ShowBlueprintExtension();
        public static ShowBlueprintExtension Get(Def def) => def.GetModExtension<ShowBlueprintExtension>() ?? DefaultValues;

        
        public bool showBlueprintInGhostMode = true;

       

    }

}