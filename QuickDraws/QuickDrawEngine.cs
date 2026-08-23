using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.Util;
using Replica.Engine.Vfx;
using Replica.Logging;

namespace Replica.QuickDraws;

public sealed class QuickDrawEngine
{
	private enum BindKind : byte
	{
		None,
		Cast,
		Status
	}

	private sealed class Tracked
	{
		public StaticVfx Vfx;

		public DateTime Expiry;

		public BindKind Bind;

		public uint BindSrc;

		public uint BindId;

		public string ShapeId = "";
	}

	private sealed class ShapeAnchor
	{
		public Func<Vector3?> Pos = () => (Vector3?)null;

		public Vector3 Last;

		public DateTime Expiry;

		public IGameObject? Owner;
	}

	private sealed class LiveLabel
	{
		public string OwnerId = "";

		public Func<Vector3?> World = () => (Vector3?)null;

		public string Text = "";

		public Vector4 Color;

		public float Size = 1f;

		public DateTime Expiry;

		public Vector2 Screen;

		public bool HasScreen;

		public bool FollowsActor;

		public Vector3 Anchor;

		public bool AnchorInit;

		public BindKind Bind;

		public uint BindSrc;

		public uint BindId;

		public Vector3 SmoothAnchor(Vector3 raw)
		{
			if (!AnchorInit)
			{
				Anchor = raw;
				AnchorInit = true;
				return raw;
			}
			Anchor = new Vector3(raw.X, Anchor.Y + (raw.Y - Anchor.Y) * 0.1f, raw.Z);
			return Anchor;
		}
	}

	private sealed class LiveArrow
	{
		public string OwnerId = "";

		public bool Chevron;

		public Func<Vector3?> Origin = () => (Vector3?)null;

		public Func<Vector3?> Target = () => (Vector3?)null;

		public bool HasTarget;

		public uint HeadingId;

		public bool Orient;

		public float Rotation;

		public float Length;

		public float Spacing;

		public float Thickness;

		public float HeadSize;

		public Vector4 Color;

		public DateTime Expiry;

		public BindKind Bind;

		public uint BindSrc;

		public uint BindId;

		public float? Heading;
	}

	public readonly record struct ArrowGeo(Vector3 Origin, float Angle, float Length, float Spacing, float Thickness, float HeadSize, Vector4 Color, bool Chevron);

	private sealed class ActiveCast
	{
		public float Duration;

		public DateTime Ends;
	}

	private sealed class ArmedFollow
	{
		public FollowUpStep Step;

		public LogEvent Ctx;

		public DateTime Expiry;

		public bool[] Met = Array.Empty<bool>();

		public LogEvent? Trigger;

		public string Key = "";
	}

	public readonly record struct FireMark(DateTime When, string Draw, string Trigger);

	private const double SuppressSeconds = 2.5;

	private readonly Configuration _config;

	private readonly IPluginLog _log;

	private readonly CombatLogCapture _capture;

	private readonly Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();

	private readonly Dictionary<string, DateTime> _lastFire = new Dictionary<string, DateTime>();

	private readonly Dictionary<string, string> _vars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private readonly List<(DateTime when, QuickDrawDef t, LogEvent e, string key)> _pending = new List<(DateTime, QuickDrawDef, LogEvent, string)>();

	private readonly List<(DateTime expiry, QuickDrawDef t, string key, uint subject)> _clearWatch = new List<(DateTime, QuickDrawDef, string, uint)>();

	private readonly List<(DateTime when, FollowUpStep s, LogEvent ctx, string key)> _pendingFollow = new List<(DateTime, FollowUpStep, LogEvent, string)>();

	private readonly List<ArmedFollow> _armedFollow = new List<ArmedFollow>();

	private readonly Dictionary<string, List<Tracked>> _live = new Dictionary<string, List<Tracked>>();

	private readonly Dictionary<string, ShapeAnchor> _shapeAnchors = new Dictionary<string, ShapeAnchor>();

	private readonly List<(DateTime when, string ownerId, DrawSpec d, LogEvent e)> _pendingShape = new List<(DateTime, string, DrawSpec, LogEvent)>();

	private readonly List<LiveLabel> _labels = new List<LiveLabel>();
	private readonly object _labelsLock = new object();

	private readonly List<LiveArrow> _arrows = new List<LiveArrow>();
	private readonly object _arrowsLock = new object();

	private const float ArrivedRadius = 0.35f;

	private const float HeadingHold = 1f;

	private readonly Dictionary<(uint actor, uint action), ActiveCast> _activeCasts = new Dictionary<(uint, uint), ActiveCast>();

	private readonly List<FireMark> _recentFires = new List<FireMark>();

	private static readonly Vector3 ArenaCenter = new Vector3(100f, 0f, 100f);

	private static readonly Regex VarTokenRx = new Regex("\\{\\$(\\w+)\\}", RegexOptions.Compiled);

	public IReadOnlyList<FireMark> RecentFires => _recentFires;

	public IEnumerable<ArrowGeo> ActiveArrows()
	{
		DateTime now = DateTime.Now;
		LiveArrow[] snapshot;
		lock (_arrowsLock)
		{
			_arrows.RemoveAll(a => a.Expiry <= now);
			snapshot = _arrows.ToArray();
		}
		foreach (LiveArrow arrow in snapshot)
		{
			Vector3? vector = arrow.Origin();
			if (!vector.HasValue)
			{
				continue;
			}
			Vector3 value = vector.Value;
			float angle = arrow.Rotation;
			float y = arrow.Length;
			if (arrow.HasTarget)
			{
				Vector3? vector2 = arrow.Target?.Invoke();
				if (!vector2.HasValue)
				{
					continue;
				}
				float num = vector2.Value.X - value.X;
				float num2 = vector2.Value.Z - value.Z;
				float num3 = MathF.Sqrt(num * num + num2 * num2);
				if (num3 >= 0.05f)
				{
					arrow.Heading = MathF.Atan2(num, num2);
					angle = arrow.Heading.Value + arrow.Rotation;
					y = num3;
				}
			}
			else if (arrow.Orient && arrow.HeadingId != 0)
			{
				angle = (Plugin.ObjectTable.SearchById(arrow.HeadingId)?.Rotation ?? 0f) + arrow.Rotation;
			}
			yield return new ArrowGeo(value, angle, MathF.Max(0.5f, y), MathF.Max(0.5f, arrow.Spacing), arrow.Thickness, arrow.HeadSize, arrow.Color, arrow.Chevron);
		}
	}

	public void RefreshLabelScreens()
	{
		DateTime now = DateTime.Now;
		LiveLabel[] snapshot;
		lock (_labelsLock)
		{
			_labels.RemoveAll(l => l.Expiry <= now);
			snapshot = _labels.ToArray();
		}
		foreach (LiveLabel label in snapshot)
		{
			label.HasScreen = false;
			Vector3? vector = label.World();
			if (vector.HasValue && PositionHelper.StableWorldToScreen(label.FollowsActor ? label.SmoothAnchor(vector.Value) : vector.Value, out var screen))
			{
				label.Screen = screen;
				label.HasScreen = true;
			}
		}
	}

	public IEnumerable<(Vector2 Screen, string Text, Vector4 Color, float Size)> ActiveLabelScreens()
	{
		DateTime now = DateTime.Now;
		LiveLabel[] snapshot;
		lock (_labelsLock)
		{
			_labels.RemoveAll(l => l.Expiry <= now);
			snapshot = _labels.ToArray();
		}
		foreach (LiveLabel label in snapshot)
		{
			if (label.HasScreen)
			{
				yield return (Screen: label.Screen, Text: label.Text, Color: label.Color, Size: label.Size);
			}
		}
	}

	public QuickDrawEngine(Configuration config, IPluginLog log, CombatLogCapture capture)
	{
		_config = config;
		_log = log;
		_capture = capture;
	}

	public void Handle(LogEvent e)
	{
		if (!_config.QuickDrawsEnabled)
		{
			return;
		}
		TrackCast(e);
		ReleaseBound(e);
		ProcessArmed(e);
		ProcessClearWatch(e);
		foreach (QuickDrawModule quickDrawModule in _config.QuickDrawModules)
		{
			if (!quickDrawModule.Enabled)
			{
				continue;
			}
			foreach (QuickDrawDef draw in quickDrawModule.Draws)
			{
				if (!draw.Enabled || !Matches(draw, e))
				{
					continue;
				}
				uint subject = TriggerSubject(draw, e);
				string text = InstanceKey(draw.Id, subject);
				double num = ((draw.Cooldown > 0.01f) ? ((double)draw.Cooldown) : 2.5);
				if ((!_lastFire.TryGetValue(text, out var value) || !((DateTime.Now - value).TotalSeconds < num)) && (ModeOf(draw) != Concurrency.Wait || !OwnerLive(text)))
				{
					_lastFire[text] = DateTime.Now;
					_recentFires.Add(new FireMark(DateTime.Now, draw.Name, string.IsNullOrEmpty(e.Name) ? e.SourceName : e.Name));
					if (_recentFires.Count > 40)
					{
						_recentFires.RemoveRange(0, _recentFires.Count - 40);
					}
					ApplyVars(draw, e);
					if (draw.DelaySeconds > 0.01f)
					{
						_pending.Add((DateTime.Now.AddSeconds(draw.DelaySeconds), draw, e, text));
					}
					else
					{
						Fire(draw, e, text);
					}
					ArmClear(draw, text, subject);
				}
			}
		}
	}

