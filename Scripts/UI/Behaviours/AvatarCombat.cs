/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Behaviour script for combat window player avatar.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-10
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AvatarCombat : MonoBehaviour
{
    [Header("Tween Timer")]
    public float healthTween      = 0.2f;
    public float healthDelayTween = 0.3f;
    public float healthDelayStart = 1.0f;
    public float rageTween        = 0.2f;
    public float effectOffset     = -15.0f;
    public float effectFix        = 10.0f;

    [Header("Components"), Space(15)]
    public Text  playerName            = null;
    public Image healthBar             = null;
    public Image rageBar               = null;
    public Image healthBarDelay        = null;
    public Button blood                = null;
    public Text bulletCount            = null;
    public RectTransform effectNode    = null;
    public Image avatar                = null;
    public Transform petIcon           = null;
    public Image enermyHealthBar       = null;
    public Image enermyHealthBarDelay  = null;
    public Image avatarBoxBg           = null;
    public Image avatarBoxMask         = null;
    public GameObject[] rageEffects    = null;
    public Transform buffRoot;
    public Transform buffTemp;

    private RectTransform m_ep;
    private GameObject m_bullet;
    private Tweener m_healthTweener = null, m_healthTweenerDelay = null, m_rageTweener = null;
    private Sprite m_selfHealthBar, m_enermyHealthBar, m_selfHealthBarDelay, m_enermyHealthBarDelay;
    private readonly Dictionary<int, BuffEffectBehaviour> m_buffBehaviourCache = new Dictionary<int, BuffEffectBehaviour>();

    private int m_rageCount = -1;
    private float m_lastRage = 0;

    public Creature creature
    {
        get { return m_creature; }
        set
        {
            if (m_creature == value) return;
            if (m_creature)
            {
                ClearBuffIcon();
                m_creature.RemoveEventListener(this);
            }

            m_creature = value;

            ResetToCreature();

            if (m_creature)
            {
                m_creature.AddEventListener(CreatureEvents.BULLET_COUNT_CHANGED, OnBulletCountChanged);
                m_creature.AddEventListener(CreatureEvents.HEALTH_CHANGED,       OnHealthChanged);
                m_creature.AddEventListener(CreatureEvents.RAGE_CHANGED,         OnRageChanged);
                m_creature.AddEventListener(CreatureEvents.BUFF_CREATE,          OnBuffCreate);
                m_creature.AddEventListener(CreatureEvents.BUFF_TRIGGER,         OnBuffTrigger);
                m_creature.AddEventListener(CreatureEvents.BUFF_REMOVE,          OnBuffRemove);

                InitBuffIcon();
            }
        }
    }
    private Creature m_creature;

    private void ClearBuffIcon()
    {
        m_buffBehaviourCache.Clear();
        Util.ClearChildren(buffRoot);
    }

    private void InitBuffIcon()
    {
        ClearBuffIcon();
        var buffs = m_creature.GetBuffList();
        foreach (var buff in buffs)
        {
            OnBuffCreate(Event_.Pop(buff));
        }
    }

    private void OnBuffCreate(Event_ e)
    {
        if (!buffRoot || !buffTemp)
            return;

        var buff = e.param1 as Buff;
        if (null == buff) return;

        if (string.IsNullOrEmpty(buff.info.icon))
            return;

        var key = buff.info.ID;
        if (m_buffBehaviourCache.ContainsKey(key))
        {
            m_buffBehaviourCache[key].Overlap(buff);
            return;
        }

        var t = buffRoot.AddNewChild(buffTemp);
        var b = t.GetComponentDefault<BuffEffectBehaviour>();
        b.Init(buff, e.sender as Creature);
        t.SafeSetActive(true);
        b.onClick = (rBuff) =>
        {
            foreach (var kv in m_buffBehaviourCache)
            {
                kv.Value.ShowDesc(rBuff);
            }
        };

        if (t && t.gameObject)
            m_buffBehaviourCache.Add(key, b);
    }

    private void OnBuffTrigger(Event_ e)
    {
        var buff = e.param1 as Buff;
        if (null == buff) return;
        var key = buff.info.ID;
        if (m_buffBehaviourCache.ContainsKey(key))
            m_buffBehaviourCache[key].OnTrigger();
    }

    private void OnBuffRemove(Event_ e)
    {
        var buff = e.param1 as Buff;
        if (null == buff) return;

        var key = buff.info.ID;
        if (m_buffBehaviourCache.ContainsKey(key))
        {
            var effectBehaviour = m_buffBehaviourCache[key];
            if (effectBehaviour)
            {
                if(effectBehaviour.Destory())
                    m_buffBehaviourCache.Remove(key);
            }
        }
    }

    public void ResetToCreature()
    {
        if (m_healthTweener != null) m_healthTweener.Kill();
        if (m_rageTweener != null) m_rageTweener.Kill();
        if (m_healthTweenerDelay != null) m_healthTweenerDelay.Kill();

        m_healthTweener = null;
        m_rageTweener = null;
        m_healthTweenerDelay = null;

        if (!m_creature)
        {
            healthBar.sprite = m_selfHealthBar;
            healthBarDelay.sprite = m_selfHealthBarDelay;

            playerName.text = string.Empty;
            healthBar.fillAmount = 0;
            healthBarDelay.fillAmount = 0;

            m_rageCount = -1;
            m_lastRage = 0;

            blood.gameObject.SetActive(false);
            m_bullet.SetActive(false);
            avatar.enabled = false;
        }
        else
        {
            healthBar.sprite = m_creature.isPlayer ? m_selfHealthBar : m_enermyHealthBar;
            healthBarDelay.sprite = m_creature.isPlayer ? m_selfHealthBarDelay : m_enermyHealthBarDelay;
            playerName.text = m_creature.uiName;
            healthBar.fillAmount = m_creature.healthRate;
            healthBarDelay.fillAmount = m_creature.healthRate;

            blood.gameObject.SetActive(false);
            m_bullet.SetActive(m_creature.isPlayer);
            avatar.enabled = true;

            avatar.transform.localEulerAngles = new Vector3(0, m_creature.isMonster || m_creature.avatar.StartsWith(Module_Avatar.npcAvatarPrefix) ? 180 : 0, 0);

            m_lastRage = m_creature.isPlayer ? m_creature.rageRate : 0;
            m_rageCount = (int)(m_lastRage / 0.33f) - 1;

            OnBulletCountChanged();

            if (petIcon)
            {
                if (m_creature.pet)
                {
                    petIcon.gameObject.SetActive(true);
                    Util.SetPetSimpleInfo(petIcon, m_creature.pet.petInfo);
                }
                else petIcon.gameObject.SetActive(false);
            }
        }

        UpdateRageBar();
        UpdateRageEffect();
        UpdateEffectPosition();
        UpdateAvatar();
        UpdateAvatarBox();
    }

    private void OnBulletCountChanged()
    {
        Util.SetText(bulletCount, 9202, Level.currentLevel == GeneralConfigInfo.sTrainLevel ? 0 : m_creature.bulletCount < 1 ? 2 : 1, m_creature.bulletCount);
    }

    private void OnHealthChanged(Event_ e)
    {
        if (m_healthTweener != null) m_healthTweener.ChangeValues(healthBar.fillAmount, m_creature.healthRate, healthTween).PlayForward();
        m_healthTweener = healthBar.DOFillAmount(m_creature.healthRate, healthTween).SetEase(Ease.OutQuad).OnUpdate(UpdateEffectPosition).SetAutoKill(false);

        if ((bool)e.param3) UpdateEffectPosition();

        TweenHealthDelay();
    }

    private void OnRageChanged()
    {
        if (!m_creature || !m_creature.isPlayer) return;

        var rage  = m_creature.rageRate;
        var count = (int)(rage / 0.33f) - 1;

        if (m_rageCount > count) m_lastRage = rage;

        if (m_rageTweener != null) m_rageTweener.ChangeValues(m_lastRage, rage, rageTween).PlayForward();
        else m_rageTweener = DOTween.To(() => m_lastRage, v => m_lastRage = v, rage, rageTween).OnUpdate(UpdateRageBar).SetAutoKill(false);

        if (m_rageCount != count)
        {
            m_rageCount = count;
            UpdateRageEffect();
        }
    }

    private void UpdateRageBar()
    {
        var p = m_lastRage % 0.33f / 0.33f;
        rageBar.fillAmount = m_rageCount > 0 && p < 0.033f ? 1.0f : p;
    }

    private void UpdateRageEffect()
    {
        if (rageEffects == null || rageEffects.Length < 1) return;
        for (int i = 0, c = rageEffects.Length; i < c; ++i) rageEffects[i].SetActive(i <= m_rageCount);
    }

    private void UpdateEffectPosition()
    {
        if (!m_ep) return;

        effectNode.anchoredPosition = new Vector2((m_ep.sizeDelta.x + effectOffset) * healthBar.fillAmount + effectFix, effectNode.anchoredPosition.y);
    }

    private void UpdateAvatar()
    {
        if (!avatar || !m_creature) return;
        Module_Avatar.SetClassAvatar(avatar.gameObject, m_creature);
    }

    private void UpdateAvatarBox()
    {
        if (!m_creature || !avatarBoxBg && !avatarBoxMask) return;

        if (m_creature.isMonster)  // Use static atlas
        {
            if (avatarBoxBg) AtlasHelper.SetShared(avatarBoxBg, Creature.GetStaticAvatarBox(m_creature.avatarBox));
            if (avatarBoxMask) AtlasHelper.SetShared(avatarBoxMask, string.Empty);
            return;
        }

        var box = ConfigManager.Get<PropItemInfo>(m_creature.avatarBox == 0 ? Creature.DEFAULT_AVATAR_BOX : m_creature.avatarBox);
        if (!box) return;

        if (avatarBoxBg)   AtlasHelper.SetShared(avatarBoxBg,   box.mesh.Length < 1 ? string.Empty : box.mesh[0]);
        if (avatarBoxMask) AtlasHelper.SetShared(avatarBoxMask, box.mesh.Length < 2 ? string.Empty : box.mesh[1]);
    }

    private void TweenHealthDelay()
    {
        if (!m_creature) return;

        var target = m_creature.healthRate;
        if (healthBarDelay.fillAmount < target)
        {
            if (m_healthTweenerDelay != null) m_healthTweenerDelay.Complete();
            healthBarDelay.fillAmount = target;
            return;
        }
        if (m_healthTweenerDelay != null) m_healthTweenerDelay.ChangeValues(healthBarDelay.fillAmount, target, healthDelayTween).SetDelay(healthDelayStart).Restart();
        else m_healthTweenerDelay = healthBarDelay.DOFillAmount(target, healthDelayTween).SetDelay(healthDelayStart).SetEase(Ease.OutQuad).SetAutoKill(false);
    }

    private void ResetTime()
    {
        healthTween      = CombatConfig.shealthBarTween;
        healthDelayTween = CombatConfig.shealthBarDelayTween;
        healthDelayStart = CombatConfig.shealthDelayStart;
        rageTween        = CombatConfig.srageBarTween;
    }

    private void Awake()
    {
        #if UNITY_EDITOR
        if (!Game.started) return;
        #endif

        m_bullet = bulletCount.transform.parent.gameObject;
        m_ep = effectNode?.parent?.parent?.rectTransform();

        m_selfHealthBar        = healthBar.sprite;
        m_enermyHealthBar      = enermyHealthBar.sprite;
        m_selfHealthBarDelay   = healthBarDelay.sprite;
        m_enermyHealthBarDelay = enermyHealthBarDelay.sprite;

        ResetTime();
        ResetToCreature();
    }

    private void OnDestroy()
    {
        EventManager.RemoveEventListener(this);

        if (m_rageTweener != null) m_rageTweener.Kill();
        if (m_healthTweenerDelay != null) m_healthTweenerDelay.Kill();
        if (m_healthTweener != null) m_healthTweener.Kill();

        m_healthTweenerDelay = null;
        m_healthTweener = null;
        m_rageTweener = null;
        m_creature = null;
        m_bullet = null;
        m_ep = null;

        healthBar.sprite            = m_selfHealthBar;
        healthBarDelay.sprite       = m_selfHealthBarDelay;

        m_selfHealthBar        = null;
        m_enermyHealthBar      = null;
        m_selfHealthBarDelay   = null;
        m_enermyHealthBarDelay = null;

        enermyHealthBar        = null;
        playerName             = null;
        healthBar              = null;
        rageBar                = null;
        healthBarDelay         = null;
        blood                  = null;
        bulletCount            = null;
        effectNode             = null;
        avatar                 = null;
        rageEffects            = null;
        enermyHealthBar        = null;
        enermyHealthBarDelay   = null;
        avatarBoxBg            = null;
        avatarBoxMask          = null;
    }

    #region Editor helper

    private void Start()
    {
        buffTemp.SafeSetActive(false);
#if UNITY_EDITOR
        EventManager.AddEventListener("EditorReloadConfig", OnEditorReloadConfig);
#endif
    }
#if UNITY_EDITOR

    private void OnEditorReloadConfig(Event_ e)
    {
        var config = (string)e.param1;

        if (config == "config_combatconfigs") ResetTime();
        if (config == "config_configtexts")   OnBulletCountChanged();
        
    }
#endif

    #endregion
}
