using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public static class NonPublicProperties
    {

        public static Func<Projectile, float> Projectile_get_StartingTicksToImpact = (Func<Projectile, float>)
            Delegate.CreateDelegate(typeof(Func<Projectile, float>), null, AccessTools.Property(typeof(Projectile), "StartingTicksToImpact").GetGetMethod(true));
    }
}