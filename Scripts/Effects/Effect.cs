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
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Effect : SceneObject
{
    #region Events

    public static string FOLLOW_EFFECT_DESTORY = "OnFollowEffectDESTORY";

    #endregion

    #region Static functions

    public static Effect Create(StateMachineInfo.Effect effInfo, Transform node, Creature source = null, Buff sourceBuff = null)
    {
        var pos = effInfo.offset * 0.1f;
        var rotation = effInfo.rotation;
        if (node)
        {
            pos = node.TransformPoint(pos);
            rotation = (node.rotation * Quaternion.Euler(rotation)).eulerAngles;
        }

        if (effInfo.lockY != 0) pos.y = effInfo.lockY < 0 ? 0 : effInfo.lockY;

        var eff = Create<Effect>(effInfo.effect, effInfo.effect, pos, rotation);

        eff.source = source;
        eff.follow = effInfo.follow;
        eff.sourceBuff = sourceBuff;
        eff.m_inherit = effInfo.inherit;
        eff.m_freez = !effInfo.markedHit && source && !source.stateMachine.playable;

        if (effInfo.follow)
#if UNITY_EDITOR
        {
            eff.transform.SetParent(node, true);
            if (source && source.lockLayer)
                Util.SetLayer(eff.gameObject, source.lockedLayer);
        }
#else
            eff.transform.SetParent(node, true);
#endif

        eff.m_originEular = eff.localEulerAngles;
        eff.lockForward   = effInfo.syncRotation ? Vector3.zero : eff.transform.forward;
        eff.startForward  = !source || source.isForward;

        if (sourceBuff != null)
        {
            eff.m_buffCheck = sourceBuff.version;
            eff.lifeTime = -1;
        }
        else eff.m_buffCheck = 0;

        if (source)
        {
            if (effInfo.interrupt) source.AddEventListener(CreatureEvents.QUIT_STATE, eff.OnCreatureQuitState);
            if (effInfo.follow)
            {
                source.AddEventListener(CreatureEvents.DIRECTION_CHANGED,    eff.OnCreatureDirectionChanged);
                source.AddEventListener(CreatureEvents.MORPH_MODEL_CHANGED,  eff.OnCreatureMorphModelChanged);

                if (node != source.behaviour.effects.transform) source.AddEventListener(CreatureEvents.RESET_LAYERS, eff.OnCreatureResetLayers);
            }
            source.AddEventListener(CreatureEvents.STATE_PLAYABLE, eff.OnCreatureStatePlayable);
        }

        eff.m_inverted = source && !source.isForward;
        eff.UpdateInvert();

        return eff;
    }

    #endregion

    public bool isFlyingEffect { get { return false; } }
    public bool follow { get; protected set; }
    public bool startForward { get; protected set; }
    public Vector3 lockForward { get; set; }
    public Creature source { get; protected set; }
    public Buff sourceBuff { get; protected set; }
    public bool inverted
    {
        get { return m_inverted; }
        set
        {
            if (m_inverted == value) return;
            m_inverted = value;

            UpdateInvert();
        }
    }

    public int lifeTime = 1500;

    protected Vector3 m_originEular;
    protected int m_time = 0;
    protected bool m_freez = false;
    protected int m_buffCheck = 0;
    protected bool m_inverted = false;
    protected bool m_inherit = false;
    protected bool m_markedHit = false;

    private EffectInvertHelper m_inverter;

    private PauseResumParticles m_pauseResum;

#if AI_LOG
    public int logId; 
#endif

    protected override void OnAddedToScene()
    {
        m_time = 0;
        enableUpdate = true;

        lifeTime = 1500;
        var ad = GetComponent<AutoDestroy>();
        if (ad)
        {
            lifeTime = ad.delay;
        }

        InitComponent();
    }

    protected void InitComponent()
    {
        var ad = GetComponent<AutoDestroy>();
        if (ad)
        {
            ad.enabled = false;
        }
        
        m_pauseResum = GetComponentDefault<PauseResumParticles>();
        m_pauseResum.Initialize(false, (float) localTimeScale);

        m_inverter = transform.GetComponent<EffectInvertHelper>();
        if (m_inverter) m_inverter.inverted = m_inverted;

        m_pauseResum.UpdateSpeed((float)localTimeScale);
        m_pauseResum.PauseResum(m_freez);

        Util.SetLayer(transform, sourceBuff ? Layers.EFFECT_BUFF : Layers.EFFECT);
    }

    protected override void OnLocalTimeScaleChanged()
    {
        m_pauseResum?.UpdateSpeed((float)localTimeScale);
    }

    public override void OnUpdate(int diff)
    {
        if (source && m_freez) return;

        m_time += diff;

        if (lockForward != Vector3.zero && lockForward != transform.forward)
            transform.forward = lockForward;

        if (lifeTime > 0 && m_time >= lifeTime)
        {
            RecordLog();
            Destroy();
        }
        else if (sourceBuff != null)
        {
            if (sourceBuff.version != m_buffCheck || sourceBuff.destroyed)
            {
                RecordLog2();
                Destroy();
            }
//            else if (gameObject.activeSelf ^ sourceBuff.actived)
//                gameObject.SetActive(sourceBuff.actived);
        }
    }

    protected virtual void RecordLog() { }
    protected virtual void RecordLog2() { }

    private void OnCreatureStatePlayable(Event_ e)
    {
        if (m_markedHit) return;

        m_freez = !(bool)e.param1;

        m_pauseResum?.PauseResum(m_freez);
    }

    private void OnCreatureDirectionChanged()
    {
        inverted = !source.isForward;
    }

    private void OnCreatureMorphModelChanged()
    {
        var node = transform?.parent;
        if (!node) return;

        var nnode = node.name == "effects" ? source.behaviour.effects.transform : Util.FindChild(source.activeRootNode, node.name);
        if (nnode != node)
        {
            Util.AddChildStayLocal(nnode, transform);
            #if UNITY_EDITOR
            if (source.lockLayer) Util.SetLayer(gameObject, source.lockedLayer);
            #endif
        }
    }

    private void OnCreatureResetLayers()
    {
        Util.SetLayer(transform, sourceBuff ? Layers.EFFECT_BUFF : Layers.EFFECT);
    }

    private void OnCreatureQuitState(Event_ e)
    {
        if (!m_inherit || e.param1 != e.param2)
            Destroy();
    }

    protected void UpdateInvert()
    {
        if (!m_inverter) return;
        m_inverter.inverted = m_inverted;
    }

    protected override void OnDestroy()
    {
        //foreach (var render in m_renderers)
        //{
        //    var mats = render.materials;
        //    foreach (var m in mats)
        //    {
        //        if (!m) continue;
        //        Object.DestroyImmediate(m);
        //    }
        //}

        m_inverter = null;

        var node = transform.parent;
        if (node)
        {
            for (var i = node.childCount - 1; i >= 0; i--)
            {
                var c = node.GetChild(i);
                if (c && c.name.Equals(name) && c != transform)
                {
                    c.gameObject.SetActive(true);
                    break;
                }
            }
        }

        base.OnDestroy();

        if (source) source.RemoveEventListener(this);

        source     = null;
        sourceBuff = null;
    }
}

