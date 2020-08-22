using System;
using RimWorld;
using Verse;

namespace VanillaStorytellersExpanded
{
	// Token: 0x02000022 RID: 34
	public struct TechLevelRange : IEquatable<TechLevelRange>
	{
		// Token: 0x06000044 RID: 68 RVA: 0x000021B4 File Offset: 0x000003B4
		public TechLevelRange(TechLevel min, TechLevel max)
		{
			this.min = min;
			this.max = max;
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000045 RID: 69 RVA: 0x00003034 File Offset: 0x00001234
		public static TechLevelRange All
		{
			get
			{
				return new TechLevelRange(TechLevel.Animal, TechLevel.Archotech);
			}
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00003050 File Offset: 0x00001250
		public bool Includes(TechLevel level)
		{
			return level >= this.min && level <= this.max;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x0000307C File Offset: 0x0000127C
		public static bool operator ==(TechLevelRange a, TechLevelRange b)
		{
			return a.min == b.min && a.max == b.max;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x000030B0 File Offset: 0x000012B0
		public static bool operator !=(TechLevelRange a, TechLevelRange b)
		{
			return !(a == b);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x000030CC File Offset: 0x000012CC
		public override string ToString()
		{
			return string.Format("{0}~{1}", this.min, this.max);
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00003100 File Offset: 0x00001300
		public static TechLevelRange FromString(string s)
		{
			string[] array = s.Split(new char[]
			{
				'~'
			});
			return new TechLevelRange(ParseHelper.FromString<TechLevel>(array[0]), ParseHelper.FromString<TechLevel>(array[1]));
		}

		// Token: 0x0600004B RID: 75 RVA: 0x0000313C File Offset: 0x0000133C
		public override bool Equals(object obj)
		{
			bool result;
			if (obj is TechLevelRange)
			{
				TechLevelRange b = (TechLevelRange)obj;
				result = (this == b);
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600004C RID: 76 RVA: 0x0000316C File Offset: 0x0000136C
		public bool Equals(TechLevelRange other)
		{
			return this == other;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x0000318C File Offset: 0x0000138C
		public override int GetHashCode()
		{
			return Gen.HashCombineStruct<TechLevel>(this.min.GetHashCode(), this.max);
		}

		// Token: 0x0400004A RID: 74
		public TechLevel min;

		// Token: 0x0400004B RID: 75
		public TechLevel max;
	}
}
