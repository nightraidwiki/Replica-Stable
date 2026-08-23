using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Everkeep;

public class ProjectionOfTriumph : ISpecialAction
{
	public override string Name => "Projection of Triumph (donut)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 16726)
		{
			float rotation = GameObject.Rotation;
			Vector2 vector = new Vector2(MathF.Sin(rotation), MathF.Cos(rotation));
			for (int i = 0; i < 4; i++)
			{
				Vector2 vector2 = new Vector2(100f, 100f) + (-15 + 10 * i) * vector;
				Vector2 vector3 = new Vector2(vector.Y, 0f - vector.X);
				for (int j = -15; j <= 15; j += 10)
				{
					DrawManager.Draw(new DrawElement
					{
						drawAvfx = "customCircle",
						Position = new Vector3(vector2.X + (float)j * vector3.X, 0f, vector2.Y + (float)j * vector3.Y),
						drawOnObject = false,
						radiusX = 4f,
						radiusZ = 4f,
						destroyTime = ((i == 0) ? 9000 : 5000),
						delayDrawTime = ((i != 0) ? (9000 + 5000 * (i - 1)) : 0),
						refColor = new Vector4(1f, 1f, 1f, 0.1f),
						refTargetColor = GroundOmen.enemyColor
					}, Svc.Objects.LocalPlayer);
				}
			}
		}
		if (GameObject.BaseId != 16727)
		{
			return;
		}
		float rotation2 = GameObject.Rotation;
		Vector2 vector4 = new Vector2(MathF.Sin(rotation2), MathF.Cos(rotation2));
		for (int k = 0; k < 4; k++)
		{
			Vector2 vector5 = new Vector2(100f, 100f) + (-15 + 10 * k) * vector4;
			Vector2 vector6 = new Vector2(vector4.Y, 0f - vector4.X);
			for (int l = -15; l <= 15; l += 10)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "customDonut",
					Position = new Vector3(vector5.X + (float)l * vector6.X, 0f, vector5.Y + (float)l * vector6.Y),
					drawOnObject = false,
					radiusX = 8f,
					radiusZ = 8f,
					refRadian = 0.375f,
					destroyTime = ((k == 0) ? 9000 : 5000),
					delayDrawTime = ((k != 0) ? (9000 + 5000 * (k - 1)) : 0),
					refColor = new Vector4(1f, 1f, 1f, 0.1f),
					refTargetColor = GroundOmen.enemyColor
				}, Svc.Objects.LocalPlayer);
			}
		}
	}
}
