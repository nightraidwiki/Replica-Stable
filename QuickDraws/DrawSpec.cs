using System;
using System.Numerics;

namespace Replica.QuickDraws;

public sealed class DrawSpec
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");

	public string AnchorShapeId { get; set; } = "";

	public string LinkShapeId { get; set; } = "";

	public uint AnchorActorBaseId { get; set; }

	public QuickShape Shape { get; set; }

	public Vector4 Color { get; set; } = new Vector4(1f, 0.55f, 0.1f, 1f);

	public float Radius { get; set; } = 6f;

	public float InnerRadius { get; set; } = 8f;

	public float HalfWidth { get; set; } = 4f;

	public float Length { get; set; } = 20f;

	public int FanAngle { get; set; } = 90;

	public float Rotation { get; set; }

	public float ChevronSpacing { get; set; } = 2f;

	public float LineThickness { get; set; } = 4f;

	public bool OrientToFacing { get; set; }

	public float OffsetForward { get; set; }

	public float OffsetSide { get; set; }

	public DrawAnchor Anchor { get; set; }

	public bool AttachToActor { get; set; } = true;

	public Vector3 FixedPosition { get; set; } = new Vector3(100f, 0f, 100f);

	public LinkTarget Link { get; set; }

	public Vector3 LinkPosition { get; set; } = new Vector3(100f, 0f, 100f);

	public bool SpanToTarget { get; set; }

	public uint TetherFilterId { get; set; }

	public VfxStyle Style { get; set; }

	public string CustomVfx { get; set; } = "";

	public float Duration { get; set; } = 5f;

	public bool UseEventDuration { get; set; }

	public int Repeat { get; set; } = 1;

	public float RepeatStep { get; set; } = 45f;

	public float StartDelay { get; set; }

	public string Label { get; set; } = "";

	public Vector4 LabelColor { get; set; } = new Vector4(1f, 1f, 1f, 1f);

	public float LabelSize { get; set; } = 1f;

	public float LabelHeight { get; set; } = 2f;

	public void NormalizeLegacy()
	{
		if (Style == VfxStyle.Plain)
		{
			return;
		}
		switch (Style)
		{
		case VfxStyle.Knockback:
			Shape = QuickShape.Knockback;
			break;
		case VfxStyle.Laser:
			Shape = QuickShape.Laser;
			break;
		case VfxStyle.Theater:
			Shape = QuickShape.Rectangle;
			break;
		case VfxStyle.Triangle:
			Shape = QuickShape.Fan;
			if (FanAngle < 30)
			{
				FanAngle = 60;
			}
			break;
		}
		Style = VfxStyle.Plain;
	}

	public void EnsureId()
	{
		if (string.IsNullOrEmpty(Id))
		{
			Id = Guid.NewGuid().ToString("N");
		}
	}

	public DrawSpec Clone()
	{
		return (DrawSpec)MemberwiseClone();
	}
}
