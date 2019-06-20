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

public class CreatureAttackCollider : AttackCollider
{
    public override int startDirection => creature ? creature.direction.ToInt() : 0;
    public override int direction => startDirection;

    protected override void Awake()
    {
        base.Awake();

        m_type = AttackColliderTypes.Attack;
        m_enableSnapshot = true;
    }
}
