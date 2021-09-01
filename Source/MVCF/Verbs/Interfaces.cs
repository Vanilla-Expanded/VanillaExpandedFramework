using System.Collections.Generic;
using Verse;

namespace MVCF.Verbs
{
    public interface IVerbGizmos
    {
        IEnumerable<Gizmo> GetGizmos(out bool skipDefault);
    }

    public interface IVerbTick
    {
        void Tick();
    }
}