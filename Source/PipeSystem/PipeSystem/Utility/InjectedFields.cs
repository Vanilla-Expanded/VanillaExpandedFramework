using RimWorld;
using Waylith.Injection;

namespace PipeSystem;

public static class InjectedFields
{
    extension(Building_Turret t)
    {
        [InjectedField]
        public CompTurretNeedsResource ResourceNeed
        {
            get => throw new System.InvalidOperationException("not injected");
            set => throw new System.InvalidOperationException("not injected");
        }
    }
}