public class FlyingEffect : Effect, IRenderObject
{
    #region Static functions

    public static FlyingEffect Create(StateMachineInfo.FlyingEffect effInfo, Creature source, int overrideForward, bool invert, StateMachineInfo.Effect hitEffect, Vector3_ root, Vector3_ rootEular, Vector3 rootRot)
    {
        var fe = ConfigManager.Get<FlyingEffectInfo>(effInfo.effect);
        if (!fe)
        {
            Logger.LogWarning("FlyingEffect::Create: Could not create flying effect [{0}], could not find effect config from config_flyingEffectInfos", effInfo.effect);
            return null;
        }

        FightRecordManager.RecordLog<LogString>(log =>
        {
            log.tag = (byte)TagType.CreateFlyEffect;
            log.value = fe.effect;
        });

        var off = effInfo.offset * 0.1;
        if (invert)
        {
            off.x = -off.x;
            off.z = -off.z;
        }

        var direction = effInfo.direction;
        var rotation  = effInfo.rotation;

        direction.Set(direction.z, direction.y, -direction.x);
        rotation.Set(-rotation.x, rotation.y, rotation.z);

        var eular = rootEular + direction;

        eular.z = Mathd.ClampAngle(eular.z);

        var z = Mathd.AngToRad(eular.z);
        var dir = new Vector3_(Mathd.Cos(z), -Mathd.Sin(z));
        var pos = root + off;
        var eff = Create<FlyingEffect>(fe.effect, fe.effect, pos, rotation);
        eff.enableUpdate = true;

        if (invert) dir.x = -dir.x;

        eff.sourceBuff = null;
        eff.follow     = false;
        eff.m_inherit  = false;
        eff.m_time     = 0;

        eff.m_originEular = eff.localEulerAngles;
        eff.lockForward   = Vector3.zero;
        eff.startForward  = overrideForward == 0 ? !source || source.isForward : overrideForward > 0;

        eff.m_curSectionIdx = 0;
        eff.m_curActivedSection = -1;
        eff.m_curSubEffectIdx = 0;
        eff.m_curSoundEffectIdx = 0;
        eff.m_curShakeIdx = 0;
        eff.m_groundHited = false;
        eff.m_renderTime = 0;

        eff.effectInfo   = fe;
        eff.source       = source;
        eff.position_    = pos;
        eff.velocity     = effInfo.velocity * 0.0001;
        eff.acceleration = effInfo.acceleration * 0.0001;
        eff.lifeTime     = fe.lifeTime;
        eff.hitEffect    = hitEffect;

        eff.CreateCollider(rootRot, dir, eular);

        eff.m_inverted = invert;
        eff.UpdateInvert();

#if AI_LOG
        eff.logId = MonsterCreature.GetMonsterRoomIndex();
        Module_AI.LogBattleMsg(source, "create a flying effect[logId:{0}] with pos {1}, lifeTime = {2}  startForward is {3}", eff.logId, eff.position_, eff.lifeTime, eff.startForward);
#endif
        return eff;
    }

