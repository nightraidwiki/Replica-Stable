using System;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Replica.Engine.Enum;

namespace Replica.Engine.Util;

public static class HeaderMarker
{
	public unsafe static HeaderMarkerEnum Mark(this ulong objectId)
	{
		HeaderMarkerEnum[] values = System.Enum.GetValues<HeaderMarkerEnum>();
		foreach (HeaderMarkerEnum headerMarkerEnum in values)
		{
			if (headerMarkerEnum != HeaderMarkerEnum.None)
			{
				GameObjectId gameObjectId = MarkingController.Instance()->Markers[(int)headerMarkerEnum];
				if (objectId == gameObjectId)
				{
					return headerMarkerEnum;
				}
			}
		}
		return HeaderMarkerEnum.None;
	}
}
