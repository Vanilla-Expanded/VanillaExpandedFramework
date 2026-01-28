namespace VEF;

/// <summary>
/// An interface that can be applied to comp properties or def mod extensions.
/// When present, it causes the objects it is applied to will be merged into a single one, somewhat mimicing XML inheritance.
/// Only applied when both objects are the same exact type, so you should be careful with inheritance.
/// It may be a smart idea to make all those methods and properties virtual in case you include inheritance for your objects.
/// Supported by default: Def.modExtensions, ThingDef.comps, HediffDef.comps, WorldObjectDef.comps, StorytellerDef.comps,
/// AbilityDef.comps, RitualVisualEffectDef.comps, RitualOutcomeEffectDef.comps, SurgeryOutcomeEffectDef.comps
/// </summary>
public interface IMergeable
{
    /// <summary>
    /// Determines in which order the objects will be merged. Other objects will be merged into the one with the highest priority.
    /// </summary>
    float Priority { get; }

    /// <summary>
    /// Handles merging other objects into this one.
    /// </summary>
    /// <param name="other">The other object that will be removed and should be merged into this one.</param>
    void Merge(object other);

    /// <summary>
    /// Determines if the other object can be merged into this one.
    /// Although not required, it may be smart to include a type check as an extra precaution.
    /// </summary>
    /// <param name="other">The other object that could be merged into this one and then removed.</param>
    /// <returns>True if the other object should be merged and removed, false if it should be kept instead.</returns>
    bool CanMerge(object other);
}