	public void Fire(QuickDrawDef t, LogEvent e)
	{
		Fire(t, e, InstanceKey(t.Id, TriggerSubject(t, e)));
	}

	public void SpawnExternal(string ownerId, DrawSpec d, LogEvent e, bool previewSelf = false)
	{
		SpawnShape(ownerId, d, e, previewSelf);
	}

	public void ClearExternal(string ownerId)
	{
		ClearOwner(ownerId);
	}

	private void Fire(QuickDrawDef t, LogEvent e, string key)
	{
		EnsureIds(t);
		if (ModeOf(t) == Concurrency.Replace)
		{
			ClearOwner(key);
		}
		if (t.DrawEnabled)
		{
			SpawnSpec(key, t.Draw, e);
		}
		foreach (DrawSpec extraShape in t.ExtraShapes)
		{
			SpawnSpec(key, extraShape, e);
		}
		DateTime now = DateTime.Now;
		foreach (FollowUpStep followUp in t.FollowUps)
		{
			string text = InstanceKey(followUp.Id, EventSubject(e));
			if (followUp.On == FollowUpOn.Timer)
			{
				_pendingFollow.Add((now.AddSeconds(Math.Max(0f, followUp.Seconds)), followUp, e, text));
				continue;
			}
			followUp.EnsureConditions();
			ArmedFollow armedFollow = new ArmedFollow
			{
				Step = followUp,
				Ctx = e,
				Expiry = now.AddSeconds(Math.Max(0.1f, followUp.Seconds)),
				Met = new bool[Math.Max(1, followUp.Conditions.Count)],
				Key = text
			};
			if (!TryAdvance(armedFollow, e))
			{
				_armedFollow.Add(armedFollow);
			}
		}
	}

	private void FireStep(FollowUpStep s, LogEvent ctx, string key)
	{
		if (s.DrawEnabled)
		{
			SpawnSpec(key, s.Draw, ctx);
		}
		foreach (DrawSpec extraShape in s.ExtraShapes)
		{
			SpawnSpec(key, extraShape, ctx);
		}
	}

	private static string InstanceKey(string id, uint subject)
	{
		if (subject != 0)
		{
			return id + "#" + subject;
		}
		return id;
	}

	private static uint TriggerSubject(QuickDrawDef t, LogEvent e)
	{
		switch (t.On)
		{
		case TriggerMatch.StatusGain:
		case TriggerMatch.StatusLose:
			return e.TargetId;
		case TriggerMatch.Headmarker:
			return e.SourceId;
		case TriggerMatch.Tether:
			return (e.TargetId != 0) ? e.TargetId : e.SourceId;
		case TriggerMatch.Cast:
		case TriggerMatch.Death:
		case TriggerMatch.CastEnd:
			return e.SourceId;
		default:
			return (e.TargetId != 0) ? e.TargetId : e.SourceId;
		}
	}

	private static uint EventSubject(LogEvent e)
	{
		if (e.TargetId == 0)
		{
			return e.SourceId;
		}
		return e.TargetId;
	}

	public void Preview(QuickDrawDef t)
	{
		EnsureIds(t);
		LogEvent e = new LogEvent
		{
			Name = (string.IsNullOrEmpty(t.Pattern) ? "Sample" : t.Pattern)
		};
		ClearOwner(t.Id);
		if (t.DrawEnabled)
		{
			SpawnShape(t.Id, t.Draw, e, previewSelf: true);
		}
		foreach (DrawSpec extraShape in t.ExtraShapes)
		{
			SpawnShape(t.Id, extraShape, e, previewSelf: true);
		}
	}

	public void PreviewShape(QuickDrawDef t, DrawSpec d)
	{
		EnsureIds(t);
		LogEvent e = new LogEvent
		{
			Name = "Sample"
		};
		ClearOwner("preview_shape");
		foreach (DrawSpec item in DependencyShapes(t, d))
		{
			SpawnShape("preview_shape", item, e, previewSelf: true);
		}
		SpawnShape("preview_shape", d, e, previewSelf: true);
	}

	public static void EnsureIds(QuickDrawDef t)
	{
		t.Draw.EnsureId();
		foreach (DrawSpec extraShape in t.ExtraShapes)
		{
			extraShape.EnsureId();
		}
		foreach (FollowUpStep followUp in t.FollowUps)
		{
			followUp.Draw.EnsureId();
			foreach (DrawSpec extraShape2 in followUp.ExtraShapes)
			{
				extraShape2.EnsureId();
			}
		}
		EnsureLinkIds(t);
	}

	private static void EnsureLinkIds(QuickDrawDef t)
	{
		foreach (DrawSpec item in EnumerateDraws(t))
		{
			if (item.Anchor == DrawAnchor.LinkedShape && string.IsNullOrEmpty(item.AnchorShapeId))
			{
				string text = FirstLinkableShape(t, item.Id);
				if (text != null)
				{
					item.AnchorShapeId = text;
				}
			}
			if (item.Link == LinkTarget.LinkedShape && string.IsNullOrEmpty(item.LinkShapeId))
			{
				string text2 = FirstLinkableShape(t, item.Id);
				if (text2 != null)
				{
					item.LinkShapeId = text2;
				}
			}
		}
	}

	private static IEnumerable<DrawSpec> EnumerateDraws(QuickDrawDef t)
	{
		yield return t.Draw;
		foreach (DrawSpec extraShape in t.ExtraShapes)
		{
			yield return extraShape;
		}
		foreach (FollowUpStep fu in t.FollowUps)
		{
			yield return fu.Draw;
			foreach (DrawSpec extraShape2 in fu.ExtraShapes)
			{
				yield return extraShape2;
			}
		}
	}

	private static string? FirstLinkableShape(QuickDrawDef t, string excludeId)
	{
		if (t.Draw.Id != excludeId)
		{
			return t.Draw.Id;
		}
		foreach (DrawSpec extraShape in t.ExtraShapes)
		{
			if (extraShape.Id != excludeId)
			{
				return extraShape.Id;
			}
		}
		foreach (FollowUpStep followUp in t.FollowUps)
		{
			if (followUp.Draw.Id != excludeId)
			{
				return followUp.Draw.Id;
			}
			foreach (DrawSpec extraShape2 in followUp.ExtraShapes)
			{
				if (extraShape2.Id != excludeId)
				{
					return extraShape2.Id;
				}
			}
		}
		return null;
	}

	private void SpawnShape(string ownerId, DrawSpec d, LogEvent e, bool previewSelf = false)
	{
		Vector3? pos = ResolvePosition(d, e, previewSelf, out IGameObject attach);
		if (!pos.HasValue && attach == null && d.Anchor != DrawAnchor.LinkedShape)
		{
			return;
		}
		int num = Math.Max(1, d.Repeat);
		for (int i = 0; i < num; i++)
		{
			DrawSpec drawSpec = d;
			if (i > 0)
			{
				float num2 = (float)i * d.RepeatStep;
				drawSpec = d.Clone();
				drawSpec.Rotation = d.Rotation + num2;
				float x = num2 * (float)Math.PI / 180f;
				float num3 = MathF.Cos(x);
				float num4 = MathF.Sin(x);
				drawSpec.OffsetForward = d.OffsetForward * num3 - d.OffsetSide * num4;
				drawSpec.OffsetSide = d.OffsetForward * num4 + d.OffsetSide * num3;
			}
			SpawnOne(ownerId, drawSpec, e, pos, attach);
		}
		SpawnLabel(ownerId, d, e, pos, attach, previewSelf);
		QuickShape shape = d.Shape;
		if (shape - 8 <= QuickShape.Fan)
		{
			d.EnsureId();
			RegisterPointAnchor(d.Id, BuildAnchorPosFunc(d, pos, attach), ResolveEventLife(d, e));
		}
	}

