/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Description
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.0
 * Created:  2017-
 * 
 ***************************************************************************************************/

using UnityEngine;

public class CreatureCollider : Box2DCollider
{
    public Creature creature { get; set; }

    public Vector3_ Check2D(Vector3_ tp, Creature target)
    {
        var n = new Vector2_(creature.x, creature.cy);
        var t = new Vector2_(target.x,   target.cy);

        var c = creature.colliderSize + target.colliderSize;

        if (t.y > n.y + creature.colliderHeight || Mathd.Abs(n.x - t.x) < c * 0.025) return tp;

        var nx = n.x;
        var tx = tp.x;

        if (tx > nx && t.x < n.x || tx < nx && t.x > n.x) return tp;

        var max = tx > nx ? t.x - c : t.x + c;
        nx = tx > nx && tx > max || tx < nx && tx < max ? max : tx;
        tp.x = nx;

        return tp;
    }

    public Vector3 Check(Vector3 tp, Creature target)
    {
        var np   = transform.position + new Vector3((float)creature.x, (float)creature.colliderOffset);
        var dir  = tp - np;
        var dir1 = target.position + new Vector3((float)target.x, (float)target.colliderOffset) - np;

        var dot = Vector3.Dot(dir, dir1);

        if (dot <= 0) return tp;

        var dis  = dir.magnitude;
        var proj = Vector3.Project(dir1, dir.normalized);
        var plen = proj.magnitude;

        var c  = creature.colliderSize;
        var c1 = target.colliderSize;

        if (plen < dis + c + c1)
        {
            var d1 = dir1.magnitude;
            var d2 = d1 - plen;
            if (d2 < c)
            {
                dir *= (float)((plen - (c + c1)) / dis);
                return np + dir;
            }
        }

        return tp;
    }
}
