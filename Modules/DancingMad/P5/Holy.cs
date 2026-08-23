using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P5;

public class Holy : ISpecialAction
{
	private ulong _sourceId;

	public override string Name => "Holy";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47952u, 47956u };

	public override void Update()
	{
		if (aoes.Count == 6)
		{
			IGameObject[] array = PlayerHelper.RaidByEnmity(_sourceId).Skip(2).ToArray();
			for (int i = 0; i < 6; i++)
			{
				IGameObject gameObject = ((i < array.Length) ? array[i] : null);
				aoes[i].Enable = gameObject != null;
				aoes[i].Owner = gameObject;
			}
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 47952)
		{
			_sourceId = info.SourceId;
			for (int i = 0; i < 6; i++)
			{
				DrawElement element = new DrawElement
				{
					drawAvfx = "general_1bxf",
					radiusX = 5f,
					radiusZ = 5f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47956u }
					}
				};
				aoes.Add(DrawManager.Draw(element, Svc.Objects.LocalPlayer));
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 47956)
		{
			aoes.Clear();
		}
		if (info.ActionId != 47952)
		{
			return;
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 5f,
				radiusZ = 5f,
				distanceCheck = new DistanceCheck
				{
					CheckType = 2,
					CheckObject = _sourceId.GameObject()
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 47955u }
				}
			}, allPlayer);
		}
	}
}
