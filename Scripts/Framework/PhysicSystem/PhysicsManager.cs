/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Description
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public sealed class PhysicsManager : SingletonBehaviour<PhysicsManager>
{
    #region Static functions

    public const int MAX_LAYER_COUNT = 31;

    public static void Update(int diff)
    {
        instance._Update(diff);
    }

    public static List<T> GetColliders<T>(int mask, bool includeInactive = false, List<T> list = null) where T : Collider_
    {
        return instance._GetColliders(mask, includeInactive, list);
    }

    public static void Foreach<T>(System.Predicate<T> call, int mask = 0, bool includeInactive = false) where T : Collider_ { instance._Foreach(call, mask, includeInactive); }

    public static bool Cross(Box2D box, Box2D other, Vector2_ tp, ref Vector2_ hit)
    {
        if (box.Intersect(other)) return true;

        var o = box.center_;

        tp -= o;
        other.Set(other.center_ - o, other.size_ + box.size_);
        box.SetCenter(Vector2_.zero);

        double minx = Mathd.Min(0, tp.x), maxx = Mathd.Max(0, tp.x), miny = Mathd.Min(0, tp.y), maxy = Mathd.Max(0, tp.y);

        var l1 = Mathd.Abs(other.leftEdge) < Mathd.Abs(other.rightEdge) ? other.leftEdge : other.rightEdge;
        var l2 = Mathd.Abs(other.topEdge) < Mathd.Abs(other.bottomEdge) ? other.topEdge : other.bottomEdge;

        bool b1 = false, b2 = false;

        if (other.leftEdge < 0 && other.rightEdge > 0) goto _l2;

 _l1:
        b1 = true;
        if (l1 > minx && l1 < maxx)
        {
            var yy = tp.y / tp.x * l1;
            if (yy > other.bottomEdge && yy < other.topEdge)
            {
                hit.Set(l1, yy);
                hit += o;

                return true;
            }
        }

        if (b2) return false;

_l2:
        b2 = true;
        if (l2 > miny && l2 < maxy)
        {
            var xx = l2 * tp.x / tp.y;
            if (xx > other.leftEdge && xx < other.rightEdge)
            {
                hit.Set(xx, l2);
                hit += o;

                return true;
            }
        }

        if (!b1) goto _l1;

        return false;
    }

    public static bool Cross(Box2D box, Sphere2D other, Vector2_ tp, ref Vector2_ hit)
    {
        if (box.Intersect(other)) return true;

        var o = other.center_;

        tp -= o;
        box.SetCenter(box.center_ - o);
        other.SetCenter(Vector2_.zero);

        var dir = tp - box.center_;

        var rr = other.radius * other.radius * dir.sqrMagnitude;
        var t  = box.topLeft + dir;
        var d  = box.leftEdge * t.y - t.x * box.topEdge;
        var dd = d * d;

        if (rr - dd > 0)
        {
            var proj = Vector2_.Project(-box.topLeft, dir.normalized);
            if (proj.magnitude <= dir.magnitude && Vector2_.Dot(proj, dir) > 0)
            {
                hit = Vector2_.Project(-box.center_, dir.normalized) + box.center_ + o;
                return true;
            }
        }

        t  = box.bottomRight + dir;
        d  = box.rightEdge * t.y - t.x * box.bottomEdge;
        dd = d * d;

        if (rr - dd > 0)
        {
            var proj = Vector2_.Project(-box.bottomRight, dir.normalized);
            if (proj.magnitude <= dir.magnitude && Vector2_.Dot(proj, dir) > 0)
            {
                hit = Vector2_.Project(-box.center_, dir.normalized) + box.center_ + o;
                return true;
            }
        }

        t  = box.topRight + dir;
        d  = box.rightEdge * t.y - t.x * box.topEdge;
        dd = d * d;

        if (rr - dd > 0)
        {
            var proj = Vector2_.Project(-box.topRight, dir.normalized);
            if (proj.magnitude <= dir.magnitude && Vector2_.Dot(proj, dir) > 0)
            {
                hit = Vector2_.Project(-box.center_, dir.normalized) + box.center_ + o;
                return true;
            }
        }

        t  = box.bottomLeft + dir;
        d  = box.leftEdge * t.y - t.x * box.bottomEdge;
        dd = d * d;

        if (rr - dd > 0)
        {
            var proj = Vector2_.Project(-box.bottomLeft, dir.normalized);
            if (proj.magnitude <= dir.magnitude && Vector2_.Dot(proj, dir) > 0)
            {
                hit = Vector2_.Project(-box.center_, dir.normalized) + box.center_ + o;
                return true;
            }
        }

        box.SetCenter(tp);

        if (box.Intersect(other))
        {
            hit = tp + o;
            return true;
        }

        return false;
    }

    public static bool Cross(Sphere2D sphere, Sphere2D other, Vector2_ tp, ref Vector2_ hit)
    {
        if (sphere.Intersect(other)) return true;

        var o = other.center_;

        tp -= o;
        sphere.SetCenter(sphere.center_ - o);
        other.SetCenter(Vector2_.zero);

        other.radius += sphere.radius;

        var dir = tp - sphere.center_;
        var rr  = other.radius * other.radius * dir.sqrMagnitude;
        var t   = sphere.center_ + dir;
        var d   = sphere.x * t.y - t.x * sphere.y;
        var dd  = d * d;

        if (rr - dd > 0)
        {
            var proj = Vector2_.Project(-sphere.center_, dir.normalized);
            if (proj.magnitude <= dir.magnitude && Vector2_.Dot(proj, dir) > 0)
            {
                hit = proj + sphere.center_ + o;
                return true;
            }
        }

        sphere.SetCenter(tp);

        if (sphere.Intersect(other))
        {
            hit = tp + o;
            return true;
        }

        return false;
    }

    public static bool Cross(Sphere2D sphere, Box2D other, Vector2_ tp, ref Vector2_ hit)
    {
        var box = new Box2D(sphere.center_, sphere.radius * 2, sphere.radius * 2);

        return Cross(box, other, tp, ref hit);
    }

    private static void UpdateTouched(Collider_ trigger, Collider_ receiver, bool old, bool now)
    {
        if (old == now) return;

        if (old)
        {
            trigger.EndCollision(receiver);
            receiver.EndCollision(trigger);
        }
        else
        {
            trigger.BeginCollision(receiver);
            receiver.BeginCollision(trigger);
        }
    }

    #endregion

    [SerializeField]
    private List<Collider_> m_colliders = new List<Collider_>();

    public List<T> _GetColliders<T>(int mask, bool includeInactive = false, List<T> list = null) where T : Collider_
    {
        if (list == null) list = new List<T>();

        foreach (var collider in m_colliders)
        {
            var c = collider as T;
            if (!c || mask != 0 && !mask.BitMask(c.layer) || !includeInactive && !c.isActiveAndEnabled) continue;

            list.Add(c);
        }

        return list;
    }
    [Conditional("FIGHT_LOG")]
    public void LogColliders()
    {
        FightRecordManager.RecordLog< LogInt>(log =>
        {
            log.tag = (byte)TagType.LogColliders;
            log.value = m_colliders.Count;
        });

        foreach (var c in m_colliders)
        {
            FightRecordManager.RecordLog<LogString>(l =>
            {
                l.tag = (byte)TagType.LogColliders;
                if (c && c.isActive)
                    l.value = Util.GetHierarchy(c.transform);
                else
                    l.value = "collider disable";
            });
        }
    }

    public void _Foreach<T>(System.Predicate<T> call, int mask = 0,  bool includeInactive = false) where T : Collider_
    {
        for (int i = 0, c = m_colliders.Count; i < c; ++i)
        {
            var col = m_colliders[i];
            if (!col || mask != 0 && !mask.BitMask(col.layer) || !includeInactive && !col.isActiveAndEnabled) continue;
            var t = col as T;
            if (!t) continue;

            if (!call(t)) return;
        }
    }

    public void _Update(int diff)
    {
        var count = m_colliders.Count;
        for (var i = 0; i < count;)
        {
            var collider = m_colliders[i];
            if (!collider || !collider.isActive)
            {
                m_colliders.RemoveAt(i);
                --count;
                LogColliders();
            }
            else
            {
                ++i;

                try
                {
                    collider.TouchUpdate(diff);
                    if (!collider.isTrigger) continue;

                    if (collider.enableSnapshot)
                    {
                        var idx = 0;
                        while (collider.UpdateSnapshot(idx++))
                            CheckCollision(collider, ref count, ref i);
                    }
                    else CheckCollision(collider, ref count, ref i);

                    collider.OnQuitFrame();
                }
                #if DEVELOPMENT_BUILD || UNITY_EDITOR
                catch (System.Exception e) { Logger.LogException(e); }
                #else
                catch { }
                #endif
            }
        }
    }

    private void CheckCollision(Collider_ trigger, ref int count, ref int i)
    {
        for (var j = 0; j < count;)
        {
            if (j == i)
            {
                ++j;
                continue;
            }

            var other = m_colliders[j];

            if (!other || !other.isActive)
            {
                m_colliders.RemoveAt(j);
                LogColliders();
                --count;
                if (j < i) --i;
            }
            else
            {
                ++j;

                if (other.isTrigger) continue;

                CheckCollision(trigger, other);
            }
        }
    }

    private void CheckCollision(Collider_ trigger, Collider_ receiver)
    {
        var mask    = trigger.targetMask;
        var touched = trigger.Touched(receiver);
        var test    = (!trigger.ignoreSameGroup || trigger.transform.parent != receiver.transform.parent) && mask.BitMask(receiver.layer);

        if (test) test = trigger.isActiveAndEnabled && receiver.isActiveAndEnabled && trigger.CollisionTest(receiver);

        if (touched != test)
        {
            FightRecordManager.RecordLog<LogCheckCollider>(l =>
            {
                l.tag = (byte) TagType.LogCheckCollider;
                l.test = test;
                l.ignoreSameGroup = trigger.ignoreSameGroup;
                l.mask = mask;
                l.layer = receiver.layer;
            });
        }
        UpdateTouched(trigger, receiver, touched, test);
    }

    public void Register(Collider_ collider)
    {
        if (m_colliders.Contains(collider)) return;

        m_colliders.Add(collider);

        LogColliders();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_colliders.Clear();

        m_colliders = null;
    }
}
