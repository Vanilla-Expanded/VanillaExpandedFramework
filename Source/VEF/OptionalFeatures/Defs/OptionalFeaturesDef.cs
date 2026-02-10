using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace VEF.OptionalFeatures
{
    public class OptionalFeaturesDef : Def
    {
        public string feature;
        public Type activationClass;
        [Unsaved] public MethodInfo activationMethod;
        public string harmonyCategory;
        [Unsaved] private bool isActive = false;

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            // Search for a static method with a single "Harmony" argument only.
            if (activationClass != null)
                activationMethod = activationClass.GetMethod("ApplyFeature", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(Harmony)], null);
        }

        public void Activate()
        {
            if (isActive)
                return;
            isActive = true;

            if (activationClass != null && harmonyCategory != null)
                Log.WarningOnce($"Feature {feature} has both {nameof(activationClass)} and {nameof(harmonyCategory)} specified, only {nameof(harmonyCategory)} will be used. Category: {harmonyCategory}, type: {activationClass}.", feature.GetHashCode());

            if (!harmonyCategory.NullOrEmpty())
                VEF_Mod.harmonyInstance.PatchCategory(harmonyCategory);
            else if (activationMethod == null)
                Log.ErrorOnce($"Feature {feature} with type {activationClass.ToStringSafe()} does not have ApplyFeature method or does not specify a harmony category", feature.GetHashCode());
            else
                activationMethod.Invoke(null, [VEF_Mod.harmonyInstance]);
        }
    }
}
