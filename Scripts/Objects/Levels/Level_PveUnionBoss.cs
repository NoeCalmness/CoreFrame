/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-08-14
 * 
 ***************************************************************************************************/

public class Level_PveUnionBoss : Level_PVE
{
    public const double SEND_HURT_TIME_INTERVAL = 1;

    public override bool canPause { get { return false; } }
    public override EnumPveLevelType pveLevelType { get { return EnumPveLevelType.UnionBoss; } }

    private double m_times = 0;

    private MonsterCreature monsterBoss;
    protected override void OnDestroy()
    {
        base.OnDestroy();
        moduleUnion.m_bossOpen = false;
        m_times = 0;
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);
        OnCountDown(diff * 0.001);
    }

    private void OnCountDown(double deltaTime)
    {
        if (moduleUnion.m_bossOpen)
        {
            m_times += deltaTime;
            if (m_times > SEND_HURT_TIME_INTERVAL)  SetHurt();
        }
    }

    private void SetHurt()
    {
        m_times = 0;

        int allHurt = (int)modulePVE.GetPveGameData(EnumPVEDataType.Attack);
        int secAttack = allHurt - moduleUnion.lastTime;
        moduleUnion.lastTime = allHurt;

        if (secAttack > 0)  moduleUnion.SendMyAttack(secAttack);
    }

    protected override void OnCreateCreatures()
    {
        base.OnCreateCreatures();
        //玩家不会和怪物的碰撞盒子产生阻挡
        player.behaviour.collider_.gameObject.SetActive(false);
        RefreshAllBuff();
    }

    protected override void AfterCreateMonster(MonsterCreature monster)
    {
        base.AfterCreateMonster(monster);
        monsterBoss = monster;
        m_times = 0;
        moduleUnion.m_inUnionPve = true;
        if (moduleUnion.m_isUnionBossTask && moduleUnion.BossInfo.bossstate == 1)
        {
            moduleUnion.m_bossOpen = true;
            monster.maxHealth = moduleUnion.m_bossStage.bossHP;
            monster.health = moduleUnion.BossInfo.remianblood;
        }
        else
        {
            var state = moduleUnion.BossInfo.remianblood <= 0 ? PVEOverState.Success : PVEOverState.GameOver;
            modulePVE.SendPVEState(state);
        }
        monster.AddEventListener(CreatureEvents.DEAD, UnionBossRealDeath);
    }

    private void UnionBossRealDeath(Event_ e)
    {
        SetHurt();
    }
    
    // 刷新所有的buff.
    private void RefreshAllBuff()
    {
        for (int i = 0; i < moduleUnion.BossBuffInfo.Count; i++)
        {
            if (moduleUnion.BossBuffInfo[i] == null || moduleUnion.BossBuffInfo[i]?.effectId <= 0) continue;
            for (int j = 0; j < moduleUnion.BossBuffInfo[i].times; j++)
            {
                BuffInfo buff = ConfigManager.Get<BuffInfo>(moduleUnion.BossBuffInfo[i].effectId);
                if (buff) Buff.Create(buff, player);
            }
        }
    }

    private void _ME(ModuleEvent<Module_Union> e)
    {
        switch (e.moduleEvent)
        {
            case Module_Union.EventUnionBossHurt:
                if (monsterBoss == null) return;
                if (monsterBoss.health > 0 && monsterBoss.health > moduleUnion.BossInfo.remianblood)
                {
                    monsterBoss.health = moduleUnion.BossInfo.remianblood;
                }
                break;
            case Module_Union.EventUnionBossOver:
                HandleToPVERecvMsg();
                break;
            case Module_Union.EventUnionBossClose:
                if (!moduleUnion.m_inUnionPve) return;
                
                modulePVEEvent.pauseCountDown = true;
                m_times = 0;
                var state = moduleUnion.BossInfo.remianblood <= 0 ? PVEOverState.Success : PVEOverState.GameOver;

                modulePVE.SendPVEState(state);
                break;
            case Module_Union.EventUnionBuyEffect:
                var effId = Util.Parse<int>(e.param1.ToString());
                if (effId == 0) return;
                RefreshAllBuff();
                break;
        }
    }
}
