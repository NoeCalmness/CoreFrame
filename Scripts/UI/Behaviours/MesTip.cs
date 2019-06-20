using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MesTip : MonoBehaviour
{
    bool istrue = true;
    float valuetime = 0;
    GeneralConfigInfo generalInfo;
    
    public void Ins_Mesplay()
    {
        generalInfo = ConfigManager.Get<GeneralConfigInfo>(0);
        istrue = false;
    }

    void Update()
    {
        if (!istrue)
        {
            valuetime += Time.unscaledDeltaTime;
            if (valuetime > generalInfo.mes_moveup_strat)
            {
                istrue = true;
                Sequence aa = DOTween.Sequence();
                aa.SetUpdate(true);
                float ms = transform.localPosition.y;
                aa.Insert(0, transform.DOLocalMoveY(ms + generalInfo.mes_moveup_all_dis, generalInfo.mes_moveup_all));//上浮速度
                CanvasGroup group = transform.GetComponent<CanvasGroup>();
                aa.Insert(generalInfo.mes_hidden_start, DOTween.To(() => group.alpha, x => group.alpha = x, 0, generalInfo.mes_hidden_all));
                aa.OnComplete(() =>
                {
                    GameObject.Destroy(gameObject);
                });
            }
        }
    }
}
