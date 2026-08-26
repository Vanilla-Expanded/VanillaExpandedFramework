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
        public bool IsActive { get; private set; } = false;

        // Tracked separately from IsActive so a feature that threw is not retried by
        // the next mod requesting it, while IsActive still reports what was patched.
        private bool activationAttempted;

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            // Search for a static method with a single "Harmony" argument only.
            if (activationClass != null)
                activationMethod = activationClass.GetMethod("ApplyFeature", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(Harmony)], null);
        }

        public void Activate()
        {
            if (activationAttempted)
                return;
            activationAttempted = true;

            if (activationClass != null && harmonyCategory != null)
                Log.WarningOnce($"Feature {feature} has both {nameof(activationClass)} and {nameof(harmonyCategory)} specified, only {nameof(harmonyCategory)} will be used. Category: {harmonyCategory}, type: {activationClass}.", feature.GetHashCode());

            try
            {
                if (!harmonyCategory.NullOrEmpty())
                    VEF_Mod.harmonyInstance.PatchCategory(harmonyCategory);
                else if (activationMethod == null)
                {
                    Log.ErrorOnce($"Feature {feature} with type {activationClass.ToStringSafe()} does not have ApplyFeature method or does not specify a harmony category", feature.GetHashCode());
                    return;
                }
                else
                    activationMethod.Invoke(null, [VEF_Mod.harmonyInstance]);

                IsActive = true;
            }
            catch (Exception e)
            {
                // Unwrap the reflection wrapper so the log leads with the actual cause.
                if (e is TargetInvocationException { InnerException: not null } tie)
                    e = tie.InnerException;
                Log.Error($"[VEF] Failed activating optional feature '{feature}'. Other optional features are unaffected, but this one may be partially applied. Exception:\n{e}");
            }
        }
    }
}
