using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveInfo : MonoBehaviour
{

    private Image canon;
    private Image canyes;
    private Text leveltxt;
    private Button cangeton;
    private Button cangetyes;
    private Image getalready;
    private Action<int, int> awardno_show;
    private Action<int, int> awardyes_show;
    private int Cannum;
    private int Value = 1;
    private void Get()
    {
        canon = transform.Find("can_on").GetComponent<Image>();
        canyes = transform.Find("can_yes").GetComponent<Image>();
        leveltxt = transform.Find("level_text").GetComponent<Text>();
        cangeton = transform.Find("canget_no").GetComponent<Button>();
        cangetyes = transform.Find("canget_yes").GetComponent<Button>();
        getalready = transform.Find("canget_already").GetComponent<Image>();

    }
    public void Click(int txt, Action<int, int> award)
    {
        Get();
        Cannum = txt;
        awardno_show = award;
        awardyes_show = award;
    }

    public void Show(EnumActiveState state, int value)
    {
        leveltxt.text = value.ToString();
        canon.gameObject.SetActive(false);
        canyes.gameObject.SetActive(false);
        cangeton.gameObject.SetActive(false);
        cangetyes.gameObject.SetActive(false);
        getalready.gameObject.SetActive(false);
        cangeton.enabled = true;
        if (state == EnumActiveState.NotPick)
        {
            canon.gameObject.SetActive(true);
            cangeton.gameObject.SetActive(true);
        }
        else if (state == EnumActiveState.CanPick)
        {
            canyes.gameObject.SetActive(true);
            cangeton.gameObject.SetActive(true);
            cangetyes.gameObject.SetActive(true);

        }
        else if (state == EnumActiveState.AlreadPick)
        {
            canyes.gameObject.SetActive(true);
            cangeton.enabled = false;
            getalready.gameObject.SetActive(true);
        }

        //方法
        cangeton.onClick.RemoveAllListeners();
        cangetyes.onClick.RemoveAllListeners();
        cangeton.onClick.AddListener(Awardno_show);
        cangetyes.onClick.AddListener(Awardyes_show);

    }
    private void Awardno_show()
    {
        if (awardno_show != null)
        {
            awardno_show(Value, Cannum);
        }
    }
    private void Awardyes_show()
    {
        if (awardyes_show != null)
        {
            awardyes_show(Value, Cannum);
        }
    }
}