	private Func<Vector3?> BuildAnchorPosFunc(DrawSpec d, Vector3? pos, IGameObject? attach)
	{
		bool flag = d.AttachToActor && attach != null && d.Anchor != DrawAnchor.LinkedShape;
		uint followId = (flag ? attach.EntityId : 0u);
		Vector3 fixedPos = pos ?? attach?.Position ?? new Vector3(100f, 0f, 100f);
		if (followId != 0)
		{
			return () => Plugin.ObjectTable.SearchById(followId)?.Position ?? fixedPos;
		}
		if (d.Anchor == DrawAnchor.LinkedShape)
		{
			return () => ResolveLinkedShapePos(d.AnchorShapeId).GetValueOrDefault(fixedPos);
		}
		return () => fixedPos;
	}

	private void RegisterPointAnchor(string id, Func<Vector3?> pos, float life)
	{
		if (!string.IsNullOrEmpty(id))
		{
			_shapeAnchors[id] = new ShapeAnchor
			{
				Expiry = DateTime.Now.AddSeconds(life),
				Pos = pos
			};
		}
	}

	private void SpawnOne(string ownerId, DrawSpec d, LogEvent e, Vector3? pos, IGameObject? attach)
	{
		if (d.Shape == QuickShape.Text)
		{
			SpawnLabel(ownerId, d, e, pos, attach, false);
			return;
		}
		QuickShape shape = d.Shape;
		if (shape == QuickShape.Arrow || shape == QuickShape.ChevronPath)
		{
			SpawnArrow(ownerId, d, e, pos, attach);
			return;
		}
		d.EnsureId();
		float num = ResolveEventLife(d, e);
		float destroyTime = Math.Max(0.1f, num) * 1000f;
		bool flag = d.AttachToActor && attach != null && d.Anchor != DrawAnchor.LinkedShape;
		IGameObject gameObject = ((d.Anchor == DrawAnchor.LinkedShape) ? ResolveLinkedShapeOwner(d.AnchorShapeId) : null);
		bool faceActor = d.OrientToFacing && (flag ? (attach != null) : (gameObject != null));
		DrawElement elem = new DrawElement
		{
			Position = (pos ?? attach?.RenderPosition() ?? new Vector3(100f, 0f, 100f)),
			drawOnObject = flag,
			refColor = d.Color,
			refTargetColor = d.Color,
			destroyTime = destroyTime,
			refOffsetZ = 0f - d.OffsetForward,
			refOffsetX = 0f - d.OffsetSide
		};
		if (d.Anchor == DrawAnchor.LinkedShape)
		{
			elem.drawOnObject = false;
			elem.PositionCustomAction = () => ResolveLinkedShapePos(d.AnchorShapeId) ?? elem.Position;
			if (d.OrientToFacing && gameObject != null)
			{
				elem.fixRotation = false;
				elem.RotationCustomAction = delegate
				{
					IGameObject gameObject3 = ResolveLinkedShapeOwner(d.AnchorShapeId);
					return (gameObject3 == null) ? d.Rotation.Degrees() : (gameObject3.Rotation.Radians() + d.Rotation.Degrees());
				};
			}
		}
		IGameObject gameObject2 = (flag ? attach : null);
		if (!ApplyLegacyCustom(elem, d, faceActor))
		{
			switch (d.Shape)
			{
			case QuickShape.Circle:
				elem.drawAvfx = "customCircle";
				elem.radiusX = d.Radius;
				elem.radiusZ = d.Radius;
				break;
			case QuickShape.Donut:
			{
				elem.drawAvfx = "customDonut";
				float num2 = MathF.Max(d.Radius, d.InnerRadius + 0.1f);
				float num3 = MathF.Min(d.InnerRadius, num2 - 0.1f);
				elem.radiusX = num2;
				elem.radiusZ = num2;
				elem.refRadian = num3 / num2;
				break;
			}
			case QuickShape.Fan:
				elem.drawAvfx = "customFan";
				elem.refRadian = d.FanAngle.Degrees().Rad;
				elem.radiusX = d.Radius;
				elem.radiusZ = d.Radius;
				ApplyRotation(elem, d, faceActor);
				break;
			case QuickShape.Rectangle:
				if (d.SpanToTarget)
				{
					SetupLine(elem, d, e, attach, flag);
					break;
				}
				elem.drawAvfx = "customRect";
				elem.radiusX = d.HalfWidth;
				elem.radiusZ = d.Length;
				ApplyRotation(elem, d, faceActor);
				break;
			case QuickShape.Line:
				SetupLine(elem, d, e, attach, flag);
				break;
			case QuickShape.Tower:
				elem.drawAvfx = "tower_noc";
				elem.radiusX = d.Radius;
				elem.radiusZ = d.Radius;
				elem.radiusY = 1f;
				break;
			case QuickShape.Knockback:
				elem.drawAvfx = "knockback_noc";
				elem.radiusX = d.Radius;
				elem.radiusZ = d.Radius;
				elem.radiusY = 1f;
				ApplyRotation(elem, d, faceActor);
				break;
			case QuickShape.Laser:
				elem.drawAvfx = "laser_noc";
				elem.radiusX = d.HalfWidth;
				elem.radiusZ = d.Length;
				elem.radiusY = 1f;
				ApplyRotation(elem, d, faceActor);
				break;
			}
		}
		StaticVfx staticVfx = DrawManager.Draw(elem, flag ? gameObject2 : null);
		if (staticVfx == null)
		{
			return;
		}
		if (!_live.TryGetValue(ownerId, out List<Tracked> value))
		{
			value = new List<Tracked>();
			_live[ownerId] = value;
		}
		Tracked tracked = new Tracked
		{
			Vfx = staticVfx,
			Expiry = DateTime.Now.AddSeconds(num),
			ShapeId = d.Id
		};
		if (d.UseEventDuration)
		{
			if (e.Kind == Replica.Logging.LogKind.CastStart && e.SourceId != 0)
			{
				tracked.Bind = BindKind.Cast;
				tracked.BindSrc = e.SourceId;
				tracked.BindId = e.DataId;
			}
			else if (e.Kind == Replica.Logging.LogKind.StatusGain && e.TargetId != 0)
			{
				tracked.Bind = BindKind.Status;
				tracked.BindSrc = e.TargetId;
				tracked.BindId = e.DataId;
			}
		}
		value.Add(tracked);
		RegisterShapeAnchor(d.Id, staticVfx, num);
	}

	private void SpawnLabel(string ownerId, DrawSpec d, LogEvent e, Vector3? pos, IGameObject? attach, bool previewSelf)
	{
		string textToDraw = d.Label;
		if (string.IsNullOrWhiteSpace(textToDraw))
		{
			return;
		}
		float num = ResolveEventLife(d, e);
		bool flag = d.AttachToActor && attach != null;
		uint followId = (flag ? attach.EntityId : 0u);
		Vector3 fixedPos = pos ?? attach?.Position ?? new Vector3(100f, 0f, 100f);
		if (d.Anchor == DrawAnchor.LinkedShape)
		{
			followId = 0u;
			fixedPos = ResolveLinkedShapePos(d.AnchorShapeId).GetValueOrDefault(fixedPos);
		}
		float labelHeight = d.LabelHeight > 0.01f ? d.LabelHeight : 1.5f;
		Vector3 up = new Vector3(0f, labelHeight, 0f);
		Vector4 vector;
		if (!(d.LabelColor.W <= 0.01f))
		{
			vector = d.LabelColor;
		}
		else
		{
			Vector4 labelColor = d.LabelColor;
			labelColor.W = 1f;
			vector = labelColor;
		}
		Vector4 color = vector;
		LiveLabel liveLabel = new LiveLabel
		{
			OwnerId = ownerId,
			FollowsActor = (followId != 0),
			World = ((followId != 0) ? ((Func<Vector3?>)delegate
			{
				IGameObject gameObject = Plugin.ObjectTable.SearchById(followId);
				return (gameObject != null) ? new Vector3?(gameObject.Position + up) : ((Vector3?)null);
			}) : ((d.Anchor == DrawAnchor.LinkedShape) ? ((Func<Vector3?>)(() => ResolveLinkedShapePos(d.AnchorShapeId).GetValueOrDefault(fixedPos) + up)) : ((Func<Vector3?>)(() => fixedPos + up)))),
			Text = textToDraw,
			Color = color,
			Size = MathF.Max(0.3f, d.LabelSize),
			Expiry = DateTime.Now.AddSeconds(num)
		};
		lock (_labelsLock)
		{
			_labels.Add(liveLabel);
		}
		if (d.UseEventDuration)
		{
			if (e.Kind == Replica.Logging.LogKind.CastStart && e.SourceId != 0)
			{
				liveLabel.Bind = BindKind.Cast;
				liveLabel.BindSrc = e.SourceId;
				liveLabel.BindId = e.DataId;
			}
			else if (e.Kind == Replica.Logging.LogKind.StatusGain && e.TargetId != 0)
			{
				liveLabel.Bind = BindKind.Status;
				liveLabel.BindSrc = e.TargetId;
				liveLabel.BindId = e.DataId;
			}
		}
	}

