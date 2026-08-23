using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class CrystalOfTimeHourglass : ISpecialAction
{
	private enum SpeedType
	{
		Quicken,
		Normal,
		Slow
	}

	private readonly Dictionary<IGameObject, SpeedType> lightMap = new Dictionary<IGameObject, SpeedType>();

	public override string Name => "Crystal of Time (hourglass)";

	public override uint Phase => 4u;

	public override uint WeatherID => 106u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40240u, 40299u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		switch (info.ActionId)
		{
		case 40240u:
			Reset();
			break;
		case 40299u:
			base.NumCasts++;
			break;
		}
		switch (base.NumCasts)
		{
		case 2:
			DrawAoe(SpeedType.Normal, 5500f);
			break;
		case 4:
			DrawAoe(SpeedType.Slow, 5000f);
			break;
		}
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id != 133 && Id != 134)
		{
			return;
		}
		lightMap[actorId.GameObject()] = ((Id == 133) ? SpeedType.Slow : SpeedType.Quicken);
		if (lightMap.Count != 4)
		{
			return;
		}
		foreach (IGameObject item in Svc.Objects.Where((IGameObject o) => o.BaseId == 17837 && !lightMap.ContainsKey(o)))
		{
			lightMap[item] = SpeedType.Normal;
		}
		if (lightMap.Count == 6)
		{
			DrawAoe(SpeedType.Quicken, 7800f);
		}
	}

	private void DrawAoe(SpeedType type, float DrawTime, float Delay = 0f)
	{
		foreach (KeyValuePair<IGameObject, SpeedType> item in lightMap.Where((KeyValuePair<IGameObject, SpeedType> kv) => kv.Value == type))
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customCircle",
				radiusX = 12f,
				radiusZ = 12f,
				drawOnObject = true,
				destroyTime = DrawTime,
				delayDrawTime = Delay,
				refColor = new Vector4(1f, 1f, 1f, 0.3f),
				refTargetColor = new Vector4(1f, 0.8f, 0.2f, 2f)
			}, item.Key);
		}
	}

	public override void Reset()
	{
		lightMap.Clear();
		base.Reset();
	}
}
