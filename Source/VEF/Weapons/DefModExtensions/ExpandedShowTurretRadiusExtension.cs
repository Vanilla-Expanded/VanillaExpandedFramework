using System;
using Verse;

namespace VEF.Weapons;

public class ExpandedShowTurretRadiusExtension : DefModExtension
{
    /// <summary>
    /// If specified, replaces the built-in check for supported verb classes with a check for this specific verb class.
    /// This checks only for this very specific verb class. However, if allowAnyVerb is also true, the check will also
    /// match any verbs based on that specific one (so for example, allowing for Verb_Shoot will also allow for Verb_ShootWithSmoke).
    /// This could come in handy for turrets that have multiple different verbs with different ranges, and we want to display a range for a specific verb only.
    /// </summary>
    public Type allowedVerbClass = null;
    /// <summary>
    /// If true, the place worker will support any verb class (as long as there's any verb present).
    /// However, if allowedVerbClass is specified, it instead changes how that specific check works.
    /// </summary>
    public bool allowAnyVerb = false;

    /// <summary>
    /// Determines if the maximum range should be drawn for this turret.
    /// </summary>
    public bool drawMaxRange = true;
    /// <summary>
    /// Determines if the minimum range should be drawn for this turret.
    /// </summary>
    public bool drawMinRange = true;
}