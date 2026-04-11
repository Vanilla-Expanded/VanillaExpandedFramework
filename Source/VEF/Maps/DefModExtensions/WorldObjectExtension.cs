using Verse;

namespace VEF.Maps;

public class WorldObjectExtension : DefModExtension
{
    // Prevent any ObjectSpawnDef from triggering on this particular map.
    // This is meant as a blanket ban to all spawn, rather than blocking
    // this WorldObjectDef in every single ObjectSpawnDef that exists.
    public bool disableObjectSpawnDefs = false;
}