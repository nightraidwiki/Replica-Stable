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

namespace Replica.Modules.FRU;

public class ShiningArmor : ISpecialAction
{
	public override string Name => "Shining Armor (look away)";

	public override uint Phase => 2u;

	public override uint WeatherID => 35u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40209u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.NumCasts++;
		if (base.NumCasts == 1)
		{
			IGameObject castObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 17823);
			DrawManager.Draw(new DrawElement
			{
				drawType = ElementType.Channeling,
				drawAvfx = "chn_chainlightning_3t1",
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40185u }
				}
			}, Svc.Objects.LocalPlayer, castObject);
			DrawManager.Draw(new DrawElement
			{
				drawType = ElementType.Channeling,
				drawAvfx = "chn_miruna1v",
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40185u }
				}
			}, Svc.Objects.LocalPlayer, castObject);
		}
	}
}
