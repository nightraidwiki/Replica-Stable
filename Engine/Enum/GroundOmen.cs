using System.Numerics;
using System.Runtime.InteropServices;

namespace Replica.Engine.Enum;

public class GroundOmen
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Friendly
	{
		public const string BasicCircle = "general_1bpf";

		public const string Circle = "general_1bpxf";

		public const string Rectangle = "general02pxf";

		public const string Rectangle2 = "general_x02pf";

		public const string Fan020 = "gl_fan020_0pt";

		public const string Fan030 = "gl_fan030_1bpf";

		public const string Fan045 = "gl_fan045_1bpxf";

		public const string Fan060 = "gl_fan060_1bpf";

		public const string BasicFan090 = "gl_fan090_1bpf";

		public const string Fan090 = "gl_fan090_1bpxf";

		public const string Fan120 = "gl_fan120_1bpxf";

		public const string Fan150 = "gl_fan150_1bpf";
	}

	public const string customFan = "customFan";

	public const string customCircle = "customCircle";

	public const string customDonut = "customDonut";

	public const string customRect = "customRect";

	public const string customRect2 = "customRect2";

	public const string Circle = "general_1bxf";

	public const string CircleEr = "er_general_1f";

	public const string CircleRed = "general_1bzt";

	public const string CircleFull = "m0347_sircle_01m1";

	public const string Rectangle = "general02xf";

	public const string RectangleFull = "general02wf";

	public const string RectangleEr = "er_general02f";

	public const string Rectangle2 = "general_x02f";

	public const string Fan015 = "gl_fan015_0x";

	public const string Fan020 = "gl_fan020_0f";

	public const string Fan030 = "gl_fan030_1bf";

	public const string Fan045 = "gl_fan045_1bf";

	public const string Fan060 = "gl_fan060_1bf";

	public const string Fan060Full = "m0611_fan_60x";

	public const string Fan060Er = "er_gl_fan060_1bf";

	public const string Fan080 = "gl_fan80_o0g";

	public const string Fan090 = "gl_fan090_1bf";

	public const string Fan090Er = "er_gl_fan090_1bf";

	public const string Fan100 = "er_gl_fan100_o0v";

	public const string Fan120 = "gl_fan120_1bf";

	public const string Fan120Er = "er_gl_fan120_1bf";

	public const string Fan130 = "gl_fan130_0x";

	public const string Fan135 = "gl_fan135_c0g";

	public const string Fan145 = "m0501_fan145_d1";

	public const string Fan150 = "gl_fan150_1bf";

	public const string Fan150Er = "er_gl_fan150_1bt";

	public const string Fan180 = "gl_fan180_1bf";

	public const string Fan180Er = "er_gl_fan180_1bf";

	public const string Fan210 = "gl_fan210_1bf";

	public const string Fan240 = "x6d3_b1_fan240_p1";

	public const string Fan270 = "gl_fan270_0100af";

	public const string Fan270Er = "er_gl_fan270_1bf";

	public const string Sircle06 = "gl_sircle_5003bf";

	public const string Sircle08 = "gl_sircle_7006x";

	public const string Sircle10 = "gl_sircle_4004bp1";

	public const string Sircle11 = "gl_sircle_1034bf";

	public const string Sircle13 = "gl_sircle_4005bf";

	public const string Sircle15 = "gl_circle4006_o0g";

	public const string Sircle15Er = "er_sircle_4006_o0d1";

	public const string Sircle17 = "gl_sircle_6010ax";

	public const string Sircle18 = "gl_sircle_1703x";

	public const string Sircle20 = "gl_sircle_4008ah1";

	public const string Sircle21 = "gl_sircle_7015k1";

	public const string Sircle23 = "gl_sircle_3007bx";

	public const string Sircle25 = "gl_sicle_4010r1";

	public const string Sircle25Er = "er_sicle_4010_n1";

	public const string Sircle27 = "gl_sircle_3008bf";

	public const string Sircle28 = "gl_sircle_1805r1";

	public const string Sircle30Er = "er_sicle_2006_n1";

	public const string Sircle33 = "gl_sircle_1505bt1";

	public const string Sircle38 = "gl_sircle_0803c";

	public const string Sircle40 = "gl_sircle_2008bi";

	public const string Sircle40Er = "er_sicle_1004r1";

	public const string Sircle42Er = "er_sicle_1205f";

	public const string Sircle47 = "gl_sircle_3014bf";

	public const string Sircle50 = "gl_sircle_3015ac";

	public const string Sircle50Er = "er_sicle_2010t";

	public const string Sircle55 = "gl_sircle_2011v";

	public const string Sircle57 = "gl_sircle_3520x";

	public const string Sircle61Er = "er_sicle_1811t";

	public const string Sircle67 = "gl_sircle_3020bf";

	public const string Sircle67Er = "er_sicle_3020t";

	public const string Sircle75 = "gl_sircle_2015bx";

	public const string Sircle82 = "gl_sircle_1109w";

	public const string Sircle86 = "gl_sircle_1412w";

	public const string Sircle88 = "gl_sircle_1715w";

	public const string Sircle90 = "gl_sircle_2018w";

	public const string Fan060_20 = "gl_fan_o60h30m6p";

	public const string Fan180_25 = "gl_fan180_4010c";

	public const string Fan060_38 = "gl_fan_o60h16m6p";

	public const string Fan060_61 = "gl_fan_o60h23m14p";

	public const string Fan060_70 = "gl_fan_o60h30m21p";

	public const string Fan270_0100 = "gl_fan270_0100af";

	public const string Fan270_42 = "gl_fan270_1908at";

	public const string Fan270_50 = "gl_fan270_1005af";

	public const string Fan270_67 = "gl_fan270_1510af";

	public const string SingleTower = "m0119_trap_02t";

	public const string SingleTowerShare = "share_trap01k1";

	public const string WhiteTower = "co_trap00h1";

	public const string KnockBack = "m0501_nockback_omen01d1";

	public const string ArrowRect = "e5d1_b1_kblaser_t1";

	public const string Triangle30 = "x6d3_b2_triangle30_p1";

	public const string Triangle60 = "x6d3_b2_triangle60_p1";

	public const string Triangle90 = "x6d3_b2_triangle90_p1";

	public const string ShareLazerGround5s = "ShareLazerGround5s";

	public const string SingleTowerSilent = "tower_noc";

	public const string KnockBackSilent = "knockback_noc";

	public const string ArrowRectSilent = "laser_noc";

	public static Vector4 enemyColor = new Vector4(1f, 0.549f, 0.3137f, Plugin.Config.CustomAlpha);

	public static Vector4 friendColor = new Vector4(0.7f, 0.9f, 1f, Plugin.Config.CustomAlpha);

	public static Vector4 Red = new Vector4(1f, 0f, 0f, Plugin.Config.CustomAlpha);

	public static Vector4 Yellow = new Vector4(1f, 1f, 0f, Plugin.Config.CustomAlpha);

	public static Vector4 Blue = new Vector4(0f, 0f, 1f, Plugin.Config.CustomAlpha);
}
