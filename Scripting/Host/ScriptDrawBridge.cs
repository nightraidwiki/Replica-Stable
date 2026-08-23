using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;
using Replica.Logging;
using Replica.QuickDraws;
using Replica.Scripting.Api;

namespace Replica.Scripting.Host;

public sealed class ScriptDrawBridge
{
	private readonly Dictionary<string, HashSet<string>> _live = new Dictionary<string, HashSet<string>>();

	private readonly object _gate = new object();

	public void Send(string scriptGuid, DrawModeEnum mode, DrawTypeEnum type, DrawPropertiesEdit props)
	{
		QuickDrawEngine engine = Plugin.Instance?.Engine;
		if (engine == null || props == null)
		{
			return;
		}
		string text = (string.IsNullOrEmpty(props.Name) ? "draw" : props.Name);
		string ownerId = OwnerId(scriptGuid, text);
		lock (_gate)
		{
			if (!_live.TryGetValue(scriptGuid, out HashSet<string> value))
			{
				value = (_live[scriptGuid] = new HashSet<string>());
			}
			value.Add(text);
		}
		DrawSpec spec = Translate(type, props);
		LogEvent e = SyntheticEvent(props);
		Plugin.Framework.RunOnFrameworkThread(delegate
		{
			try
			{
				engine.SpawnExternal(ownerId, spec, e);
			}
			catch (Exception ex)
			{
				Plugin.Log.Error("[script draw] " + ex.Message);
			}
		});
	}

	public void Remove(string scriptGuid, string nameRegex)
	{
		QuickDrawEngine engine = Plugin.Instance?.Engine;
		if (engine == null)
		{
			return;
		}
		List<string> hits = new List<string>();
		lock (_gate)
		{
			if (!_live.TryGetValue(scriptGuid, out HashSet<string> value))
			{
				return;
			}
			foreach (string item in value)
			{
				bool flag;
				try
				{
					flag = string.IsNullOrEmpty(nameRegex) || Regex.IsMatch(item, nameRegex);
				}
				catch
				{
					flag = item == nameRegex;
				}
				if (flag)
				{
					hits.Add(item);
				}
			}
			foreach (string item2 in hits)
			{
				value.Remove(item2);
			}
		}
		if (hits.Count == 0)
		{
			return;
		}
		Plugin.Framework.RunOnFrameworkThread(delegate
		{
			foreach (string item3 in hits)
			{
				engine.ClearExternal(OwnerId(scriptGuid, item3));
			}
		});
	}

	public void ClearAll(string scriptGuid)
	{
		QuickDrawEngine engine = Plugin.Instance?.Engine;
		if (engine == null)
		{
			return;
		}
		List<string> names;
		lock (_gate)
		{
			if (!_live.TryGetValue(scriptGuid, out HashSet<string> value))
			{
				return;
			}
			names = new List<string>(value);
			value.Clear();
		}
		Plugin.Framework.RunOnFrameworkThread(delegate
		{
			foreach (string item in names)
			{
				engine.ClearExternal(OwnerId(scriptGuid, item));
			}
		});
	}

	private static string OwnerId(string scriptGuid, string name)
	{
		return "script:" + scriptGuid + ":" + name;
	}

	private static LogEvent SyntheticEvent(DrawPropertiesEdit p)
	{
		Vector3 vector = p.Position ?? Vector3.Zero;
		return new LogEvent
		{
			Kind = LogKind.Note,
			SourceId = (uint)p.Owner,
			TargetId = (uint)p.TargetObject,
			X = vector.X,
			Y = vector.Z
		};
	}

