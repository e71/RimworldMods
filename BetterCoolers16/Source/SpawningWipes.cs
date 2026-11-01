using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterCV;

[HarmonyPatch(typeof(GenSpawn), "SpawningWipes")]
public static class SpawningWipes
{
	[HarmonyPostfix]
	public static void _Postfix(BuildableDef newEntDef, BuildableDef oldEntDef, ref bool __result)
	{
		if (__result)
		{
			BuildableDef buildableDef = GenConstruct.BuiltDefOf(newEntDef as ThingDef);
			BuildableDef buildableDef2 = GenConstruct.BuiltDefOf(oldEntDef as ThingDef);
			if (buildableDef != null && buildableDef2 != null && ((BetterCVUtil.isOverWall(buildableDef) && BetterCVUtil.isWall(buildableDef2)) || (BetterCVUtil.isOverWall(buildableDef2) && BetterCVUtil.isWall(buildableDef))))
			{
				__result = false;
			}
		}
	}
}
