using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.Util;

namespace Replica.Engine.Preview;

public static class UmadPreview
{
	private const float StepMs = 3500f;

	public static bool Run()
	{
		IGameObject localPlayer = Svc.Objects.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}
		Vector3 origin = localPlayer.Position;
		Angle facing = localPlayer.Rotation.Radians();
		int slot = 0;
		SimpleElement.Fan(origin, 100f, 120, facing, 3500f, Delay());
		slot++;
		SimpleElement.Rectangle(origin, 100f, 3f, 0f, facing, 3500f, Delay());
		slot++;
		SimpleElement.Fan(origin, 40f, 90, facing, 3500f, Delay());
		SimpleElement.Rectangle(origin, 40f, 5f, 0f, facing, 3500f, Delay());
		slot++;
		SimpleElement.Fan(origin, 100f, 180, facing, 3500f, Delay());
		slot++;
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.Channeling,
			drawAvfx = "chn_miro1v",
			destroyTime = 3500f,
			delayDrawTime = Delay()
		}, localPlayer, localPlayer);
		slot++;
		SimpleElement.Circle(origin, 5f, 3500f, Delay());
		slot++;
		Omen("nockback_omen04t1", 6f, 6f);
		slot++;
		Omen("e5d1_b1_kblaser_t1", 1f, 10f, fix: true);
		slot++;
		Omen("m0347_sircle_01m1", 6f, 6f);
		Vector3? pos = origin + Offset(facing, 8f, 120f);
		Omen("m0347_sircle_01m1", 6f, 6f, fix: false, null, pos);
		pos = origin + Offset(facing, 8f, 240f);
		Omen("m0347_sircle_01m1", 6f, 6f, fix: false, null, pos);
		slot++;
		SimpleElement.Fan(origin, 100f, 180, facing, 3500f, Delay());
		slot++;
		Omen("general02wf", 20f, 80f, fix: true);
		Omen("tank_lockon_5m_5s_noc", 7f, 7f, fix: false, GroundOmen.Red);
		slot++;
		SimpleElement.Circle(origin, 5f, 3500f, Delay());
		slot++;
		SimpleElement.Circle(origin, 5f, 3500f, Delay());
		Omen("gl_fan180_1bf", 100f, 100f, fix: true);
		slot++;
		Omen("gl_fan090_1bf", 40f, 40f, fix: true);
		pos = origin + Offset(facing, 8f, 0f);
		Omen("general_1bpxf", 5f, 5f, fix: false, null, pos);
		slot++;
		SimpleElement.ShowText("UMAD telegraph preview");
		return true;
		float Delay()
		{
			return (float)slot * 3500f;
		}
		void Omen(string avfx, float rx, float rz, bool fix = false, Vector4? color = null, Vector3? vector = null)
		{
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = avfx,
				Position = vector.GetValueOrDefault(origin),
				drawOnObject = false,
				radiusX = rx,
				radiusZ = rz,
				refRotation = facing,
				fixRotation = fix,
				destroyTime = 3500f,
				delayDrawTime = (float)slot * 3500f
			};
			if (color.HasValue)
			{
				drawElement.refColor = color.Value;
				drawElement.refTargetColor = color.Value;
			}
			DrawManager.Draw(drawElement);
		}
	}

	private static Vector3 Offset(Angle facing, float dist, float degrees)
	{
		Angle angle = facing + degrees.Degrees();
		return new Vector3(dist * (float)Math.Sin(angle.Rad), 0f, dist * (float)Math.Cos(angle.Rad));
	}
}
