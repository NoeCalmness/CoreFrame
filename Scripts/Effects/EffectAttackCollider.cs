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

public class EffectAttackCollider : AttackCollider
{
    public FlyingEffect effect
    {
        get { return m_effect; }
        set
        {
            if (m_effect == value) return;
            m_effect = value;

            if (m_effect && m_effect.effectInfo.bullet) m_type = AttackColliderTypes.Bullet;
            m_startDirection = creature ? creature.direction.ToInt() : 0;
        }
    }

    public override int startDirection => m_startDirection;
    public override int direction => m_effect ? m_effect.intDirection : 0;

    private FlyingEffect m_effect;
    private int m_startDirection = 0;

    protected override void Awake()
    {
        base.Awake();

        m_type = AttackColliderTypes.Effect;

        gameObject.layer = 12;
    }

    public void CollisionBegin(Collider_ other)
    {
        OnCollisionBegin(other);
    }

    public override void OnRealHitTarget(CreatureHitCollider target, AttackInfo a)
    {
        base.OnRealHitTarget(target, a);

        effect.OnHit(target);
    }

    protected override void OnCollisionBegin(Collider_ other)
    {
        base.OnCollisionBegin(other);

        // We only handle ground hit here, normal hit will handled by OnRealHitTarget
        if (other.gameObject.layer == 11)  // We hit ground!
            effect.OnHit(null);
    }
}
