using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{

    public class FactionDefExtension : DefModExtension
    {

        private static readonly FactionDefExtension DefaultValues = new FactionDefExtension();
        public static FactionDefExtension Get(Def def) => def.GetModExtension<FactionDefExtension>() ?? DefaultValues;

        public override IEnumerable<string> ConfigErrors()
        {
            // The closest we have to ResolveReferences :/
            siegeMealDef = ThingDefOf.MealSurvivalPack;

            yield break;
        }

        public string settlementGenerationSymbol;

        public string packAnimalTexNameSuffix;

        public bool customSieges;
        public List<string> artilleryBuildingTags = new List<string>() { "Artillery_BaseDestroyer" };
        public ThingDef siegeMealDef;
    }

}
