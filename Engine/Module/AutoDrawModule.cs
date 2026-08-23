using System;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.Sheets;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Engine.Module;

public static class AutoDrawModule
{
	public static void Run(ActorCastInfo info)
	{
		IGameObject gameObject = info.SourceId.GameObject();
		if (gameObject == null)
		{
			return;
		}
		IGameObject target = ((info.TargetId != 3758096384u && info.TargetId != info.SourceId) ? info.TargetId.GameObject() : null);
		Lumina.Excel.Sheets.Action row = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>().GetRow(info.ActionId);
		Shape shape = ShapeUtil.GetShape(row.CastType);
		if (row.CastType == 1 || (row.Omen.IsValid && !string.IsNullOrEmpty(row.Omen.Value.Path.ExtractText()) && info.DisplayDelay == 0) || (shape == Shape.Circle && row.EffectRange >= 50))
		{
			return;
		}
		float num = MathF.Max((float)(int)info.DisplayDelay / 10f - 4f, 0f) * 1000f;
		DrawElement element = new DrawElement
		{
			Actor = IGameObjectHelper.Find(info.SourceId),
			drawType = ElementType.Omen,
			Position = info.Pos,
			drawOnObject = false,
			radiusX = (int)row.EffectRange,
			radiusZ = (int)row.EffectRange,
			target = target,
			refRotation = info.Facing,
			delayDrawTime = num,
			destroyTime = info.CastTime * 1000f - num
		};
		if (!TrySetOmenPath(row, ref element))
		{
			element.drawAvfx = ShapeToAvfx(shape);
			if (string.IsNullOrEmpty(element.drawAvfx))
			{
				return;
			}
		}
		ApplyShapeSizing(ref element, shape, row, gameObject);
		Draw(element, shape, gameObject, info);
	}

	private static bool TrySetOmenPath(Lumina.Excel.Sheets.Action row, ref DrawElement element)
	{
		if (!row.Omen.IsValid)
		{
			return false;
		}
		string text = row.Omen.Value.Path.ExtractText();
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		element.drawAvfx = text;
		return true;
	}

	private static string ShapeToAvfx(Shape shape)
	{
		switch (shape)
		{
		case Shape.Circle:
			return "general_1bxf";
		case Shape.Rectangle:
		case Shape.RectToTarget:
			return "general02xf";
		case Shape.Cross:
			return "general_x02f";
		case Shape.Triangle:
			return "x6d3_b2_triangle90_p1";
		default:
			return string.Empty;
		}
	}

	private static void ApplyShapeSizing(ref DrawElement element, Shape shape, Lumina.Excel.Sheets.Action row, IGameObject source)
	{
		DrawElement drawElement = element;
		bool flag = ((shape == Shape.Rectangle || shape == Shape.RectToTarget) ? true : false);
		drawElement.radiusX = (flag ? ((float)(int)row.XAxisModifier / 2f) : element.radiusX);
		switch (row.CastType)
		{
		case 3:
		case 5:
			element.radiusX += source.HitboxRadius;
			element.radiusZ += source.HitboxRadius;
			break;
		case 4:
			element.radiusZ += source.HitboxRadius;
			break;
		}
		if (shape == Shape.RectToTarget)
		{
			element.endToTarget = true;
		}
	}

	private static void Draw(DrawElement element, Shape shape, IGameObject source, ActorCastInfo info)
	{
		if (shape == Shape.RectToTarget)
		{
			element.Position = source.Position;
			if (info.TargetId == 3758096384u)
			{
				element.targetPosition = info.TargetPos;
			}
			DrawManager.Draw(element);
		}
		else if (element.target != null && shape == Shape.Circle)
		{
			element.drawOnObject = true;
			DrawManager.Draw(element, element.target);
		}
		else
		{
			DrawManager.Draw(element);
			if (shape == Shape.Cross)
			{
				element.refRotation += 90.Degrees();
				DrawManager.Draw(element);
			}
		}
	}
}
