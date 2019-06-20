using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Delaypaomadeng : MonoBehaviour
{

    private RectTransform not_pos;
    private Chathyperlink text_notice;

    bool istrue = true;
    float times = 0;
    float Maxtime = 0;
    float m_width = 0;

    private void Get()
    {
        not_pos = gameObject.GetComponent<RectTransform>();
        text_notice = gameObject.GetComponent<Chathyperlink>();
    }

    public void Delay(string notice)
    {
        Get();
        times = 0;

        text_notice.text = notice;
        text_notice.Set();
        text_notice.text = text_notice.gettxt;
        m_width = text_notice.preferredWidth;

        not_pos.anchoredPosition = Vector3.zero;//固定起始位置
        GeneralConfigInfo generalInfo = GeneralConfigInfo.defaultConfig;
        float multiple = (m_width + generalInfo.strat_diatance) / generalInfo.move_diatance;
        float thistime = multiple * generalInfo.move_time;

        Vector3 newvalue = Vector3.zero;
        Tweener tween = not_pos.DOLocalMoveX(-(m_width + generalInfo.strat_diatance), thistime);//公告移动速度
        tween.SetUpdate(true);
        tween.SetEase(Ease.Linear);

        Maxtime = (m_width / (m_width + generalInfo.strat_diatance)) * thistime;
        istrue = false;
        tween.OnComplete(() =>
        {
            if (!istrue)
            {
                istrue = true;
                times = 0;
                Module_Global.instance.DelayNext();
            }
            Module_Global.instance.CloseNotice();
            GameObject.Destroy(gameObject);
        });
    }

    void Update()
    {
        if (!istrue)
        {
            times += Time.unscaledDeltaTime;
            if (times >= Maxtime)
            {
                istrue = true;
                times = 0;
                Module_Global.instance.DelayNext();
            }
        }
    }
}
