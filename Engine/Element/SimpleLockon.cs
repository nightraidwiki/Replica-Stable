using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Enum;
using Replica.Engine.Managers;

namespace Replica.Engine.Element;

public static class SimpleLockon
{
	public static void EyeWarn(IGameObject gameObject, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.Omen,
			drawAvfx = "eye_warn",
			delayDrawTime = delay,
			drawOnObject = true,
			radiusX = 1f,
			radiusZ = 1f,
			radiusY = 2f
		}, gameObject);
	}

	public static void ShareLockon(IGameObject? gameObject, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "com_share_4_5s_c0c",
			delayDrawTime = delay
		}, gameObject);
	}

	public static void ShareLockon2(IGameObject gameObject, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "m0618trg_a0k1",
			delayDrawTime = delay
		}, gameObject);
	}

	public static void ShareRect4s(IGameObject? gameObject, IGameObject? castObject = null, float delay = 0f)
	{
		DrawShareLaser("share_laser_4sec_0t", gameObject, castObject, delay);
	}

	public static void ShareRect5s(IGameObject? gameObject, IGameObject? castObject = null, float delay = 0f)
	{
		DrawShareLaser("share_laser_5sec_0t", gameObject, castObject, delay);
	}

	public static void ShareRect8s(IGameObject? gameObject, IGameObject? castObject = null, float delay = 0f)
	{
		DrawShareLaser("share_laser_8sec_0t", gameObject, castObject, delay);
	}

	private static void DrawShareLaser(string vfx, IGameObject? target, IGameObject? caster, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = vfx,
			delayDrawTime = delay
		}, target, caster);
	}

	public static void Share6S(IGameObject gameObject, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "com_share3_6s0p",
			delayDrawTime = delay
		}, gameObject);
	}

	public static void Share5S(IGameObject gameObject, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "com_share_5_5s_c0c",
			delayDrawTime = delay
		}, gameObject);
	}

	public static void ShareLockon2_6m(IGameObject gameObject, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.Omen,
			drawAvfx = "share2_6m",
			delayDrawTime = delay,
			drawOnObject = true,
			radiusX = 1f,
			radiusZ = 1f,
			radiusY = 2f
		}, gameObject);
	}

	public static void TarLockOn5m5s(IGameObject gameObject, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "loc05sp_05af",
			delayDrawTime = delay
		}, gameObject);
	}

	public static void TarLockOn6m5s(IGameObject? gameObject, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "loc06sp_05ak1",
			delayDrawTime = delay
		}, gameObject);
	}

	public static void TarLockOn14m8s(IGameObject gameObject, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "tag_ae14m_8s_r1",
			delayDrawTime = delay
		}, gameObject);
	}

	public static void TarLockOn5m8s(IGameObject gameObject, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "tag_ae5m_8s_0v",
			delayDrawTime = delay
		}, gameObject);
	}

	public static void TarLockOn8m5s(IGameObject gameObject, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "loc08sp_05at",
			delayDrawTime = delay
		}, gameObject);
	}

	public static void Dice1_5s(IGameObject gameObject)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "sph_lockon2_num01_s5p"
		}, gameObject);
	}

	public static void Dice2_5s(IGameObject gameObject)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "sph_lockon2_num02_s5p"
		}, gameObject);
	}

	public static void Dice3_5s(IGameObject gameObject)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "sph_lockon2_num03_s5p"
		}, gameObject);
	}

	public static void Dice4_5s(IGameObject gameObject)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "sph_lockon2_num04_s5p"
		}, gameObject);
	}

	public static void TankLockOn_4m_5s(IGameObject gameObject)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "tank_lockonae_4m_5s_01t",
			drawType = ElementType.LockOn
		}, gameObject);
	}

	public static void TankShare(IGameObject gameObject, float delay = 0f)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "m0676trg_tw_d0t1p",
			drawType = ElementType.LockOn,
			delayDrawTime = delay
		}, gameObject);
	}

	public static void lockon_4m_8s(IGameObject gameObject)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "tag_ae4m_8s_0v",
			drawType = ElementType.LockOn
		}, gameObject);
	}

	public static void DrawLockon(IGameObject gameObject, string vfx)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = vfx,
			drawType = ElementType.LockOn
		}, gameObject);
	}
}