    #endregion

    public new bool isFlyingEffect { get { return true; } }

    public double velocity;
    public double acceleration;
    public Vector3_ direction;
    public int intDirection;
    public Vector3_ eular_;
    public Vector3_ position_;
    public FlyingEffectInfo effectInfo;
    public StateMachineInfo.Effect hitEffect = StateMachineInfo.Effect.empty;

    private int m_curSubEffectIdx;
    private int m_curSoundEffectIdx;
    private int m_curSectionIdx;
    private int m_curShakeIdx;
    private int m_curActivedSection;

    private EffectAttackCollider m_collider;
    private Transform m_root;

    private double m_lastY;
    private bool m_groundHited;

    protected override void OnAddedToScene()
    {
        InitComponent();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        m_time = 0;
    }

    protected override void RecordLog()
    {
        FightRecordManager.RecordLog<LogEffectDestory>(log =>
        {
            log.tag = (byte)TagType.LogEffectDestory;
            log.currentLife = m_time;
            log.life = lifeTime;
        });
    }

    protected override void RecordLog2()
    {
        FightRecordManager.RecordLog<LogEffectDestory2>(log =>
        {
            log.tag = (byte)TagType.LogEffectDestory;
            log.buffCheck = m_buffCheck;
            log.version = sourceBuff.version;
            log.isDestroyed = sourceBuff.destroyed;
        });
    }

    public void OnHit(CreatureHitCollider hit)
    {
        FightRecordManager.RecordLog<LogEmpty>(log =>
        {
            log.tag = (byte)TagType.FlyEffectOnHit;
        });

        if (destroyed || !hit && m_groundHited) return;

        if (!hit) m_groundHited = true;

        if (!effectInfo.hitEffect.isEmpty)
            Create(effectInfo.hitEffect, source, startForward ? 1 : -1, inverted, hitEffect, position_, eular_, m_root.eulerAngles);

        if (effectInfo.removeOnHit == 1 || effectInfo.removeOnHit == 2 && !hit)
        {
            FightRecordManager.RecordLog<LogEffectDestoryOnHit>(l =>
            {
                l.tag = (byte)TagType.FlyEffectOnHit;
                l.name = name;
                l.removeOnHit = effectInfo.removeOnHit;
            });

            Destroy();
        }

#if AI_LOG
        Module_AI.LogBattleMsg(source, "flying effect[logId: {0}] OnHit,hit creature is {1} with effect pos = {2}, collider pos = {3} ", logId, hit == null ? "null" : hit.creature ? hit.creature.uiName : "null",position_, m_collider == null ? "null" : m_collider.position.ToString());
#endif
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

#if AI_LOG
        Module_AI.LogBattleMsg(source, "flying effect destory [logId: {0}] with effect pos = {1}, collider pos = {2} ", logId, position_, m_collider == null ? "null" : m_collider.position.ToString());
#endif
        Object.Destroy(m_root.gameObject);
        m_collider.isActive = false;
        
        effectInfo = null;
    }