	private static bool IsPositionalLink(LinkTarget l)
	{
		if (l != LinkTarget.FixedSpot && l != LinkTarget.ArenaCenter)
		{
			if ((int)l >= 8)
			{
				return (int)l <= 15;
			}
			return false;
		}
		return true;
	}

	private void SpawnArrow(string ownerId, DrawSpec d, LogEvent e, Vector3? pos, IGameObject? attach)
	{
		d.EnsureId();
		float num = ResolveEventLife(d, e);
		bool flag = d.AttachToActor && attach != null && d.Anchor != DrawAnchor.LinkedShape;
		uint followId = (flag ? attach.EntityId : 0u);
		Vector3 fixedPos = pos ?? attach?.Position ?? new Vector3(100f, 0f, 100f);
		Func<Vector3?> origin = ((followId != 0) ? ((Func<Vector3?>)(() => Plugin.ObjectTable.SearchById(followId)?.Position)) : ((d.Anchor == DrawAnchor.LinkedShape) ? ((Func<Vector3?>)(() => ResolveLinkedShapePos(d.AnchorShapeId).GetValueOrDefault(fixedPos))) : ((Func<Vector3?>)(() => fixedPos))));
		uint farId = ResolveLink(d, e, attach)?.EntityId ?? 0;
		bool hasTarget = true;
		Func<Vector3?> target;
		if (farId != 0)
		{
			target = () => Plugin.ObjectTable.SearchById(farId)?.Position;
		}
		else if (d.Link == LinkTarget.LinkedShape)
		{
			target = () => ResolveLinkedShapePos(d.LinkShapeId);
		}
		else if (IsPositionalLink(d.Link))
		{
			Vector3 fp = ResolveLinkPosition(d, e);
			target = () => fp;
		}
		else
		{
			target = () => (Vector3?)null;
			hasTarget = false;
		}
		Vector4 vector;
		if (!(d.Color.W <= 0.01f))
		{
			vector = d.Color;
		}
		else
		{
			Vector4 color = d.Color;
			color.W = 1f;
			vector = color;
		}
		Vector4 color2 = vector;
		LiveArrow liveArrow = new LiveArrow
		{
			OwnerId = ownerId,
			Chevron = (d.Shape == QuickShape.ChevronPath),
			Origin = origin,
			Target = target,
			HasTarget = hasTarget,
			HeadingId = followId,
			Orient = d.OrientToFacing,
			Rotation = d.Rotation * ((float)Math.PI / 180f),
			Length = d.Length,
			Spacing = d.ChevronSpacing,
			Thickness = MathF.Max(1f, d.LineThickness),
			HeadSize = MathF.Max(0.5f, d.HalfWidth),
			Color = color2,
			Expiry = DateTime.Now.AddSeconds(num)
		};
		if (d.UseEventDuration)
		{
			if (e.Kind == Replica.Logging.LogKind.CastStart && e.SourceId != 0)
			{
				liveArrow.Bind = BindKind.Cast;
				liveArrow.BindSrc = e.SourceId;
				liveArrow.BindId = e.DataId;
			}
			else if (e.Kind == Replica.Logging.LogKind.StatusGain && e.TargetId != 0)
			{
				liveArrow.Bind = BindKind.Status;
				liveArrow.BindSrc = e.TargetId;
				liveArrow.BindId = e.DataId;
			}
		}
		lock (_arrowsLock)
		{
			_arrows.Add(liveArrow);
		}
	}

	private void SpawnSpec(string ownerId, DrawSpec d, LogEvent e, bool previewSelf = false)
	{
		if (!previewSelf && d.StartDelay > 0.01f)
		{
			_pendingShape.Add((DateTime.Now.AddSeconds(d.StartDelay), ownerId, d, e));
		}
		else
		{
			SpawnShape(ownerId, d, e, previewSelf);
		}
	}

	private void ReleaseBound(LogEvent e)
	{
		Replica.Logging.LogKind kind = e.Kind;
		if ((kind == Replica.Logging.LogKind.CastFinish || kind == Replica.Logging.LogKind.Ability) ? true : false)
		{
			RemoveBound(BindKind.Cast, e.SourceId, e.DataId);
		}
		else if (e.Kind == Replica.Logging.LogKind.StatusLose)
		{
			RemoveBound(BindKind.Status, e.TargetId, e.DataId);
		}
	}

	private void RemoveBound(BindKind kind, uint src, uint id)
	{
		if (src == 0 || id == 0)
		{
			return;
		}
		foreach (List<Tracked> value in _live.Values)
		{
			for (int num = value.Count - 1; num >= 0; num--)
			{
				Tracked tracked = value[num];
				if (tracked.Bind == kind && tracked.BindSrc == src && tracked.BindId == id)
				{
					UnregisterShapeAnchor(tracked.ShapeId);
					try
					{
						tracked.Vfx.Remove();
					}
					catch
					{
					}
					value.RemoveAt(num);
				}
			}
		}
		lock (_labelsLock)
		{
			_labels.RemoveAll((LiveLabel l) => l.Bind == kind && l.BindSrc == src && l.BindId == id);
		}
		lock (_arrowsLock)
		{
			_arrows.RemoveAll((LiveArrow a) => a.Bind == kind && a.BindSrc == src && a.BindId == id);
		}
	}

	private static void ApplyRotation(DrawElement elem, DrawSpec d, bool faceActor)
	{
		if (faceActor)
		{
			elem.fixRotation = false;
			elem.refRotation = d.Rotation.Degrees();
		}
		else
		{
			elem.fixRotation = true;
			elem.refRotation = d.Rotation.Degrees();
		}
	}

	private static bool ApplyLegacyCustom(DrawElement elem, DrawSpec d, bool faceActor)
	{
		if (string.IsNullOrWhiteSpace(d.CustomVfx))
		{
			return false;
		}
		elem.drawAvfx = d.CustomVfx.Trim();
		elem.radiusY = 1f;
		elem.radiusX = ((d.HalfWidth > 0.1f) ? d.HalfWidth : d.Radius);
		elem.radiusZ = ((d.Length > 0.1f) ? d.Length : d.Radius);
		ApplyRotation(elem, d, faceActor);
		return true;
	}

	private void TrackCast(LogEvent e)
	{
		if (e.Kind == Replica.Logging.LogKind.CastStart && e.Value > 0.05f)
		{
			_activeCasts[(e.SourceId, e.DataId)] = new ActiveCast
			{
				Duration = e.Value,
				Ends = DateTime.Now.AddSeconds(e.Value)
			};
		}
		else if (e.Kind == Replica.Logging.LogKind.CastFinish)
		{
			_activeCasts.Remove((e.SourceId, e.DataId));
		}
	}

	private float ResolveEventLife(DrawSpec d, LogEvent e)
	{
		if (!d.UseEventDuration)
		{
			return d.Duration;
		}
		if (e.Value > 0.1f)
		{
			return RemainingCastLife(e);
		}
		Replica.Logging.LogKind kind = e.Kind;
		if ((kind == Replica.Logging.LogKind.CastStart || kind == Replica.Logging.LogKind.Ability) ? true : false)
		{
			return LookupCastLife(e.SourceId, e.DataId) ?? d.Duration;
		}
		if (e.Kind == Replica.Logging.LogKind.StatusGain)
		{
			return LookupStatusLife(e.TargetId, e.DataId) ?? d.Duration;
		}
		return d.Duration;
	}

	private float RemainingCastLife(LogEvent e)
	{
		if (_activeCasts.TryGetValue((e.SourceId, e.DataId), out ActiveCast value))
		{
			float num = (float)(value.Ends - DateTime.Now).TotalSeconds;
			if (num > 0.1f)
			{
				return num;
			}
		}
		if (Plugin.ObjectTable.SearchById(e.SourceId) is IBattleChara { IsCasting: not false } battleChara && battleChara.CastActionId == e.DataId)
		{
			float num2 = battleChara.TotalCastTime - battleChara.CurrentCastTime;
			if (num2 > 0.1f)
			{
				return num2;
			}
		}
		return e.Value;
	}

	private float? LookupCastLife(uint actorId, uint actionId)
	{
		if (_activeCasts.TryGetValue((actorId, actionId), out ActiveCast value))
		{
			float num = (float)(value.Ends - DateTime.Now).TotalSeconds;
			if (num > 0.1f)
			{
				return num;
			}
			if (value.Duration > 0.1f)
			{
				return value.Duration;
			}
		}
		if (Plugin.ObjectTable.SearchById(actorId) is IBattleChara { IsCasting: not false } battleChara && battleChara.CastActionId == actionId)
		{
			float num2 = battleChara.TotalCastTime - battleChara.CurrentCastTime;
			if (num2 > 0.1f)
			{
				return num2;
			}
		}
		return null;
	}

