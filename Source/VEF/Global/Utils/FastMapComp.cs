using Verse;

namespace VEF.Utils;

public class FastMapComp<T> where T : MapComponent
{
    private static Map cachedMap;
    private static T cachedComp;

    public static T Get(Map map)
    {
        if (map == null)
            return null;
        if (map == cachedMap)
            return cachedComp;
        return cachedComp = (cachedMap = map).GetComponent<T>();
    }
}