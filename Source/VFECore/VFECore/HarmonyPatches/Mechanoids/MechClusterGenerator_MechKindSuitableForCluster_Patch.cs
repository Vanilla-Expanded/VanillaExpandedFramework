using HarmonyLib;
using RimWorld;
using Verse;

namespace VFE.Mechanoids
{
    [HarmonyPatch(typeof(MechClusterGenerator), nameof(MechClusterGenerator.MechKindSuitableForCluster))]
	public class MechClusterGenerator_MechKindSuitableForCluster_Patch
	{
		public static void Postfix(PawnKindDef __0, ref bool __result)
		{
			if (__result)
			{
				var extension = __0.race.GetModExtension<MechanoidExtension>();
				if (extension != null && extension.preventSpawnInAncientDangersAndClusters)
                {
					__result = false;
				}
			}
		}
	}
}