    private void CreateCollider(Vector3 rot, Vector3_ _direction, Vector3_ _eular)
    {
        eular_ = _eular;
        direction = _direction;
        intDirection = direction.x > 0 ? 1 : direction.x < 0 ? -1 : 0;

        m_root = new GameObject(name).transform;
        m_root.eulerAngles = new Vector3_(eular_.z * eular_.x, Mathd.ClampAngle(rot.y));
        m_root.position = position_;

        CreateCollider();

        m_lastY = position_.y;
    }

    private void CreateCollider()
    {
        var col = new GameObject("collider").transform;
        col.transform.SetParent(m_root);

        col.localPosition    = Vector3.zero;
        col.localScale       = Vector3.one;
        col.localEulerAngles = Vector3.zero;

        col.gameObject.layer = 9;

        m_collider = col.GetComponentDefault<EffectAttackCollider>();

        m_collider.creature     = source;
        m_collider.effect       = this;
        m_collider.isTrigger    = true;
        m_collider.layer        = Creature.COLLIDER_LAYER_EFFECT;
        m_collider.syncPosition = false;
        m_collider.position     = position_;
        m_collider.offset       = m_collider.sphereOffset = Vector2_.zero;
        m_collider.boxSize      = Vector2_.zero;
        m_collider.radius       = 0;

        m_collider.AddLayerToTarget(Creature.COLLIDER_LAYER_HIT);

        var dc = m_collider.GetComponentDefault<DebugCollider>();
        dc.color = Color.magenta;
        dc.drawCollider = true;

        var ang = eulerAngles;
        transform.SetParent(m_root.transform);
        transform.forward = direction;
        transform.localEulerAngles += ang;
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);

        FightRecordManager.RecordLog<LogEffectUpdate>(log =>
        {
            log.tag = (byte)TagType.EffectOnUpdate;
            log.name = effectInfo?.effect;
            log.life = lifeTime;
            log.currentLife = m_time;
            log.freez = m_freez;
            log.source = source.Identify;
            log.isDestroyed = destroyed;
        });

        if (destroyed) return;

        var tp = position_ + direction * (velocity + acceleration * m_time) * diff;
        if (tp.y < 0) tp.y = 0;

        if (m_collider.isActiveAndEnabled && (m_collider.radius > 0 || m_collider.boxSize != Vector2_.zero))
        {
            PhysicsManager.Foreach<CreatureHitCollider>(collider =>
            {
                var target = collider.creature;
                if (!target || source.SameCampWith(target) || collider.Touched(m_collider) || m_collider.AttackedCount(collider) > 0) return true; // ignore touched, attacked and same team targets

                var hit = (Vector2_)position_;
                var check = m_collider.radius > 0 && PhysicsManager.Cross(m_collider.sphere, collider.box, (Vector2_)tp + m_collider.sphereOffset, ref hit) ? 1 :
                m_collider.boxSize != Vector2_.zero && PhysicsManager.Cross(m_collider.box, collider.box, (Vector2_)tp + m_collider.offset, ref hit) ? 2 : 0;

                if (check == 0) return true;

                hit -= check == 1 ? m_collider.sphereOffset : m_collider.offset;

                var np = (Vector3_)hit;
                np.z = position_.z;

                position_ = np;
                m_root.position = np;
                m_collider.position = np;

                collider.BeginCollision(m_collider);
                m_collider.CollisionBegin(collider);

                return this;
            }, Creature.COLLIDER_LAYER_HIT.ToMask());

            if (!this) return;
        }

        position_ = tp;
        m_root.position = tp;
        m_collider.position = tp;

        UpdateSubEffects();
        UpdateSoundEffects();
        UpdateShakeEffects();
        UpdateAttackBox();

