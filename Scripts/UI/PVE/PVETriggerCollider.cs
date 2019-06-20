using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PVETriggerCollider : Box2DCollider
{
    #region static functions

    public static Transform triggerParent;
    public static PVETriggerCollider Create(SCreateTriggerBehaviour b)
    {
        if (!Level.currentRoot) return null;
        if(!triggerParent)
        {
            triggerParent = Level.currentRoot.AddNewChild();
            triggerParent.name = "trigger_parent";
        }

        Transform t = triggerParent.AddNewChild();
        t.name = Util.Format("trigger_{0}",b.triggerId.ToString("D2"));
        var c = t.GetComponentDefault<PVETriggerCollider>();
        //必须要返回的示例，避免修改内存中的备份
        SCreateTriggerBehaviour b_new = new SCreateTriggerBehaviour(b.behaviour.DeepClone(), null);
        Vector4_ range = b_new.range;
        if(b_new.isRandom)
        {
            range.x = (new PseudoRandom()).Range(b_new.range.x, b_new.range.y);
            range.y = 0;
        }
        c.InitComponent(b_new, range);
        c.RefreshScenePlayers();
        return c;
    }

    #endregion

    #region enum

    public enum EnumTriggerState
    {
        Close,
        Open,
    }

    public enum EnumTriggerType
    {
        None = 0,
        Enter,
        Exit,
        Stay,
    }

    #endregion

    #region fileds

    private Color[] m_color = new Color[2] { new Color(0.5f,0,1), new Color(0.9f,0,1) };

    public SCreateTriggerBehaviour behaviour { get; private set; }
    public int triggerId { get { return behaviour == null ? 0 : behaviour.triggerId; } }
    public EnumTriggerState state
    {
        get { return behaviour == null ? EnumTriggerState.Close : (EnumTriggerState)behaviour.state; }
        set { if (behaviour != null) behaviour.state = (int)value; }
    }

    public string activeEffect { get { return behaviour == null ? string.Empty : behaviour.activeState; } }
    public string inactiveEffect { get { return behaviour == null ? string.Empty : behaviour.inactiveEffect; } }
    private GameObject m_activeEffObj;
    private GameObject m_inactiveEffObj;

    //当前进入触发器的所有玩家
    public List<Creature> m_enterCreatures = new List<Creature>();
    //已经进来并且离开的所有玩家
    public List<Creature> m_exitCreatures = new List<Creature>();
    //当前场景上的所有玩家
    public List<Creature> m_sceneCreatures = new List<Creature>();
    #endregion

    #region override

    protected override void OnCollisionBegin(Collider_ other)
    {
        if (CheckValidCollider(other))
        {
            base.OnCollisionBegin(other);
            Creature c = (other as CreatureHitCollider).creature;
            OnCreatureEnterTrigger(c);
        }
    }

    protected override void OnCollisionEnd(Collider_ _other)
    {
        if (CheckValidCollider(_other))
        {
            base.OnCollisionEnd(_other);
            Creature c = (_other as CreatureHitCollider).creature;
            OnCreatureExitTrigger(c);
        }
    }

    private bool CheckValidCollider(Collider_ c)
    {
        return state == EnumTriggerState.Open && c is CreatureHitCollider;
    }

    #endregion

    #region functions

    private void InitComponent(SCreateTriggerBehaviour b, Vector4_ range)
    {
        behaviour = b;
        CreateEffectNode();
        isTrigger = true;
        layer = 0;
        AddLayerToTarget(Creature.COLLIDER_LAYER_HIT);
        SetRange(range);
        PlayParticle();
    }

    private void RefreshScenePlayers()
    {
        m_sceneCreatures.Clear();
        m_sceneCreatures = ObjectManager.FindObjects<Creature>(c => c.creatureCamp == CreatureCamp.PlayerCamp && !(c is PetCreature || c is MonsterCreature));
    }

    private void CreateEffectNode()
    {
        CreatePerEffectNode(ref m_activeEffObj, activeEffect);
        CreatePerEffectNode(ref m_inactiveEffObj, inactiveEffect);
    }

    private void CreatePerEffectNode(ref GameObject obj,string assetName)
    {
        if (!string.IsNullOrEmpty(assetName) && !obj)
        {
            obj = Level.GetPreloadObjectFromPool(assetName);
            if(!obj) obj = Level.GetPreloadObject<GameObject>(assetName);

            if (obj)
            {
                obj.transform.SetParent(transform);
                obj.SafeSetActive(false);
            }
            else Logger.LogError("cannot load pve trigger collider effect with asset name {0}", assetName);
        }
    }

    public void SetRange(Vector4 range)
    {
        size = new Vector2_(range.z,range.w);
        position = new Vector3_(range.x,range.y);
        offset = new Vector2_(0, range.w * 0.5f);
        PlayParticle();
    }

    public void ReActive(SCreateTriggerBehaviour b, Vector4 range)
    {
        behaviour = b;
        //force set state to 1
        if(state != EnumTriggerState.Open)
        {
            behaviour.state = 1;
        }
        SetRange(range);
    }


    private void PlayParticle()
    {
        if(state == EnumTriggerState.Close && !m_inactiveEffObj) CreatePerEffectNode(ref m_inactiveEffObj, inactiveEffect);
        if(state == EnumTriggerState.Open && !m_activeEffObj) CreatePerEffectNode(ref m_activeEffObj, activeEffect);

        if (m_inactiveEffObj)
        {
            m_inactiveEffObj.SafeSetActive(state == EnumTriggerState.Close);
            PlayParticle(m_inactiveEffObj);
        }

        if (m_activeEffObj)
        {
            m_activeEffObj.SafeSetActive(state == EnumTriggerState.Open);
            PlayParticle(m_activeEffObj);
        }
    }

    private void PlayParticle(GameObject obj)
    {
        if (!obj) return;

        ParticleSystem[] ps = obj.GetComponentsInChildren<ParticleSystem>();
        foreach (var item in ps)
        {
            item.Play();
        }
    }

    public void SetState(int s)
    {
        state = (EnumTriggerState)s;
        if (state == EnumTriggerState.Close) ClearTriggerCreatures();
        PlayParticle();
    }

    public void ClearTriggerCreatures()
    {
        m_enterCreatures.Clear();
        m_exitCreatures.Clear();
    }

    public void OnCreatureEnterTrigger(Creature p)
    {
        if (CheckValidCreature(p))
        {
            if (m_enterCreatures.Contains(p)) Logger.LogWarning("player {0} has already enter,you need remove first when creature exit from trigger", p);
            else m_enterCreatures.Add(p);

            if (m_exitCreatures.Contains(p)) m_exitCreatures.Remove(p);

            Module_PVEEvent.instance?.AddCondition(new SEnterTriggerConditon(triggerId, EnumTriggerType.Enter, GetValidCount(true)));
            CheckAllPlayerCondition();
        }
        else
        {
            if(p is MonsterCreature && p.creatureCamp == CreatureCamp.MonsterCamp)
            {
                if (!m_enterCreatures.Contains(p)) m_enterCreatures.Add(p);
                if (m_exitCreatures.Contains(p)) m_exitCreatures.Remove(p);
                Module_PVEEvent.instance?.AddCondition(new SEnterTriggerConditon(triggerId, EnumTriggerType.Enter,0 , (p as MonsterCreature).monsterId));
            }
        }
    }

    public void OnCreatureExitTrigger(Creature p)
    {
        if (CheckValidCreature(p))
        {
            if (m_exitCreatures.Contains(p)) Logger.LogWarning("player {0} has already exit,you need remove first when creature enter from trigger", p);
            else m_exitCreatures.Add(p);

            if (m_enterCreatures.Contains(p)) m_enterCreatures.Remove(p);

            Module_PVEEvent.instance?.AddCondition(new SEnterTriggerConditon(triggerId, EnumTriggerType.Exit, GetValidCount(false)));
            CheckAllPlayerCondition();
        }
        else
        {
            if (p is MonsterCreature && p.creatureCamp == CreatureCamp.MonsterCamp)
            {
                if (!m_exitCreatures.Contains(p)) m_exitCreatures.Add(p);
                if (m_enterCreatures.Contains(p)) m_enterCreatures.Remove(p);
                Module_PVEEvent.instance?.AddCondition(new SEnterTriggerConditon(triggerId, EnumTriggerType.Exit, 0, (p as MonsterCreature).monsterId));
            }
        }
    }

    private bool CheckValidCreature(Creature p)
    {
        return p && !(p is MonsterCreature || p is PetCreature) && p.creatureCamp == CreatureCamp.PlayerCamp;
    }
    
    private int GetValidCount(bool enterCreature)
    {
        List<Creature> cs = enterCreature ? m_enterCreatures : m_exitCreatures;
        int count = 0;
        foreach (var item in cs)
        {
            if (item) count++;
        }
        return count;
    }

    private void CheckAllPlayerCondition()
    {
        bool allIn = m_enterCreatures.Count > 0, allExit = m_exitCreatures.Count > 0;

        foreach (var item in m_sceneCreatures)
        {
            //死亡玩家不检查
            if (item.health <= 0 || !item.gameObject.activeInHierarchy) continue;

            if (!m_enterCreatures.Contains(item)) allIn = false;
            if (!m_exitCreatures.Contains(item)) allExit = false;
        }

        if (allIn) Module_PVEEvent.instance?.AddCondition(new SEnterTriggerConditon(triggerId, EnumTriggerType.Enter, -1));
        if (allExit) Module_PVEEvent.instance?.AddCondition(new SEnterTriggerConditon(triggerId, EnumTriggerType.Exit, -1));
    }

    #endregion

    #region draw gizmos

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnDrawGizmos()
    {
        DrawPVETrigger();
    }

    private void OnDrawGizmosSelected()
    {
        DrawPVETrigger();
    }

    private void DrawPVETrigger()
    {
        UpdateBox();

        Gizmos.color = m_color.GetValue<Color>((int)state);

        if (m_box.size_ != Vector2_.zero)
        {
            Gizmos.DrawLine((Vector2)m_box.topLeft, (Vector2)m_box.topRight);
            Gizmos.DrawLine((Vector2)m_box.topRight, (Vector2)m_box.bottomRight);
            Gizmos.DrawLine((Vector2)m_box.bottomRight, (Vector2)m_box.bottomLeft);
            Gizmos.DrawLine((Vector2)m_box.bottomLeft, (Vector2)m_box.topLeft);
        }
    }
#endif

    #endregion
}