	private static float? LookupStatusLife(uint actorId, uint statusId)
	{
		if (actorId == 0 || statusId == 0)
		{
			return null;
		}
		if (!(Plugin.ObjectTable.SearchById(actorId) is IBattleChara battleChara))
		{
			return null;
		}
		foreach (IStatus status in battleChara.StatusList)
		{
			if (status.StatusId == statusId)
			{
				if (status.RemainingTime > 0.1f)
				{
					return status.RemainingTime;
				}
				break;
			}
		}
		return null;
	}

	private void SetupLine(DrawElement elem, DrawSpec d, LogEvent e, IGameObject? anchor, bool glue)
	{
		elem.drawAvfx = "customRect";
		elem.radiusX = MathF.Max(0.1f, d.HalfWidth);
		elem.radiusY = 1f;
		elem.radiusZ = 1f;
		elem.endToTarget = true;
		elem.drawOnObject = glue;
		IGameObject gameObject = ResolveLink(d, e, anchor);
		if (gameObject != null)
		{
			elem.target = gameObject;
		}
		else if (d.Link == LinkTarget.LinkedShape)
		{
			elem.TargetPositionCustomAction = () => ResolveLinkedShapePos(d.LinkShapeId) ?? elem.targetPosition;
		}
		else
		{
			elem.targetPosition = ResolveLinkPosition(d, e);
		}
	}

	private static Vector3 ResolveLinkPosition(DrawSpec d, LogEvent e)
	{
		switch (d.Link)
		{
		case LinkTarget.FixedSpot:
			return d.LinkPosition;
		case LinkTarget.ArenaCenter:
			return ArenaCenter;
		case LinkTarget.WaymarkA:
		case LinkTarget.WaymarkB:
		case LinkTarget.WaymarkC:
		case LinkTarget.WaymarkD:
		case LinkTarget.Waymark1:
		case LinkTarget.Waymark2:
		case LinkTarget.Waymark3:
		case LinkTarget.Waymark4:
			return Waymark((int)(d.Link - 8)) ?? d.LinkPosition;
		default:
			return new Vector3(e.X, 0f, e.Y);
		}
	}

	private IGameObject? ResolveLink(DrawSpec d, LogEvent e, IGameObject? anchor)
	{
		return d.Link switch
		{
			LinkTarget.EventTarget => Actor(e.TargetId), 
			LinkTarget.EventSource => Actor(e.SourceId), 
			LinkTarget.MyTarget => Actor(Plugin.PlayerState.EntityId)?.TargetObject, 
			LinkTarget.NearestPlayer => Nearest(anchor, onlyPlayers: true, wantEnemy: false), 
			LinkTarget.NearestEnemy => Nearest(anchor, onlyPlayers: false, wantEnemy: true), 
			LinkTarget.PlayerWithSameStatus => PlayerWithStatus(anchor, e.DataId), 
			LinkTarget.TetheredToMe => TetheredActor(d.TetherFilterId), 
			LinkTarget.LinkedShape => null, 
			_ => null, 
		};
	}

	private IGameObject? TetheredActor(uint tetherId)
	{
		uint entityId = Plugin.PlayerState.EntityId;
		if (entityId == 0)
		{
			return null;
		}
		foreach (CombatLogCapture.LiveTether activeTether in _capture.ActiveTethers)
		{
			if (tetherId == 0 || activeTether.Id == tetherId)
			{
				if (activeTether.To == entityId && activeTether.From != 0)
				{
					return Actor(activeTether.From);
				}
				if (activeTether.From == entityId && activeTether.To != 0)
				{
					return Actor(activeTether.To);
				}
			}
		}
		return null;
	}

	private static IGameObject? Nearest(IGameObject? from, bool onlyPlayers, bool wantEnemy)
	{
		if (from == null)
		{
			return null;
		}
		IGameObject result = null;
		float num = float.MaxValue;
		foreach (IGameObject item in Plugin.ObjectTable)
		{
			if (item.EntityId == from.EntityId)
			{
				continue;
			}
			bool flag = item.ObjectKind == ObjectKind.Pc;
			bool flag2 = item is IBattleNpc;
			if ((!onlyPlayers || flag) && (!wantEnemy || flag2) && item is IBattleChara { CurrentHp: not 0u })
			{
				float num2 = Vector3.DistanceSquared(item.Position, from.Position);
				if (num2 < num)
				{
					num = num2;
					result = item;
				}
			}
		}
		return result;
	}

	private static IGameObject? PlayerWithStatus(IGameObject? exclude, uint statusId)
	{
		if (statusId == 0)
		{
			return null;
		}
		foreach (IGameObject item in Plugin.ObjectTable)
		{
			if ((exclude != null && item.EntityId == exclude.EntityId) || item.ObjectKind != ObjectKind.Pc || !(item is IBattleChara battleChara))
			{
				continue;
			}
			foreach (IStatus status in battleChara.StatusList)
			{
				if (status.StatusId == statusId)
				{
					return item;
				}
			}
		}
		return null;
	}

	private Vector3? ResolvePosition(DrawSpec d, LogEvent e, bool previewSelf, out IGameObject? attach)
	{
		attach = null;
		if (previewSelf && d.Anchor != DrawAnchor.FixedPosition && (int)d.Anchor < 6)
		{
			IGameObject gameObject = LocalPlayer();
			if (gameObject != null)
			{
				attach = gameObject;
				return gameObject.Position;
			}
			return new Vector3(100f, 0f, 100f);
		}
		switch (d.Anchor)
		{
		case DrawAnchor.Source:
			attach = Actor(e.SourceId);
			return attach?.Position;
		case DrawAnchor.Target:
			attach = Actor(e.TargetId);
			return attach?.Position;
		case DrawAnchor.Self:
			attach = LocalPlayer();
			return attach?.Position;
		case DrawAnchor.EventPosition:
			return new Vector3(e.X, 0f, e.Y);
		case DrawAnchor.TetheredToMe:
			attach = TetheredActor(d.TetherFilterId);
			return attach?.Position;
		case DrawAnchor.ArenaCenter:
			return ArenaCenter;
		case DrawAnchor.WaymarkA:
		case DrawAnchor.WaymarkB:
		case DrawAnchor.WaymarkC:
		case DrawAnchor.WaymarkD:
		case DrawAnchor.Waymark1:
		case DrawAnchor.Waymark2:
		case DrawAnchor.Waymark3:
		case DrawAnchor.Waymark4:
			return Waymark((int)(d.Anchor - 6));
		case DrawAnchor.LinkedShape:
			return ResolveLinkedShapePos(d.AnchorShapeId);
		case DrawAnchor.NearbyActorById:
			attach = NearestByBaseId(d.AnchorActorBaseId, e);
			return attach?.RenderPosition();
		default:
			return d.FixedPosition;
		}
	}

	private unsafe static Vector3? Waymark(int index)
	{
		MarkingController* ptr = MarkingController.Instance();
		if (ptr == null)
		{
			return null;
		}
		int num = 0;
		Span<FFXIVClientStructs.FFXIV.Client.Game.UI.FieldMarker> fieldMarkers = ptr->FieldMarkers;
		for (int i = 0; i < fieldMarkers.Length; i++)
		{
			ref FFXIVClientStructs.FFXIV.Client.Game.UI.FieldMarker reference = ref fieldMarkers[i];
			if (num == index)
			{
				if (!reference.Active)
				{
					return null;
				}
				return new Vector3((float)reference.X / 1000f, (float)reference.Y / 1000f, (float)reference.Z / 1000f);
			}
			num++;
		}
		return null;
	}

	private static IGameObject? Actor(uint id)
	{
		if (id != 0)
		{
			return Plugin.ObjectTable.SearchById(id);
		}
		return null;
	}

	private static IGameObject? LocalPlayer()
	{
		IGameObject localPlayer = Plugin.ObjectTable.LocalPlayer;
		return localPlayer ?? Actor(Plugin.PlayerState.EntityId);
	}

	private bool OwnerLive(string id)
	{
		PruneOwner(id);
		if (_live.TryGetValue(id, out List<Tracked> value))
		{
			return value.Count > 0;
		}
		return false;
	}

	private void ClearOwner(string id)
	{
		_pendingShape.RemoveAll(((DateTime when, string ownerId, DrawSpec d, LogEvent e) p) => p.ownerId == id);
		lock (_labelsLock)
		{
			_labels.RemoveAll((LiveLabel l) => l.OwnerId == id);
		}
		lock (_arrowsLock)
		{
			_arrows.RemoveAll((LiveArrow a) => a.OwnerId == id);
		}
		if (!_live.TryGetValue(id, out List<Tracked> value))
		{
			return;
		}
		foreach (Tracked item in value)
		{
			UnregisterShapeAnchor(item.ShapeId);
			try
			{
				item.Vfx.Remove();
			}
			catch
			{
			}
		}
		value.Clear();
	}

