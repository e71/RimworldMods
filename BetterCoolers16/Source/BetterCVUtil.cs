using System.Linq;
using Verse;

namespace BetterCV;

public static class BetterCVUtil
{
	public static bool isWall(BuildableDef bDef)
	{
		if (!(bDef is ThingDef thingDef))
		{
			return false;
		}
		if (thingDef.graphicData != null)
		{
			if ((thingDef.graphicData.linkFlags & LinkFlags.Wall) == 0)
			{
				return thingDef.IsSmoothed;
			}
			return true;
		}
		return false;
	}

	public static bool isOverWall(BuildableDef bDef)
	{
		if (!(bDef is ThingDef thingDef) || thingDef.building == null || bDef.PlaceWorkers == null)
		{
			return false;
		}
		return thingDef.building.canPlaceOverWall && bDef.PlaceWorkers.Any(p => p is PlaceWorker_OnWall);
	}
}
