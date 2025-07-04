﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace VEF.OptionalFeatures
{
    public class OptionalFeatures : Mod
    {
        public static Dictionary<string, Type> Features = new Dictionary<string, Type>
        {
            
        };

        public static List<(string, Type, MethodInfo)> ActiveFeatures = new List<(string, Type, MethodInfo)>();

        public OptionalFeatures(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                foreach (OptionalFeaturesDef featureToAdd in DefDatabase<OptionalFeaturesDef>.AllDefsListForReading)
                {
                    if (featureToAdd.feature != null)
                    {
                        Features[featureToAdd.feature] = featureToAdd.activationClass;
                    }
                }

                foreach (var feature in DefDatabase<ModDef>.AllDefs.SelectMany(def => def.Activate.Where(feature => !ActiveFeatures.Any(tuple => tuple.Item1 == feature))))
                    if (Features.ContainsKey(feature))
                        ActiveFeatures.Add((feature, Features[feature], Features[feature].GetMethod("ApplyFeature", BindingFlags.Static | BindingFlags.Public)));
                    else Log.ErrorOnce($"Feature not found: {feature}", feature.GetHashCode());

                foreach (var (name, type, method) in ActiveFeatures)
                    if (method == null)
                        Log.ErrorOnce($"Feature {name} with type {type} does not have ApplyFeature method", type.GetHashCode());
                    else
                        method.Invoke(null, new object[] {VEF_Mod.harmonyInstance});
            });
        }
    }

    public class ModDef : Def
    {
        public List<string> Activate;
    }
}