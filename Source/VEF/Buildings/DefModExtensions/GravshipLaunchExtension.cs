using Verse;

namespace VEF.Buildings;

public class GravshipLaunchExtension : DefModExtension
{
    /// <summary>
    /// Determines if this building should be considered an oxygen pusher.
    /// Having any such building will remove the warning about having no oxygen pump on the gravship while launching into orbit.
    /// If there's less than 1 oxygen pusher per 400 tiles of substructure, a warning about low amount of oxygen pumps will be displayed.
    /// Using this is not needed if you're using vanilla CompOxygenPusher/CompProperties_OxygenPusher.
    /// </summary>
    public bool isOxygenPusher = false;
    /// <summary>
    /// Determines if this building should be considered a heater.
    /// Having any such building will remove the warning about having no heaters on the gravship while launching into orbit.
    /// If there's less than 1 heater per 250 tiles of substructure, a warning about low amount of heaters will be displayed.
    /// Using this is not needed if you're using vanilla Building_Heater, or CompHeatPusher/CompProperties_HeatPusher with heatPerSecond of over 20.
    /// </summary>
    public bool isHeater = false;
}