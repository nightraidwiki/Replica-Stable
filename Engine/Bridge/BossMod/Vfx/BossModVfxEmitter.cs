using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.Util;
using Replica.Engine.Vfx;
using static Replica.Engine.Bridge.BossMod.Reflection.BossModReflection;

namespace Replica.Engine.Bridge.BossMod.Vfx;

[SkipLocalsInit]
public sealed class BossModVfxEmitter
{
    private readonly Dictionary<MirrorKey, StaticVfx> _active = new(128);
    private readonly HashSet<MirrorKey> _frame = new(128);
    private readonly List<MirrorKey> _stale = new(32);

    private readonly Dictionary<MirrorKey, (long LastTrigger, List<ActorVfx> Instances)> _activeLockOns = new(32);
    private readonly HashSet<MirrorKey> _frameLockOns = new(32);
    private readonly List<MirrorKey> _staleLockOns = new(16);

    public int ActiveCount => _active.Count;

    public void BeginFrame()
    {
        _frame.Clear();
        _frameLockOns.Clear();
    }

    public void EndFrame()
    {
        _stale.Clear();
        foreach (var kv in _active)
        {
            if (!_frame.Contains(kv.Key))
            {
                _stale.Add(kv.Key);
            }
        }
        foreach (var k in _stale)
        {
            if (_active.Remove(k, out var v))
            {
                v?.Remove();
            }
        }

        _staleLockOns.Clear();
        foreach (var k in _activeLockOns.Keys)
        {
            if (!_frameLockOns.Contains(k))
            {
                _staleLockOns.Add(k);
            }
        }
        foreach (var k in _staleLockOns)
        {
            if (_activeLockOns.Remove(k, out var entry))
            {
                foreach (var inst in entry.Instances)
                {
                    try { inst?.Remove(); } catch { }
                }
            }
        }
    }

    public void Emit(
        int kind, ulong actorHash,
        float ox, float oz, float rot,
        float p1, float p2, float p3,
        DrawElement elem, bool actorTracked,
        Func<Angle>? rotAction)
    {
        // When actor is tracked, ox/oz are 0 in key to avoid churning on small movements.
        // If rotAction is present, rot is also 0 in key to avoid churning while turning.
        // If p2 or p3 are 0 (dynamic distance/offset), key remains completely stable.
        var key = actorTracked
            ? new MirrorKey(kind, actorHash, 0, 0, rotAction != null ? 0 : R2(rot), R1(p1), R1(p2), R1(p3))
            : new MirrorKey(kind, actorHash, R1(ox), R1(oz), R2(rot), R1(p1), R1(p2), R1(p3));

        _frame.Add(key);
        if (_active.ContainsKey(key)) return;

        var vfx = DrawManager.Draw(elem);
        if (vfx != null)
        {
            _active[key] = vfx;
        }
    }

    public void EmitLockOn(int kind, ulong actorHash, IGameObject target, long loopIntervalMs = 4400L)
    {
        var key = new MirrorKey(kind, actorHash, 0, 0, 0, 0, 0, 0);
        _frameLockOns.Add(key);

        long now = Environment.TickCount64;
        if (_activeLockOns.TryGetValue(key, out var entry))
        {
            if (now - entry.LastTrigger >= loopIntervalMs)
            {
                var vfx = new ActorVfx("com_share_4_5s_c0c".LockOn(), target, target);
                entry.Instances.Add(vfx);
                _activeLockOns[key] = (now, entry.Instances);
            }
        }
        else
        {
            var list = new List<ActorVfx> { new ActorVfx("com_share_4_5s_c0c".LockOn(), target, target) };
            _activeLockOns[key] = (now, list);
        }
    }

    public void ClearAll()
    {
        foreach (var v in _active.Values)
        {
            try { v?.Remove(); } catch { }
        }
        _active.Clear();
        _frame.Clear();
        _stale.Clear();

        foreach (var entry in _activeLockOns.Values)
        {
            foreach (var inst in entry.Instances)
            {
                try { inst?.Remove(); } catch { }
            }
        }
        _activeLockOns.Clear();
        _frameLockOns.Clear();
        _staleLockOns.Clear();
    }
}
