// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-04-08      14:25
//  *LastModify：2019-04-08      14:25
//  ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class DamageHUD : MonoBehaviour
{
    public const int MAX_DAMAGE_NUMBER_COUNT = 20;
    private readonly Queue<DamageNumber> m_damageNumbers = new Queue<DamageNumber>();

    private void Awake()
    {
        ClearDamageNum();
        GameObject obj = Level.GetPreloadObject<GameObject>(Level_Battle.BATTLE_UI_ASSET_NAME[0], false);
        if (obj == null)
        {
            Logger.LogError("cannot load {0} ,because there is no valid preloaded_asset with name {0}", Level_Battle.BATTLE_UI_ASSET_NAME[0]);
            return;
        }
        Camera battleCamera = Level.current.mainCamera;
        Camera uiCamera = UIManager.worldCamera;
        
        for (int i = 0; i < MAX_DAMAGE_NUMBER_COUNT; i++)
        {
            Transform t = transform.AddNewChild(obj);
            t.gameObject.name = $"damage_{i:d2}";
            var dm = t.GetComponentDefault<DamageNumber>();
            if (dm)
            {
                dm.InitCamera(battleCamera, uiCamera);
                m_damageNumbers.Enqueue(dm);
            }
            t.gameObject.SetActive(false);
        }

        AddEventListener();
    }

    private void AddEventListener()
    {
        EventManager.AddEventListener(CreatureEvents.TAKE_DAMAGE, OnCreatureTakeDamage);
        EventManager.AddEventListener(CreatureEvents.DEAL_UI_DAMAGE, OnCreatureDealDamage);
        EventManager.AddEventListener(CreatureEvents.DEAL_DAMAGE, OnCreatureDealDamage);
    }

    private void OnDestroy()
    {
        EventManager.RemoveEventListener(this);
    }

    private void OnCreatureDealDamage(Event_ e)
    {
        var sender = e.sender as Creature;
        var c = e.param1 as Creature;
        var damage = e.param2 as DamageInfo;
        if (!damage || !sender || !sender.isPlayer) return;

        ShowDamageNumber(c, damage);
    }

    private void OnCreatureTakeDamage(Event_ e)
    {
        var sender = e.sender as Creature;
        var damage = e.param2 as DamageInfo;
        if (!sender || !sender.isPlayer || !damage || damage.buffEffectFlag == BuffInfo.EffectFlags.Unknow)
            return;

        ShowDamageNumber(sender, damage);
    }

    private void ShowDamageNumber(Creature creature, DamageInfo info)
    {
        bool isBuff = info.buffEffectFlag != BuffInfo.EffectFlags.Unknow;
        //to player,we only need show buff damage
        if (creature.isPlayer && !isBuff) return;

        var dm = GetValidDamageNumber();
        if (!dm)
        {
            Logger.LogError("cannot find a valid DamageNumber Componnet!! ");
            return;
        }
        dm.ResetTransPosition(creature, isBuff);
        dm.ShowDamageNumber(info);
    }

    private DamageNumber GetValidDamageNumber()
    {
        if (m_damageNumbers == null || m_damageNumbers.Count == 0)
            return null;
        var component = m_damageNumbers.Dequeue();
        component.rectTransform.SetAsLastSibling();
        //reycle the component
        m_damageNumbers.Enqueue(component);
        return component;
    }

    public void ClearDamageNum()
    {
        while (m_damageNumbers.Count > 0)
        {
            var dm = m_damageNumbers.Dequeue();
            if (dm) Object.Destroy(dm.gameObject);
        }

        m_damageNumbers.Clear();
    }
}