	private void PruneOwner(string id)
	{
		if (_live.TryGetValue(id, out List<Tracked> value))
		{
			DateTime now = DateTime.Now;
			value.RemoveAll((Tracked t) => t.Expiry <= now);
		}
	}

	private void ProcessArmed(LogEvent e)
	{
		if (_armedFollow.Count == 0)
		{
			return;
		}
		DateTime now = DateTime.Now;
		for (int num = _armedFollow.Count - 1; num >= 0; num--)
		{
			ArmedFollow armedFollow = _armedFollow[num];
			if (now > armedFollow.Expiry)
			{
				_armedFollow.RemoveAt(num);
			}
			else if (TryAdvance(armedFollow, e))
			{
				_armedFollow.RemoveAt(num);
			}
		}
	}

	private bool TryAdvance(ArmedFollow a, LogEvent e)
	{
		if (!KindMatches(a.Step.On, e))
		{
			return false;
		}
		List<FollowCond> conditions = a.Step.Conditions;
		bool flag = false;
		for (int i = 0; i < conditions.Count; i++)
		{
			if (!a.Met[i] && CondMatches(a.Step.On, conditions[i], e))
			{
				a.Met[i] = true;
				flag = true;
				if ((object)a.Trigger == null)
				{
					a.Trigger = e;
				}
			}
		}
		if (conditions.Count == 0 && !flag)
		{
			a.Trigger = e;
			a.Met = new bool[1] { true };
			flag = true;
		}
		if (!flag)
		{
			return false;
		}
		if (a.Step.RequireAll && !AllMet(a.Met))
		{
			return false;
		}
		FireStep(a.Step, a.Trigger ?? e, a.Key);
		return true;
	}

	private static bool AllMet(bool[] met)
	{
		for (int i = 0; i < met.Length; i++)
		{
			if (!met[i])
			{
				return false;
			}
		}
		return true;
	}

	private static bool KindMatches(FollowUpOn on, LogEvent e)
	{
		switch (on)
		{
		case FollowUpOn.Cast:
			return e.Kind == Replica.Logging.LogKind.CastStart;
		case FollowUpOn.CastEnd:
		{
			Replica.Logging.LogKind kind = e.Kind;
			return (kind == Replica.Logging.LogKind.CastFinish || kind == Replica.Logging.LogKind.Ability) ? true : false;
		}
		case FollowUpOn.StatusGain:
			return e.Kind == Replica.Logging.LogKind.StatusGain;
		case FollowUpOn.StatusLose:
			return e.Kind == Replica.Logging.LogKind.StatusLose;
		case FollowUpOn.Headmarker:
			return e.Kind == Replica.Logging.LogKind.Headmarker;
		case FollowUpOn.Tether:
			return e.Kind == Replica.Logging.LogKind.Tether;
		case FollowUpOn.Death:
			return e.Kind == Replica.Logging.LogKind.Death;
		case FollowUpOn.Chat:
			return e.Kind == Replica.Logging.LogKind.Chat;
		default:
			return false;
		}
	}

