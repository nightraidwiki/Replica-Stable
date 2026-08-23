using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DancingMad.P5;

public class Meteor : ISpecialAction
{
	private ulong _sourceId;

	public override string Name => "Meteor";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47952u, 47954u };

	public override void Update()
	{
		if (aoes.Count == 2)
		{
			IGameObject[] array = PlayerHelper.RaidByEnmity(_sourceId).Take(2).ToArray();
			for (int i = 0; i < 2; i++)
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
			DrawElement element = new DrawElement
			{
				drawAvfx = "general_1bzt",
				radiusX = 5f,
				radiusZ = 5f,
				refColor = GroundOmen.Red,
				refTargetColor = GroundOmen.Red,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 47954u }
				}
			};
			aoes.Add(DrawManager.Draw(element, Svc.Objects.LocalPlayer));
			aoes.Add(DrawManager.Draw(element, Svc.Objects.LocalPlayer));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 47954)
		{
			aoes.Clear();
		}
	}
}
