using System;

namespace VEF.CacheClearing;

[AttributeUsage(AttributeTargets.Field)]
public class NoCacheClearingAttribute : Attribute;