        if (!m_groundHited && m_lastY > 0 && tp.y <= 0) OnHit(null);
        m_lastY = tp.y;
    }

    private double m_renderTime = 0;

    public void OnRenderUpdate()
    {
        if (enableUpdate)
        {
            var delta = Time.smoothDeltaTime * 1000.0;
            m_renderTime += delta;

            var tp = (Vector3_)m_root.position + direction * (velocity + acceleration * m_renderTime) * delta;
            if (tp.y < 0) tp.y = 0;

            m_root.position = tp;
        }
    }

    public void OnPostRenderUpdate() { }

    private void UpdateAttackBox()
    {
        if (m_curActivedSection > -1 && m_time >= effectInfo.sections[m_curActivedSection].endTime)
        {
            m_curActivedSection = -1;
            m_collider.SetAttackBox(StateMachineInfo.AttackBox.empty, StateMachineInfo.Effect.empty, startForward);
        }

        if (m_curSectionIdx >= effectInfo.sections.Length) return;
        var section = effectInfo.sections[m_curSectionIdx];
        if (m_time < section.startTime) return;

        m_collider.SetAttackBox(effectInfo.sections[m_curSectionIdx].attackBox, hitEffect, startForward);

        m_curActivedSection = m_curSectionIdx;
        ++m_curSectionIdx;

        UpdateAttackBox();
    }

    private void UpdateSubEffects()
    {
        if (m_curSubEffectIdx >= effectInfo.subEffects.Length) return;

        var subEffect = effectInfo.subEffects[m_curSubEffectIdx];
        if (m_time < subEffect.startFrame) return;

        ++m_curSubEffectIdx;

        Create(subEffect, source, startForward ? 1 : -1, inverted, hitEffect, position_, eular_, m_root.eulerAngles);

        UpdateSubEffects();
    }

    private void UpdateSoundEffects()
    {
        if (m_curSoundEffectIdx >= effectInfo.soundEffects.Length) return;

        var se = effectInfo.soundEffects[m_curSoundEffectIdx];
        if (m_time < se.startFrame) return;

        ++m_curSoundEffectIdx;

        if (se.isEmpty || !se.global && source && !source.isPlayer || Random.Range(0, 1.0f) > se.rate)
        {
            UpdateSoundEffects();
            return;
        }

        var s = se.SelectSound();
        if (s.isEmpty) return;

        AudioManager.PlayAudio(s.sound, se.isVoice || s.isVoice ? AudioTypes.Voice : AudioTypes.Sound, false, se.overrideType);
    }

    private void UpdateShakeEffects()
    {
        if (m_curShakeIdx >= effectInfo.cameraShakes.Length) return;

        var shake = effectInfo.cameraShakes[m_curShakeIdx];
        if (m_time < shake.startFrame) return;

        ++m_curShakeIdx;

        var shakeInfo = ConfigManager.Get<CameraShakeInfo>(shake.intValue0);
        if (shakeInfo) Camera_Combat.Shake(shakeInfo.intensity, shakeInfo.duration * 0.001f, shakeInfo.range, position, source);

        UpdateShakeEffects();
    }
}

public class FollowTargetEffect : Effect, ILogicTransform
{
    private double      acceleration;
    private double      velocity;
    private Vector3_    position_;
    private Creature    target;
    private StateMachineInfo.FollowTargetEffect.TriggerType triggerType;
    /// <summary>
    /// 第一步。飞到随机点
    /// </summary>
    private bool        firstStep;
    private Vector3_    randomPosition;
    private Vector3_    offset;

    private Motion_SlantingThrow motion;
    private object      conditionValue;
    private Vector3_    startPos;
    #region static functions

    public static FollowTargetEffect Create(StateMachineInfo.FollowTargetEffect effInfo, Transform node, Creature target,Vector3_ position,
        Creature source = null, Buff sourceBuff = null)
    {
        var eff = Create<FollowTargetEffect>(effInfo.effect, effInfo.effect, position + effInfo.motionData.offset,
            (node.rotation*Quaternion.Euler(effInfo.rotation)).eulerAngles);

        eff.enableUpdate    = true;
        eff.source          = source;
        eff.follow          = false;
        eff.m_time          = 0;
        eff.offset          = effInfo.motionData.offset;
        eff.target          = target;
        eff.sourceBuff      = sourceBuff;
        eff.triggerType     = effInfo.triggerType;
        eff.m_originEular   = eff.localEulerAngles;
        eff.localScale      = effInfo.scale;
        eff.velocity        = effInfo.motionData.velocity;
        eff.acceleration    = effInfo.motionData.acceleration;
        eff.randomPosition = eff.position_ = position + effInfo.motionData.offset;
        eff.motion          = new Motion_SlantingThrow(eff, target, effInfo.motionData);
        eff.lifeTime        = -1;
        //粒子系统要更改粒子节点才能改其缩放。搞不懂为什么
        var particles = eff.transform.GetComponentsInChildren<ParticleSystem>();
        if (particles != null && particles.Length > 0)
        {
            for (var i = 0; i < particles.Length; i++)
            {
                particles[i].transform.localScale = effInfo.scale;
            }
        }

        eff.transform.SetParent(null);

        if (effInfo.randomPosition)
        {
            var p = eff.position_;
            eff.randomPosition = new Vector3_(p.x + 1.5 * moduleBattle._Range(-1.0, 1.0), p.y + 1.5 * moduleBattle._Range(0, 1.0));
            eff.startPos = eff.randomPosition;
        }
        else
            eff.startPos = eff.position_;


        if (sourceBuff != null)
        {
            eff.m_buffCheck = sourceBuff.version;
            eff.lifeTime = -1;
        }
        else eff.m_buffCheck = 0;

        eff.m_inverted = source && !source.isForward;
        eff.UpdateInvert();
        
        eff.InitTrigger();

#if AI_LOG
        eff.logId = MonsterCreature.GetMonsterRoomIndex();
        Module_AI.LogBattleMsg(source, "create a FollowTargetEffect[logId: {0}] with target {0} startPos {1}, lifeTime = {1}  startForward is {2}", eff.logId, eff.position_, eff.lifeTime, eff.startForward);
#endif

        return eff;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        m_time = 0;
    }

