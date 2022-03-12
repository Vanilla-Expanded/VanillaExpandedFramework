using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MVCF.Comps;
using MVCF.VerbComps;
using Verse;

namespace MVCF.Utilities
{
    public static class ManagedVerbUtility
    {
        private static readonly ConditionalWeakTable<Verb, ManagedVerb> managedVerbForVerbs = new();

        public static ManagedVerb CreateManaged(this AdditionalVerbProps props, bool hasComps)
        {
            var mv = props switch
            {
                {managedClass: { } type} => (ManagedVerb) Activator.CreateInstance(type),
                _ when hasComps => new VerbWithComps(),
                _ => new ManagedVerb()
            };

            return mv;
        }

        public static T TryGetComp<T>(this ManagedVerb verb) where T : VerbComp => verb.AllComps.OfType<T>().FirstOrDefault();

        public static void SaveManaged(this Verb verb)
        {
            if (managedVerbForVerbs.TryGetValue(verb, out var mv)) managedVerbForVerbs.Remove(verb);
            else mv = null;
            Scribe_Deep.Look(ref mv, "MVCF_ManagedVerb");
            managedVerbForVerbs.Add(verb, mv);
        }

        public static void Register(this ManagedVerb mv)
        {
            if (managedVerbForVerbs.TryGetValue(mv.Verb, out var man))
            {
                if (man == mv) return;
                managedVerbForVerbs.Remove(mv.Verb);
            }

            managedVerbForVerbs.Add(mv.Verb, mv);
        }

        public static ManagedVerb Managed(this Verb verb, bool warnOnFailed = true)
        {
            if (managedVerbForVerbs.TryGetValue(verb, out var mv))
                return mv;

            if (warnOnFailed)
                Log.ErrorOnce("[MVCF] Attempted to get ManagedVerb for verb " + verb.Label() +
                              " which does not have one. This may cause issues.", verb.GetHashCode());

            return null;
        }
    }
}