	private static DrawSpec Translate(DrawTypeEnum type, DrawPropertiesEdit p)
	{
		DrawSpec drawSpec = new DrawSpec
		{
			Color = p.Color,
			Rotation = p.Rotation * (180f / (float)Math.PI),
			Duration = MathF.Max(0.1f, (float)p.DestoryAt / 1000f),
			StartDelay = MathF.Max(0f, (float)p.Delay / 1000f),
			AttachToActor = (p.Owner != 0),
			OrientToFacing = (!p.FixRotation && p.Owner != 0)
		};
		if (p.Owner != 0L)
		{
			drawSpec.Anchor = DrawAnchor.Source;
		}
		else if (p.Position.HasValue)
		{
			drawSpec.Anchor = DrawAnchor.FixedPosition;
			drawSpec.FixedPosition = p.Position.Value;
		}
		else
		{
			drawSpec.Anchor = DrawAnchor.Self;
		}
		if (p.Offset.HasValue)
		{
			drawSpec.OffsetSide = p.Offset.Value.X;
			drawSpec.OffsetForward = p.Offset.Value.Z;
		}
		if (p.TargetPosition.HasValue)
		{
			drawSpec.Link = LinkTarget.FixedSpot;
			drawSpec.LinkPosition = p.TargetPosition.Value;
		}
		else if (p.TargetObject != 0L)
		{
			drawSpec.Link = LinkTarget.EventTarget;
		}
		else
		{
			drawSpec.Link = ResolveLink(p.TargetResolvePattern);
		}
		float x = p.Scale.X;
		float y = p.Scale.Y;
		switch (type)
		{
		case DrawTypeEnum.Circle:
			drawSpec.Shape = QuickShape.Circle;
			drawSpec.Radius = x;
			break;
		case DrawTypeEnum.Donut:
			drawSpec.Shape = QuickShape.Donut;
			drawSpec.Radius = x;
			drawSpec.InnerRadius = ((p.InnerScale.X > 0f) ? p.InnerScale.X : (x * 0.5f));
			break;
		case DrawTypeEnum.Fan:
		case DrawTypeEnum.SightAvoid:
			drawSpec.Shape = QuickShape.Fan;
			drawSpec.Radius = x;
			drawSpec.FanAngle = (int)MathF.Round(p.Radian * (180f / (float)Math.PI));
			break;
		case DrawTypeEnum.HotWing:
		case DrawTypeEnum.Rect:
		case DrawTypeEnum.Straight:
			drawSpec.Shape = QuickShape.Rectangle;
			drawSpec.HalfWidth = x * 0.5f;
			drawSpec.Length = y;
			break;
		case DrawTypeEnum.Line:
			drawSpec.Shape = QuickShape.Line;
			drawSpec.HalfWidth = MathF.Max(0.1f, x * 0.5f);
			break;
		case DrawTypeEnum.Displacement:
			drawSpec.Shape = QuickShape.ChevronPath;
			drawSpec.ChevronSpacing = ((x > 0f) ? x : 2f);
			drawSpec.Length = ((y > 0f) ? y : 20f);
			drawSpec.LineThickness = 4f;
			break;
		case DrawTypeEnum.Arrow:
			drawSpec.Shape = QuickShape.Arrow;
			drawSpec.Length = ((y > 0f) ? y : 10f);
			drawSpec.HalfWidth = MathF.Max(0.5f, x);
			drawSpec.LineThickness = 4f;
			break;
		case DrawTypeEnum.Text:
			drawSpec.Shape = QuickShape.Text;
			drawSpec.Label = p.Name ?? "Text";
			drawSpec.LabelSize = (x > 0f) ? x : 1.5f;
			break;
		case DrawTypeEnum.Path:
			drawSpec.Shape = QuickShape.ChevronPath;
			drawSpec.ChevronSpacing = ((x > 0f) ? x : 2f);
			drawSpec.Length = ((y > 0f) ? y : 20f);
			drawSpec.LineThickness = 4f;
			break;
		default:
			drawSpec.Shape = QuickShape.Circle;
			drawSpec.Radius = x;
			break;
		}
		return drawSpec;
	}

	private static LinkTarget ResolveLink(PositionResolvePatternEnum pattern)
	{
		return pattern switch
		{
			PositionResolvePatternEnum.OwnerTarget => LinkTarget.EventTarget, 
			PositionResolvePatternEnum.PlayerNearestOrder => LinkTarget.NearestPlayer, 
			PositionResolvePatternEnum.OwnerEnmityOrder => LinkTarget.NearestEnemy, 
			PositionResolvePatternEnum.TetherSource => LinkTarget.TetheredToMe, 
			PositionResolvePatternEnum.TetherTarget => LinkTarget.TetheredToMe, 
			_ => LinkTarget.EventTarget, 
		};
	}
}
