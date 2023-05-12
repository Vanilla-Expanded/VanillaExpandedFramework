using Verse;

namespace VFECore.Abilities;

/// power (after stat offsets) becomes a random number within a range.
///
/// The formula used is Rand.Range(power * min, power * max)
public class AbilityExtension_RandomPowerMultiplier : DefModExtension
{
    public float min = 1f;  // Must be <= max
    public float max = 1f;  // Must be >= min
}