	private bool CondMatches(FollowUpOn on, FollowCond c, LogEvent e)
	{
		if (on == FollowUpOn.Chat)
		{
			if (string.IsNullOrWhiteSpace(c.Pattern))
			{
				return true;
			}
			if (c.UseRegex)
			{
				return RegexMatch(c.Pattern, e.Name);
			}
			return e.Name.Contains(c.Pattern, StringComparison.OrdinalIgnoreCase);
		}
		if (c.Source != SourceFilter.Anyone)
		{
			ActorKind actorKind = c.Source switch
			{
				SourceFilter.Enemy => ActorKind.Enemy, 
				SourceFilter.You => ActorKind.You, 
				SourceFilter.Party => ActorKind.Party, 
				_ => ActorKind.Other, 
			};
			if (e.SourceKind != actorKind)
			{
				return false;
			}
		}
		if (!RoleMatches(c.SourceRole, e.SourceId))
		{
			return false;
		}
		if (!RoleMatches(c.TargetRole, e.TargetId))
		{
			return false;
		}
		uint entityId = Plugin.PlayerState.EntityId;
		if (c.OnlyOnSelf && on switch
		{
			FollowUpOn.Tether => (e.SourceId == entityId || e.TargetId == entityId) ? 1 : 0, 
			FollowUpOn.Death => (e.SourceId == entityId) ? 1 : 0, 
			_ => (e.TargetId == entityId) ? 1 : 0, 
		} == 0)
		{
			return false;
		}
		if (on - 4 <= FollowUpOn.Cast)
		{
			if (c.DataId != 0)
			{
				return e.DataId == c.DataId;
			}
			return true;
		}
		if (on == FollowUpOn.StatusGain)
		{
			if (StatusEventMatches(c, e))
			{
				return true;
			}
			uint actorId = (c.OnlyOnSelf ? entityId : e.TargetId);
			return ActorHasStatus(actorId, c);
		}
		if (c.MatchById && c.DataId != 0)
		{
			return e.DataId == c.DataId;
		}
		if (!string.IsNullOrWhiteSpace(c.Pattern))
		{
			return e.Name.Contains(c.Pattern, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool StatusEventMatches(FollowCond c, LogEvent e)
	{
		if (c.MatchById && c.DataId != 0 && e.DataId == c.DataId)
		{
			return true;
		}
		if (!string.IsNullOrWhiteSpace(c.Pattern))
		{
			return e.Name.Contains(c.Pattern, StringComparison.OrdinalIgnoreCase);
		}
		if (c.MatchById)
		{
			return c.DataId == 0;
		}
		return true;
	}

	private bool ActorHasStatus(uint actorId, FollowCond c)
	{
		if (actorId == 0)
		{
			return false;
		}
		if (!(Plugin.ObjectTable.SearchById(actorId) is IBattleChara battleChara))
		{
			return false;
		}
		bool flag = c.MatchById && c.DataId != 0;
		bool flag2 = !string.IsNullOrWhiteSpace(c.Pattern);
		foreach (IStatus status in battleChara.StatusList)
		{
			if (status.StatusId == 0)
			{
				continue;
			}
			if (flag && status.StatusId == c.DataId)
			{
				return true;
			}
			if (flag2)
			{
				string text = Plugin.DataManager.GetExcelSheet<Status>().GetRowOrDefault(status.StatusId)?.Name.ExtractText();
				if (text != null && text.Contains(c.Pattern, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			if (!flag && !flag2)
			{
				return true;
			}
		}
		return false;
	}

	private bool RegexMatch(string pattern, string input)
	{
		try
		{
			if (!_regexCache.TryGetValue(pattern, out Regex value))
			{
				value = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
				_regexCache[pattern] = value;
			}
			return value.IsMatch(input);
		}
		catch
		{
			return false;
		}
	}

	public void Tick()
	{
		DateTime now = DateTime.Now;
		for (int num = _pending.Count - 1; num >= 0; num--)
		{
			if (!(_pending[num].when > now))
			{
				(DateTime when, QuickDrawDef t, LogEvent e, string key) tuple = _pending[num];
				QuickDrawDef item = tuple.t;
				LogEvent item2 = tuple.e;
				string item3 = tuple.key;
				_pending.RemoveAt(num);
				Fire(item, item2, item3);
			}
		}
		for (int num2 = _pendingFollow.Count - 1; num2 >= 0; num2--)
		{
			if (!(_pendingFollow[num2].when > now))
			{
				(DateTime when, FollowUpStep s, LogEvent ctx, string key) tuple2 = _pendingFollow[num2];
				FollowUpStep item4 = tuple2.s;
				LogEvent item5 = tuple2.ctx;
				string item6 = tuple2.key;
				_pendingFollow.RemoveAt(num2);
				FireStep(item4, item5, item6);
			}
		}
		for (int num3 = _pendingShape.Count - 1; num3 >= 0; num3--)
		{
			if (!(_pendingShape[num3].when > now))
			{
				(DateTime when, string ownerId, DrawSpec d, LogEvent e) tuple3 = _pendingShape[num3];
				string item7 = tuple3.ownerId;
				DrawSpec item8 = tuple3.d;
				LogEvent item9 = tuple3.e;
				_pendingShape.RemoveAt(num3);
				SpawnShape(item7, item8, item9);
			}
		}
		for (int num4 = _armedFollow.Count - 1; num4 >= 0; num4--)
		{
			if (_armedFollow[num4].Expiry <= now)
			{
				_armedFollow.RemoveAt(num4);
			}
		}
		for (int num5 = _clearWatch.Count - 1; num5 >= 0; num5--)
		{
			if (_clearWatch[num5].expiry <= now)
			{
				_clearWatch.RemoveAt(num5);
			}
		}
		lock (_labelsLock)
		{
			_labels.RemoveAll((LiveLabel l) => l.Expiry <= now);
		}
		lock (_arrowsLock)
		{
			_arrows.RemoveAll((LiveArrow a) => a.Expiry <= now);
		}
		foreach (string item10 in _shapeAnchors.Keys.ToList())
		{
			if (_shapeAnchors[item10].Expiry <= now)
			{
				_shapeAnchors.Remove(item10);
			}
		}
		foreach (string key in _live.Keys.ToList())
		{
			PruneOwner(key);
		}
	}

	private bool Matches(QuickDrawDef t, LogEvent e)
	{
		if (!t.AnyZone && t.Zones.Count > 0 && !t.Zones.Contains(Plugin.ClientState.TerritoryType))
		{
			return false;
		}
		bool flag;
		switch (t.On)
		{
		case TriggerMatch.Any:
			flag = true;
			break;
		case TriggerMatch.Cast:
			flag = e.Kind == Replica.Logging.LogKind.CastStart;
			break;
		case TriggerMatch.CastEnd:
		{
			Replica.Logging.LogKind kind = e.Kind;
			bool flag2 = ((kind == Replica.Logging.LogKind.CastFinish || kind == Replica.Logging.LogKind.Ability) ? true : false);
			flag = flag2;
			break;
		}
		case TriggerMatch.StatusGain:
			flag = e.Kind == Replica.Logging.LogKind.StatusGain;
			break;
		case TriggerMatch.StatusLose:
			flag = e.Kind == Replica.Logging.LogKind.StatusLose;
			break;
		case TriggerMatch.Death:
			flag = e.Kind == Replica.Logging.LogKind.Death;
			break;
		case TriggerMatch.Headmarker:
			flag = e.Kind == Replica.Logging.LogKind.Headmarker;
			break;
		case TriggerMatch.Tether:
			flag = e.Kind == Replica.Logging.LogKind.Tether;
			break;
		case TriggerMatch.Chat:
			flag = e.Kind == Replica.Logging.LogKind.Chat;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag)
		{
			return false;
		}
		if (t.On == TriggerMatch.Chat)
		{
			if (string.IsNullOrEmpty(t.Pattern))
			{
				return true;
			}
			if (t.UseRegex)
			{
				return RegexMatch(t.Pattern, e.Name);
			}
			return e.Name.Contains(t.Pattern, StringComparison.OrdinalIgnoreCase);
		}
		if (t.Source != SourceFilter.Anyone)
		{
			ActorKind actorKind = t.Source switch
			{
				SourceFilter.Enemy => ActorKind.Enemy, 
				SourceFilter.You => ActorKind.You, 
				SourceFilter.Party => ActorKind.Party, 
				_ => ActorKind.Other, 
			};
			if (e.SourceKind != actorKind)
			{
				return false;
			}
		}
		if (t.OnlyOnSelf)
		{
			uint entityId = Plugin.PlayerState.EntityId;
			if (e.Kind == Replica.Logging.LogKind.Tether)
			{
				if (e.SourceId != entityId && e.TargetId != entityId)
				{
					return false;
				}
			}
			else if ((e.IsStatus || e.Kind == Replica.Logging.LogKind.Headmarker) && e.TargetId != entityId)
			{
				return false;
			}
		}
		if (!RoleMatches(t.SourceRole, e.SourceId))
		{
			return false;
		}
		if (!RoleMatches(t.TargetRole, e.TargetId))
		{
			return false;
		}
		if (!NameContains(t.SourceName, e.SourceName))
		{
			return false;
		}
		if (!NameContains(t.TargetName, e.TargetName))
		{
			return false;
		}
		if (!NumMatches(t, e))
		{
			return false;
		}
		if (!VarMatches(t, e))
		{
			return false;
		}
		if (!StatusMatches(t, e))
		{
			return false;
		}
		if (t.MatchById)
		{
			return e.DataId == t.DataId;
		}
		if (string.IsNullOrEmpty(t.Pattern))
		{
			return true;
		}
		if (t.UseRegex)
		{
			return RegexMatch(t.Pattern, e.Name);
		}
		return e.Name.Contains(t.Pattern, StringComparison.OrdinalIgnoreCase);
	}

	private string Substitute(string text, LogEvent e)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (text.Contains("{$", StringComparison.Ordinal))
		{
			text = VarTokenRx.Replace(text, (Match m) => (!_vars.TryGetValue(m.Groups[1].Value, out string value)) ? "" : value);
		}
		text = text.Replace("{name}", e.Name, StringComparison.OrdinalIgnoreCase).Replace("{source}", e.SourceName, StringComparison.OrdinalIgnoreCase).Replace("{target}", e.TargetName, StringComparison.OrdinalIgnoreCase);
		return text;
	}

	private static bool NameContains(string want, string actual)
	{
		if (!string.IsNullOrWhiteSpace(want))
		{
			if (!string.IsNullOrEmpty(actual))
			{
				return actual.Contains(want, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}
		return true;
	}

	private static bool RoleMatches(RoleFilter want, uint actorId)
	{
		if (want == RoleFilter.Any)
		{
			return true;
		}
		if (actorId == 0)
		{
			return false;
		}
		if (!(Plugin.ObjectTable.SearchById(actorId) is IBattleChara { ClassJob: { IsValid: not false }, ClassJob: { Value: var value } }))
		{
			return false;
		}
		return RoleClass(value.Role) == want;
	}

	private static RoleFilter RoleClass(byte role)
	{
		switch (role)
		{
		case 1:
			return RoleFilter.Tank;
		case 4:
			return RoleFilter.Healer;
		case 2:
		case 3:
			return RoleFilter.Dps;
		default:
			return RoleFilter.Any;
		}
	}

	private bool NumMatches(QuickDrawDef t, LogEvent e)
	{
		if (t.NumConds.Count == 0)
		{
			return true;
		}
		bool flag;
		foreach (NumCond numCond in t.NumConds)
		{
			float num = ReadField(numCond.Field, e);
			NumField field = numCond.Field;
			flag = ((field - 2 <= NumField.Value || field - 8 <= NumField.SourceHpPct) ? true : false);
			if (flag && num < 0f)
			{
				flag = false;
			}
			else
			{
				if (Compare(num, numCond.Op, numCond.Value))
				{
					continue;
				}
				flag = false;
			}
			goto IL_0092;
		}
		return true;
		IL_0092:
		return flag;
	}

	private static float ReadField(NumField f, LogEvent e)
	{
		return f switch
		{
			NumField.StackCount => e.Count, 
			NumField.Value => e.Value, 
			NumField.Param1 => e.Param1, 
			NumField.Param2 => e.Param2, 
			NumField.Param3 => e.Param3, 
			NumField.Param4 => e.Param4, 
			NumField.SourceHpPct => HpPct(e.SourceId), 
			NumField.TargetHpPct => HpPct(e.TargetId), 
			NumField.DistSourceToTarget => ActorDist(e.SourceId, e.TargetId), 
			NumField.DistMeToSource => ActorDist(Plugin.PlayerState.EntityId, e.SourceId), 
			NumField.DistMeToTarget => ActorDist(Plugin.PlayerState.EntityId, e.TargetId), 
			_ => 0f, 
		};
	}

	private static float ActorDist(uint a, uint b)
	{
		if (a == 0 || b == 0)
		{
			return -1f;
		}
		IGameObject gameObject = Actor(a);
		IGameObject gameObject2 = Actor(b);
		if (gameObject == null || gameObject2 == null)
		{
			return -1f;
		}
		return Vector3.Distance(gameObject.Position, gameObject2.Position);
	}

	private static float HpPct(uint actorId)
	{
		if (actorId == 0)
		{
			return -1f;
		}
		if (Plugin.ObjectTable.SearchById(actorId) is IBattleChara { MaxHp: not 0u } battleChara)
		{
			return (float)battleChara.CurrentHp / (float)battleChara.MaxHp * 100f;
		}
		return -1f;
	}

	private bool StatusMatches(QuickDrawDef t, LogEvent e)
	{
		if (t.StatusGates.Count == 0)
		{
			return true;
		}
		foreach (StatusGate statusGate in t.StatusGates)
		{
			if (!ActorStatusGate(statusGate.Who switch
			{
				StatusGateWho.Self => Plugin.PlayerState.EntityId, 
				StatusGateWho.Source => e.SourceId, 
				StatusGateWho.Target => e.TargetId, 
				_ => 0u, 
			}, statusGate))
			{
				return false;
			}
		}
		return true;
	}

	private static bool ActorStatusGate(uint actorId, StatusGate g)
	{
		bool flag = ActorHasNamedStatus(actorId, g.StatusId, g.Name);
		if (!g.Have)
		{
			return !flag;
		}
		return flag;
	}

	private static bool ActorHasNamedStatus(uint actorId, uint statusId, string name)
	{
		if (actorId == 0)
		{
			return false;
		}
		if (!(Plugin.ObjectTable.SearchById(actorId) is IBattleChara battleChara))
		{
			return false;
		}
		bool flag = statusId != 0;
		bool flag2 = !string.IsNullOrWhiteSpace(name);
		if (!flag && !flag2)
		{
			return false;
		}
		foreach (IStatus status in battleChara.StatusList)
		{
			if (status.StatusId == 0)
			{
				continue;
			}
			if (flag && status.StatusId == statusId)
			{
				return true;
			}
			if (flag2)
			{
				string text = Plugin.DataManager.GetExcelSheet<Status>().GetRowOrDefault(status.StatusId)?.Name.ExtractText();
				if (text != null && text.Contains(name, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static IEnumerable<DrawSpec> DependencyShapes(QuickDrawDef t, DrawSpec d)
	{
		HashSet<string> seen = new HashSet<string>();
		if (d.Anchor == DrawAnchor.LinkedShape && !string.IsNullOrEmpty(d.AnchorShapeId))
		{
			DrawSpec drawSpec = FindShape(t, d.AnchorShapeId);
			if (drawSpec != null && seen.Add(drawSpec.Id))
			{
				yield return drawSpec;
			}
		}
		QuickShape shape = d.Shape;
		bool flag = ((shape == QuickShape.Line || shape - 9 <= QuickShape.Donut) ? true : false);
		if ((flag || (d.Shape == QuickShape.Rectangle && d.SpanToTarget)) && d.Link == LinkTarget.LinkedShape && !string.IsNullOrEmpty(d.LinkShapeId))
		{
			DrawSpec drawSpec2 = FindShape(t, d.LinkShapeId);
			if (drawSpec2 != null && seen.Add(drawSpec2.Id))
			{
				yield return drawSpec2;
			}
		}
	}

	private static DrawSpec? FindShape(QuickDrawDef t, string id)
	{
		if (t.Draw.Id == id)
		{
			return t.Draw;
		}
		foreach (DrawSpec extraShape in t.ExtraShapes)
		{
			if (extraShape.Id == id)
			{
				return extraShape;
			}
		}
		foreach (FollowUpStep followUp in t.FollowUps)
		{
			if (followUp.Draw.Id == id)
			{
				return followUp.Draw;
			}
			foreach (DrawSpec extraShape2 in followUp.ExtraShapes)
			{
				if (extraShape2.Id == id)
				{
					return extraShape2;
				}
			}
		}
		return null;
	}

	private Vector3? ResolveLinkedShapePos(string shapeId)
	{
		if (string.IsNullOrEmpty(shapeId))
		{
			return null;
		}
		if (!_shapeAnchors.TryGetValue(shapeId, out ShapeAnchor value))
		{
			return null;
		}
		Vector3? result = value.Pos();
		if (result.HasValue)
		{
			value.Last = result.Value;
			return result;
		}
		if (!(value.Last != default(Vector3)))
		{
			return null;
		}
		return value.Last;
	}

	private IGameObject? ResolveLinkedShapeOwner(string shapeId)
	{
		if (string.IsNullOrEmpty(shapeId))
		{
			return null;
		}
		if (!_shapeAnchors.TryGetValue(shapeId, out ShapeAnchor value))
		{
			return null;
		}
		return value.Owner;
	}

	private void RegisterShapeAnchor(string id, StaticVfx vfx, float life)
	{
		if (string.IsNullOrEmpty(id))
		{
			return;
		}
		_shapeAnchors[id] = new ShapeAnchor
		{
			Expiry = DateTime.Now.AddSeconds(life),
			Owner = vfx.Owner,
			Pos = delegate
			{
				if (vfx.LastPosition != default(Vector3))
				{
					return vfx.LastPosition;
				}
				return (vfx.Position != default(Vector3)) ? new Vector3?(vfx.Position) : ((Vector3?)null);
			}
		};
	}

	private void UnregisterShapeAnchor(string id)
	{
		if (!string.IsNullOrEmpty(id))
		{
			_shapeAnchors.Remove(id);
		}
	}

	private static IGameObject? NearestByBaseId(uint baseId, LogEvent e)
	{
		if (baseId == 0)
		{
			return null;
		}
		Vector3 value = new Vector3(e.X, 0f, e.Y);
		if (value.X == 0f && value.Z == 0f)
		{
			value = Actor(e.SourceId)?.Position ?? new Vector3(100f, 0f, 100f);
		}
		IGameObject result = null;
		float num = float.MaxValue;
		foreach (IGameObject item in Plugin.ObjectTable)
		{
			if (item.BaseId == baseId)
			{
				float num2 = Vector3.DistanceSquared(item.Position, value);
				if (num2 < num)
				{
					num = num2;
					result = item;
				}
			}
		}
		return result;
	}

	private static bool Compare(float a, NumOp op, float b)
	{
		return op switch
		{
			NumOp.Eq => Math.Abs(a - b) < 0.0001f, 
			NumOp.Ne => Math.Abs(a - b) >= 0.0001f, 
			NumOp.Lt => a < b, 
			NumOp.Le => a <= b, 
			NumOp.Gt => a > b, 
			NumOp.Ge => a >= b, 
			_ => true, 
		};
	}

	private bool VarMatches(QuickDrawDef t, LogEvent e)
	{
		if (t.VarConds.Count == 0)
		{
			return true;
		}
		foreach (VarCond varCond in t.VarConds)
		{
			if (!string.IsNullOrWhiteSpace(varCond.Name))
			{
				_vars.TryGetValue(varCond.Name, out string value);
				if (value == null)
				{
					value = "";
				}
				string text = Substitute(varCond.Value, e);
				bool flag;
				if (varCond.Numeric)
				{
					flag = Compare(float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0f, b: float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2) ? result2 : 0f, op: varCond.Op);
				}
				else
				{
					int num = string.Compare(value, text, StringComparison.OrdinalIgnoreCase);
					flag = varCond.Op switch
					{
						NumOp.Eq => num == 0, 
						NumOp.Ne => num != 0, 
						NumOp.Lt => num < 0, 
						NumOp.Le => num <= 0, 
						NumOp.Gt => num > 0, 
						NumOp.Ge => num >= 0, 
						_ => true, 
					};
				}
				if (!flag)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void ApplyVars(QuickDrawDef t, LogEvent e)
	{
		if (t.SetVars.Count == 0)
		{
			return;
		}
		foreach (VarAction setVar in t.SetVars)
		{
			if (!string.IsNullOrWhiteSpace(setVar.Name))
			{
				string text = Substitute(setVar.Value, e);
				if (setVar.Op == VarOp.Increment)
				{
					_vars.TryGetValue(setVar.Name, out string value);
					float num = (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0f);
					float num2 = (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2) ? result2 : 1f);
					_vars[setVar.Name] = (num + num2).ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					_vars[setVar.Name] = text;
				}
			}
		}
	}

	private static Concurrency ModeOf(QuickDrawDef t)
	{
		if (!t.NoReentry || t.Concurrency != Concurrency.Stack)
		{
			return t.Concurrency;
		}
		return Concurrency.Wait;
	}

	private void ArmClear(QuickDrawDef t, string key, uint subject)
	{
		if (t.ClearOn.Enabled)
		{
			_clearWatch.Add((DateTime.Now.AddSeconds(Math.Max(0.5f, t.ClearOn.Seconds)), t, key, subject));
		}
	}

	private void ProcessClearWatch(LogEvent e)
	{
		if (_clearWatch.Count == 0)
		{
			return;
		}
		DateTime now = DateTime.Now;
		for (int num = _clearWatch.Count - 1; num >= 0; num--)
		{
			var (dateTime, quickDrawDef, id, num2) = _clearWatch[num];
			if (now > dateTime)
			{
				_clearWatch.RemoveAt(num);
			}
			else if (ClearMatches(quickDrawDef.ClearOn, e) && (num2 == 0 || EventSubject(e) == num2))
			{
				ClearOwner(id);
				_clearWatch.RemoveAt(num);
			}
		}
	}

	private static bool ClearMatches(ClearRule r, LogEvent e)
	{
		if (!KindMatches(r.On, e))
		{
			return false;
		}
		if (r.OnlyOnSelf)
		{
			uint entityId = Plugin.PlayerState.EntityId;
			if (r.On switch
			{
				FollowUpOn.Tether => (e.SourceId == entityId || e.TargetId == entityId) ? 1 : 0, 
				FollowUpOn.Death => (e.SourceId == entityId) ? 1 : 0, 
				_ => (e.TargetId == entityId) ? 1 : 0, 
			} == 0)
			{
				return false;
			}
		}
		if (r.MatchById && r.DataId != 0)
		{
			return e.DataId == r.DataId;
		}
		if (!string.IsNullOrWhiteSpace(r.Pattern))
		{
			return e.Name.Contains(r.Pattern, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}
}
