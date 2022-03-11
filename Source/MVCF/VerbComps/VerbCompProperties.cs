using System;
using System.Collections.Generic;
using MVCF.Comps;
using Verse;

namespace MVCF.VerbComps
{
    public class VerbCompProperties
    {
        public Type compClass;

        public VerbCompProperties()
        {
        }

        public VerbCompProperties(Type type) => compClass = type;

        public virtual void ResolveReferences()
        {
        }

        public virtual IEnumerable<string> ConfigErrors(VerbProperties verbProps, AdditionalVerbProps additionalProps)
        {
            yield break;
        }

        public virtual void PostLoad(VerbProperties verbProps, AdditionalVerbProps additionalProps)
        {
            Base.EnabledFeatures.Add("VerbComps");
        }
    }
}