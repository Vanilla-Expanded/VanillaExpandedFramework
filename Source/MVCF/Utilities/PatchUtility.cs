using System;
using System.Reflection;

namespace MVCF.Utilities;

public static class PatchUtility
{
    public static T CreateDelegate<T>(this MethodInfo info) where T : Delegate => (T)info.CreateDelegate(typeof(T));
}
