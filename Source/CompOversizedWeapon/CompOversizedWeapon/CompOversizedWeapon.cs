using System;
using HarmonyLib;
using Verse;

namespace CompOversizedWeapon
{
    public class CompOversizedWeapon : ThingComp
    {
        public CompProperties_OversizedWeapon Props => props as CompProperties_OversizedWeapon;

        private Func<bool> compDeflectorIsAnimatingNow = AlwaysFalse;

        private static bool AlwaysFalse() => false;

        private static readonly Type compDeflectorType = GenTypes.GetTypeInAnyAssembly("CompDeflector.CompDeflector");

        public bool CompDeflectorIsAnimatingNow => compDeflectorIsAnimatingNow();

        public bool IsOnGround => ParentHolder is Map;

        // Caching comps needs to happen after all comps are created. Ideally, this would be done right after
        // ThingWithComps.InitializeComps(). This requires overriding two hooks: PostPostMake and PostExposeData.

        public override void PostPostMake()
        {
            base.PostPostMake();
            CacheComps();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.LoadingVars)
                CacheComps();
        }

        private void CacheComps()
        {
            if (compDeflectorType != null)
            {
                // Avoiding ThingWithComps.GetComp<T> and implementing a specific non-generic version of it here.
                // That method is slow because the `isinst` instruction with generic type arg operands is very slow,
                // while `isinst` instruction against non-generic type operand like used below is fast.
                // For the optional CompDeflector, we have to use the slower IsAssignableFrom reflection check.
                var comps = parent.AllComps;
                for (int i = 0, count = comps.Count; i < count; i++)
                {
                    var comp = comps[i];
                    var compType = comp.GetType();
                    if (compDeflectorType.IsAssignableFrom(compType))
                    {
                        compDeflectorIsAnimatingNow =
                            (Func<bool>)AccessTools.PropertyGetter(compType, "IsAnimatingNow").CreateDelegate(typeof(Func<bool>), comp);
                        break;
                    }
                }
            }
        }
    }
}
