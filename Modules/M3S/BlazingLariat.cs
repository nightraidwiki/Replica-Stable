using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M3S;

public class BlazingLariat : ISpecialAction
{
	public override string Name => "Blazing Lariat (stack / spread cone)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37848u, 37849u, 37850u, 37851u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 37848:
		case 37849:
			PlayerHelper.AllPlayers.ForEach(delegate(IGameObject player)
			{
				SimpleElement.FanToTarget(info.SourceId, player.EntityId, 40f, 45, Follow: true, default(Angle), 3000f, 37852u);
			});
			break;
		case 37850:
		case 37851:
		{
			List<IGameObject> tank = PlayerHelper.Tank;
			List<IGameObject> healer = PlayerHelper.Healer;
			int num = 0;
			IGameObject[] array = new IGameObject[tank.Count + healer.Count];
			Span<IGameObject> span = CollectionsMarshal.AsSpan(tank);
			span.CopyTo(new Span<IGameObject>(array).Slice(num, span.Length));
			num += span.Length;
			Span<IGameObject> span2 = CollectionsMarshal.AsSpan(healer);
			span2.CopyTo(new Span<IGameObject>(array).Slice(num, span2.Length));
			num += span2.Length;
			foreach (IGameObject target in array)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan020_0pt",
					radiusX = 40f,
					radiusZ = 40f,
					drawOnObject = true,
					target = target,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 37853u }
					}
				}, info.SourceId.GameObject());
			}
			break;
		}
		}
	}
}
