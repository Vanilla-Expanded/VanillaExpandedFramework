using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VEF.OptionalFeatures
{
    public class OptionalFeatures : Mod
    {
        public OptionalFeatures(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                var features = new Dictionary<string, OptionalFeaturesDef>();

                foreach (var featureToAdd in DefDatabase<OptionalFeaturesDef>.AllDefsListForReading)
                {
                    if (featureToAdd.feature != null)
                    {
                        features[featureToAdd.feature] = featureToAdd;
                    }
                }

                foreach (var feature in DefDatabase<ModDef>.AllDefs.SelectMany(def => def.Activate))
                    if (feature != null && features.TryGetValue(feature, out var f))
                        f.Activate();
                    else Log.ErrorOnce($"Feature not found: {feature}", (feature ?? "null").GetHashCode());
            });
        }
    }

    public class ModDef : Def
    {
        public List<string> Activate;
    }
}