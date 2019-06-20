using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AwardTip : MonoBehaviour
{
    private Text m_award1;
    private Text m_award2;
    private GameObject m_awardobj1;
    private GameObject m_awardobj2;
    
    List<string> namenum = new List<string>(); //文字和num的拼接
    List<PropItemInfo> Info = new List<PropItemInfo>(); //图片名称
    List<Color> m_color = new List<Color>(); //颜色
    List<int> start = new List<int>(); //颜色
    // Use this for initialization
    private void Awake()
    {
        namenum.Clear();
        Info.Clear();
        m_color.Clear();
        start.Clear();
        m_award1 = transform.Find("Text1").GetComponent<Text>();
        m_award2 = transform.Find("Text2").GetComponent<Text>();
        m_awardobj1 = transform.Find("item_01").gameObject;
        m_awardobj2 = transform.Find("item_02").gameObject;

    }

    public void lengthAdd(string name, PropItemInfo info, Color col, int star)
    {
        namenum.Add(name);
        Info.Add(info);
        m_color.Add(col);
        start.Add(star);
    }

    public void showdata()
    {
        m_awardobj1.gameObject.SetActive(true);
        m_awardobj2.gameObject.SetActive(false);

        Util.SetItemInfo(m_awardobj1, Info[0], 0, 0, false, start[0]);

        m_award1.gameObject.SetActive(true);
        if (namenum.Count > 0 && m_color.Count > 0)
        {
            Util.SetText(m_award1, namenum[0]);
        }
        m_award1.color = m_color[0];

        if (namenum.Count > 1 && m_color.Count > 1)
        {
            m_awardobj2.gameObject.SetActive(true);
            Util.SetItemInfo(m_awardobj2, Info[1], 0, 0, false, start[1]);

            m_award2.gameObject.SetActive(true);
            Util.SetText(m_award2, namenum[1]);
            m_award2.color = m_color[1];
        }
        else if (namenum.Count == 1)
        {
            m_awardobj1.transform.localPosition = new Vector3(-70, 0, 0);
            m_award1.transform.localPosition = new Vector3(80, 0, 0);
            m_award2.gameObject.SetActive(false);
        }
    }

    public void Anima(float height)
    {
        Sequence a = DOTween.Sequence();
        GeneralConfigInfo generalInfo = GeneralConfigInfo.defaultConfig;
        if (generalInfo == null)
        {
            Logger.LogError("GeneralConfigInfo awardtip null");
            return;
        }
        a.Insert(generalInfo.awd_moveup_strat, transform.DOLocalMoveY(height + generalInfo.awd_moveup_all_dis, generalInfo.awd_moveup_all));//等待时间
        CanvasGroup group = transform.GetComponent<CanvasGroup>();
        a.Insert(generalInfo.awd_hidden_start, DOTween.To(() => group.alpha, x => group.alpha = x, 0, generalInfo.awd_hidden_all));//开始时间 //渐隐时间

        a.OnComplete(() =>
        {
            transform.localPosition = new Vector3(0, 0, 0);
            m_awardobj1.transform.localPosition = new Vector3(-250, 0, 0);
            m_award1.transform.localPosition = new Vector3(-125, 0, 0);
            DOTween.To(() => group.alpha, x => group.alpha = x, 1, 0);
            transform.gameObject.SetActive(false);
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
                Info.Clear();
                namenum.Clear();
                m_color.Clear();
            }
        });
    }
}
