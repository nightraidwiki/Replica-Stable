using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Ui;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP;

public class CosmoArrow : ISpecialAction
{
	public override string Name => "Cosmo Arrow";

	public override uint WeatherID => 175u;

	public override uint Phase => 6u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31651u };

	public override bool HasConfig => true;

	public override void DrawConfig()
	{
		ImGui.SetNextItemWidth(300f);
		Vector4 col = Plugin.Config.TopP6CosmoArrowColor;
		if (ImGui.ColorEdit4("P6 Cosmo Arrow", ref col))
		{
			Plugin.Config.TopP6CosmoArrowColor = col;
			Plugin.Config.Save();
		}
		ImGui.SameLine();
		if (ImGui.Button("Preview color"))
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customRect2",
				drawOnObject = true,
				radiusX = 2.5f,
				radiusZ = 10f,
				destroyTime = 3000f,
				refColor = Plugin.Config.TopP6CosmoArrowColor,
				refTargetColor = Plugin.Config.TopP6CosmoArrowColor
			}, Svc.Objects.LocalPlayer);
		}
		ImGui.SameLine();
		if (ImGuiUtil.IconButton(FontAwesomeIcon.Redo, "Reset###TopP6CosmoArrowColor"))
		{
			Plugin.Config.TopP6CosmoArrowColor = new Vector4(1f, 1f, 0f, 1f);
			Plugin.Config.Save();
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject? gameObject = info.SourceId.GameObject();
		Angle facing = info.Facing;
		Vector3 position = gameObject.Position;
		float num = (float)Math.Cos(facing.Rad);
		float sinFacing = (float)Math.Sin(facing.Rad);
		float cosFacing = num;
		for (int i = 0; i < 6; i++)
		{
			float num2 = 2.5f + (float)(5 * (i + 1));
			int delayTime = (int)(info.CastTime * 1000f) + 2000 * i;
			CreateDirectionalOmen(position, cosFacing, sinFacing, num2, facing, delayTime);
			CreateDirectionalOmen(position, cosFacing, sinFacing, 0f - num2, facing, delayTime);
		}
	}

	private void CreateDirectionalOmen(Vector3 basePosition, float cosFacing, float sinFacing, float offset, Angle facing, int delayTime)
	{
		Vector3 position = new Vector3(basePosition.X + cosFacing * offset, basePosition.Y, basePosition.Z + sinFacing * offset);
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "customRect",
			Position = position,
			drawOnObject = false,
			refRotation = facing,
			radiusX = 2.5f,
			radiusZ = 100f,
			destroyTime = 2000f,
			delayDrawTime = delayTime,
			refColor = Plugin.Config.TopP6CosmoArrowColor,
			refTargetColor = Plugin.Config.TopP6CosmoArrowColor
		}, Svc.Objects.LocalPlayer);
	}
}
