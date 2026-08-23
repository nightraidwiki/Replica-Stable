using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP.P5;

public class P2Tower : ISpecialAction
{
	private Dictionary<string, Vector2> P5TowerDirs = new Dictionary<string, Vector2>
	{
		{
			"N",
			new Vector2(100f, 100f)
		},
		{
			"S",
			new Vector2(100f, 100f)
		},
		{
			"E",
			new Vector2(100f, 100f)
		},
		{
			"W",
			new Vector2(100f, 100f)
		},
		{
			"NE",
			new Vector2(100f, 100f)
		},
		{
			"NW",
			new Vector2(100f, 100f)
		},
		{
			"SE",
			new Vector2(100f, 100f)
		},
		{
			"SW",
			new Vector2(100f, 100f)
		}
	};

	public override string Name => "P2 (tower)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 32788u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 32788)
		{
			base.CanDraw = true;
			P5TowerDirs = new Dictionary<string, Vector2>
			{
				{
					"N",
					new Vector2(100f, 100f)
				},
				{
					"S",
					new Vector2(100f, 100f)
				},
				{
					"E",
					new Vector2(100f, 100f)
				},
				{
					"W",
					new Vector2(100f, 100f)
				},
				{
					"NE",
					new Vector2(100f, 100f)
				},
				{
					"NW",
					new Vector2(100f, 100f)
				},
				{
					"SE",
					new Vector2(100f, 100f)
				},
				{
					"SW",
					new Vector2(100f, 100f)
				}
			};
			Plugin.DebugChat("P5 P2 guide init");
		}
	}

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId - 2013245 > 1 || !base.CanDraw)
		{
			return;
		}
		base.CanDraw = false;
		new TimeHelper(100L, delegate
		{
			HeaderMarkerEnum headerMarkerEnum = Svc.Objects.LocalPlayer?.GameObjectId.Mark() ?? HeaderMarkerEnum.None;
			IEnumerable<IGameObject> enumerable = Svc.Objects.Where((IGameObject o) => o.BaseId - 2013245 <= 1);
			IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 15720);
			float num = 0f;
			float num2 = 0f;
			foreach (IGameObject item in enumerable)
			{
				num += item.Position.X;
				num2 += item.Position.Z;
				Vector2 vector = new Vector2(item.Position.X, item.Position.Z);
				double num3 = VectorAngle(new Vector2(gameObject.Position.X, gameObject.Position.Z), vector);
				if (-5.0 < num3 && num3 < 5.0)
				{
					P5TowerDirs["N"] = vector;
				}
				if (num3 > 175.0 || num3 < -175.0)
				{
					P5TowerDirs["S"] = vector;
				}
				if (60.0 < num3 && num3 < 120.0)
				{
					P5TowerDirs["E"] = vector;
				}
				if (-60.0 > num3 && num3 > -120.0)
				{
					P5TowerDirs["W"] = vector;
				}
				if (15.0 < num3 && num3 < 50.0)
				{
					P5TowerDirs["NE"] = vector;
				}
				if (-15.0 > num3 && num3 > -50.0)
				{
					P5TowerDirs["NW"] = vector;
				}
				if (133.0 < num3 && num3 < 165.0)
				{
					P5TowerDirs["SE"] = vector;
				}
				if (-133.0 > num3 && num3 > -165.0)
				{
					P5TowerDirs["SW"] = vector;
				}
			}
			string text = ((Vector2.Distance(new Vector2(num / (float)enumerable.Count(), num2 / (float)enumerable.Count()), new Vector2(gameObject.Position.X, gameObject.Position.Z)) < 20f) ? "Reversed" : "Normal");
			Vector2 vector2 = new Vector2(100f, 100f);
			if (enumerable.Count() == 5)
			{
				if (text == "Normal")
				{
					switch (headerMarkerEnum)
					{
					case HeaderMarkerEnum.Attack1:
					case HeaderMarkerEnum.Chain1:
						vector2 = P5TowerDirs["N"];
						break;
					case HeaderMarkerEnum.Attack3:
					case HeaderMarkerEnum.Attack4:
						vector2 = P5TowerDirs["SW"];
						break;
					case HeaderMarkerEnum.Chain3:
					case HeaderMarkerEnum.Circle:
						vector2 = P5TowerDirs["SE"];
						break;
					case HeaderMarkerEnum.Attack2:
						vector2 = P5TowerDirs["W"];
						break;
					case HeaderMarkerEnum.Chain2:
						vector2 = P5TowerDirs["E"];
						break;
					}
				}
				else
				{
					switch (headerMarkerEnum)
					{
					case HeaderMarkerEnum.Attack4:
					case HeaderMarkerEnum.Circle:
						vector2 = P5TowerDirs["S"];
						break;
					case HeaderMarkerEnum.Attack1:
					case HeaderMarkerEnum.Attack2:
						vector2 = P5TowerDirs["NW"];
						break;
					case HeaderMarkerEnum.Chain1:
					case HeaderMarkerEnum.Chain2:
						vector2 = P5TowerDirs["NE"];
						break;
					case HeaderMarkerEnum.Attack3:
						vector2 = P5TowerDirs["W"];
						break;
					case HeaderMarkerEnum.Chain3:
						vector2 = P5TowerDirs["E"];
						break;
					}
				}
			}
			else if (text == "Normal")
			{
				switch (headerMarkerEnum)
				{
				case HeaderMarkerEnum.Chain1:
					vector2 = P5TowerDirs["NW"];
					break;
				case HeaderMarkerEnum.Attack1:
					vector2 = P5TowerDirs["NE"];
					break;
				case HeaderMarkerEnum.Attack3:
					vector2 = P5TowerDirs["SW"];
					break;
				case HeaderMarkerEnum.Chain3:
					vector2 = P5TowerDirs["SE"];
					break;
				case HeaderMarkerEnum.Attack2:
				case HeaderMarkerEnum.Attack4:
					vector2 = P5TowerDirs["W"];
					break;
				case HeaderMarkerEnum.Chain2:
				case HeaderMarkerEnum.Circle:
					vector2 = P5TowerDirs["E"];
					break;
				}
			}
			else
			{
				switch (headerMarkerEnum)
				{
				case HeaderMarkerEnum.Attack2:
					vector2 = P5TowerDirs["NW"];
					break;
				case HeaderMarkerEnum.Chain2:
					vector2 = P5TowerDirs["NE"];
					break;
				case HeaderMarkerEnum.Circle:
					vector2 = P5TowerDirs["SW"];
					break;
				case HeaderMarkerEnum.Attack4:
					vector2 = P5TowerDirs["SE"];
					break;
				case HeaderMarkerEnum.Attack1:
				case HeaderMarkerEnum.Attack3:
					vector2 = P5TowerDirs["W"];
					break;
				case HeaderMarkerEnum.Chain1:
				case HeaderMarkerEnum.Chain3:
					vector2 = P5TowerDirs["E"];
					break;
				}
			}
			if (vector2.X != 100f || vector2.Y != 100f)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "share_trap01k1",
					Position = new Vector3(vector2.X, 0f, vector2.Y),
					drawOnObject = false,
					radiusX = 3f,
					radiusY = 5f,
					radiusZ = 3f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 31492u }
					}
				}, gameObject);
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "e5d1_b1_kblaser_t1",
					radiusX = 1.5f,
					drawOnObject = true,
					targetPosition = new Vector3(vector2.X, 0f, vector2.Y),
					endToTarget = true,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 31492u }
					}
				}, Svc.Objects.LocalPlayer);
				Plugin.DebugChat("P5 P2 tower guide");
			}
		});
	}

	private static double VectorAngle(Vector2 mainVector, Vector2 minorVector, int type = 0)
	{
		float num;
		float num2;
		float num3;
		float num4;
		if (type != 1)
		{
			num = mainVector.X - 100f;
			num2 = 0f - (mainVector.Y - 100f);
			num3 = minorVector.X - 100f;
			num4 = 0f - (minorVector.Y - 100f);
		}
		else
		{
			num = mainVector.X;
			num2 = mainVector.Y;
			num3 = minorVector.X;
			num4 = minorVector.Y;
		}
		float num5 = num * num3 + num2 * num4;
		float num6 = MathF.Sqrt(num * num + num2 * num2);
		float num7 = MathF.Sqrt(num3 * num3 + num4 * num4);
		double num8 = Math.Acos(Math.Clamp(num5 / (num6 * num7), -1.0, 1.0));
		double num9 = ((!(num * num4 - num3 * num2 > 0f)) ? 1 : (-1));
		return num8 * num9 * (180.0 / Math.PI);
	}
}
