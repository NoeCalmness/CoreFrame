// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-03-11      18:23
//  *LastModify：2019-03-11      18:23
//  ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window_HpSlider : Window
{
    public static UIFollowTarget GetFollowScript(Creature rCreature)
    {
        var w = GetOpenedWindow<Window_HpSlider>();
        if (w == null || rCreature == null) return null;
        Entry f = null;
        if (!w.cache.TryGetValue(rCreature, out f))
        {
            f = w.CreateHealthBar(rCreature, false);
        }
        f?.follow?.UpdateFrame();
        return f?.follow;
    }

    public Transform templete;

    private readonly Dictionary<Creature, Entry> cache = new Dictionary<Creature, Entry>(); 

    protected override void OnOpen()
    {
        isFullScreen = false;
        templete = GetComponent<Transform>("templete");

        templete.SafeSetActive(false);
        EventManager.AddEventListener(CreatureEvents.SET_HEALTH_BAR_VISIABLE_UI  , OnSetHpBarVisiable);
        EventManager.AddEventListener(CreatureEvents.HEALTH_CHANGED              , OnHealthChange);
        EventManager.AddEventListener(Events.ON_DESTROY                          , OnObjectDestory);
        EventManager.AddEventListener(Events.ON_VISIBILITY_CHANGED               , OnObjectVisiableChanged);
    }

    private void OnObjectVisiableChanged(Event_ e)
    {
        OnSetHpBarVisiable(e);
    }

    private void OnObjectDestory(Event_ e)
    {
        var c = e.sender as Creature;
        if (null == c) return;
        RemoveHpSlider(c);
    }


    protected override void OnClose()
    {
        EventManager.RemoveEventListener(this);
    }

    protected override void OnReturn()
    {

    }


    protected override void OnBecameVisible(bool oldState, bool forward)
    {
        base.OnBecameVisible(oldState, forward);

        cache.Clear();
        Util.ClearChildren(transform);

        ObjectManager.Foreach<Creature>(c =>
        {
            if (c.hpVisiableCount > 0)
            {
                CreateHealthBar(c, c.hpVisiable);
            }
            return true;
        });
    }

    protected override void OnHide(bool forward)
    {
        base.OnHide(forward);
        cache.Clear();
        Util.ClearChildren(transform);
    }

    private void OnSetHpBarVisiable(Event_ e)
    {
        var c = e.sender as Creature;

        if (!c) return;
        Entry entry = null;
        if (cache.TryGetValue(c, out entry))
        {
            entry.follow.SafeSetActive(c.hpVisiable);
        }
        else if(c.hpVisiableCount > 0)
            CreateHealthBar(c, c.hpVisiable);
    }

    private void OnHealthChange(Event_ e)
    {
        var c = e.sender as Creature;

        if (!c) return;

        Entry entry = null;
        if (cache.TryGetValue(c, out entry))
        {
            if(entry.slider)
                entry.slider.value = c.healthRate;
        }
    }

    private Entry CreateHealthBar(Creature c, bool rVisiable = true)
    {
        if (cache.ContainsKey(c))
            return cache[c];

        var t = transform.AddNewChild(templete);

        var node = Util.FindChild(c.transform, "Bip001 Head");
        var offset = new Vector3(0, 60, 0);
        if (!node)
        {
            offset = new Vector3(0, 300, 0);
            node = c.transform;
        }
        
        if (node == null)
            return null;
        var monster = c as MonsterCreature;
        if (null != monster && monster.monsterInfo != null)
        {
            offset.x += monster.monsterInfo.bloodOffset.x;
            offset.y += monster.monsterInfo.bloodOffset.y;
        }
        var f = UIFollowTarget.Start(t, node);
        t.SafeSetActive(rVisiable);
        f.offset = offset;
        t.name = c.uiName;

        Entry entry = new Entry
        {
            follow = f,
            slider = t.GetComponent<Slider>("slider"),
            healthImage = t.GetComponent<Image>("slider/Fill Area/Fill")
        };

        
        if (null != monster)
        {
            var r = entry.slider.rectTransform();

            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, monster.monsterInfo?.bloodBarSize.x == 0 ? r.rect.size.x : monster.monsterInfo.bloodBarSize.x);
            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, monster.monsterInfo?.bloodBarSize.y == 0 ? r.rect.size.y : monster.monsterInfo.bloodBarSize.y);
        }

        entry.slider.value = c.healthRate;
        entry.healthImage.color = c.creatureCamp == CreatureCamp.MonsterCamp
            ? GeneralConfigInfo.monsterHealthColor
            : GeneralConfigInfo.playerHealthColor;

        cache.Set(c, entry);
        return entry;
    }

    private void RemoveHpSlider(Creature c)
    {
        Entry entry = null;
        if (cache.TryGetValue(c, out entry))
        {
            if (entry.follow)
                GameObject.Destroy(entry.follow.gameObject);
            cache.Remove(c);
        }
    }

    public class Entry
    {
        public Slider slider;

        public Image healthImage;

        public UIFollowTarget follow;
    }
}
