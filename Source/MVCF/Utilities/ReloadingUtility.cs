using System.Collections.Generic;
using System.Linq;
using Reloading;
using Verse;

namespace MVCF.Utilities
{
    public static class ReloadingUtility
    {
        private static readonly Dictionary<Verb, IReloadable> reloadables = new();

        public static IEnumerable<IReloadable> AllReloadComps(this Pawn p)
        {
            if (p?.equipment != null)
                foreach (var comp in p.equipment.AllEquipmentListForReading.SelectMany(eq => eq.AllComps))
                    if (comp is IReloadable reloadable)
                        yield return reloadable;
            if (p?.apparel != null)
                foreach (var comp in p.apparel.WornApparel.SelectMany(app => app.AllComps))
                    if (comp is IReloadable reloadable)
                        yield return reloadable;

            if (p?.health?.hediffSet != null)
                foreach (var comp in p.health.hediffSet.hediffs.OfType<HediffWithComps>()
                    .SelectMany(hediff => hediff.comps))
                    if (comp is IReloadable reloadable)
                        yield return reloadable;
        }

        public static IReloadable GetReloadableComp(this Thing thing)
        {
            switch (thing)
            {
                case Pawn p:
                    return p.health?.hediffSet?.hediffs?.OfType<HediffWithComps>()?.SelectMany(hediff => hediff.comps)
                        .OfType<IReloadable>()?.FirstOrDefault();
                case ThingWithComps twc:
                    return twc.AllComps.OfType<IReloadable>().FirstOrDefault();
                default:
                    return thing.TryGetComp<CompReloadable>();
            }
        }

        public static IReloadable GetReloadable(this Verb verb)
        {
            if (reloadables.ContainsKey(verb)) return reloadables[verb];

            IReloadable rv;

            if (verb.EquipmentSource != null &&
                verb.EquipmentSource.AllComps.FirstOrFallback(comp => comp is IReloadable) is IReloadable r1)
                rv = r1;

            else if (verb.HediffCompSource?.parent != null &&
                     verb.HediffCompSource.parent.comps.FirstOrFallback(comp => comp is IReloadable) is IReloadable r2
            )
                rv = r2;
            else rv = null;
            reloadables.Add(verb, rv);
            return rv;
        }
    }
}