    private void InitTrigger()
    {
        if (triggerType == StateMachineInfo.FollowTargetEffect.TriggerType.None)
            enableUpdate = true;
        else
        {
            if (randomPosition != position_)
            {
                firstStep = true;
            }else
                enableUpdate = false;
            switch (triggerType)
            {
                case StateMachineInfo.FollowTargetEffect.TriggerType.ComboBreak:
                    source.AddEventListener(CreatureEvents.COMBO_BREAK, OnComboBreak);
                    break;
            }
        }
    }

    private void OnComboBreak(Event_ e)
    {
        enableUpdate = true;
        conditionValue = e.param1;
        source.RemoveEventListener(CreatureEvents.COMBO_BREAK, OnComboBreak);
    }

    #endregion

    protected override void OnAddedToScene()
    {
        InitComponent();
        //这类特效，只会在贴近目标后销毁
        lifeTime = -1;
    }

    protected override void RecordLog()
    {
        FightRecordManager.RecordLog<LogEffectDestory>(log =>
        {
            log.tag = (byte)TagType.LogEffectDestory;
            log.currentLife = m_time;
            log.life = lifeTime;
        });
    }

    protected override void RecordLog2()
    {
        FightRecordManager.RecordLog<LogEffectDestory2>(log =>
        {
            log.tag = (byte)TagType.LogEffectDestory;
            log.buffCheck = m_buffCheck;
            log.version = sourceBuff.version;
            log.isDestroyed = sourceBuff.destroyed;
        });
    }

    public override void OnUpdate(int diff)
    {
        if (!enableUpdate) return;
        base.OnUpdate(diff);

        if (firstStep)
        {
            var direction = randomPosition - position_;
            direction.Normalize();
            var p = position_ + direction * (velocity + acceleration * m_time * 0.001) * diff * 0.001;
            if (p.y < 0) p.y = 0;
            position = position_ = p; ;
            if (Vector3_.Distance(position_, randomPosition) < 0.2 || Vector3_.Dot(startPos - randomPosition, position_ - randomPosition) <= 0)
            {
                enableUpdate = false;
                firstStep = false;
            }
            return;
        }

        var targetPos = (Vector3_) target.LogicPosition + offset;
        position = position_ = motion.Evaluate(diff * 0.001);
        var d = Vector3_.Distance(position_, targetPos);
        var a = Vector3_.Dot(startPos - targetPos, position_ - targetPos);

        FightRecordManager.RecordLog<LogEffectPos>(log =>
        {
            log.tag = (byte)TagType.EffectPos;
            log.pos = new double[3];
            log.pos[0] = position_.x;
            log.pos[1] = position_.y;
            log.pos[2] = position_.z;
            log.distance = d;
            log.angle = a;
        });


        if ( d < 0.2 || a <= 0)
        {
            Destroy();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DispatchEvent(FOLLOW_EFFECT_DESTORY, Event_.Pop(conditionValue));

        FightRecordManager.RecordLog<LogEmpty>(log =>
        {
            log.tag = (byte)TagType.FollowEffectDestory;
        });

#if AI_LOG
        Module_AI.LogBattleMsg(source, "FollowTargetEffect[logId: {0}] destoryed with pos {0}", logId, position_);
#endif
    }

    public Vector3_ LogicPosition
    {
        get { return position_; }
        set { position_ = value; }
    }
}