using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShowEnhanceItem : EnhanceItem
{
    private Button uButton;
    private Image Images;
    private GameObject high;
    private Text IDID;
    public int ThisID;
    public int index;
    protected override void OnStart()
    {
        Images = transform.Find("Gary").GetComponent<Image>();
        uButton = GetComponent<Button>();
        high = transform.Find("checkon_background").gameObject;
        IDID = transform.Find("ID").GetComponent<Text>();
        uButton.onClick.AddListener(OnClickUGUIButton);
    }

    public void OnClickUGUIButton()
    {
        OnClickEnhanceItem();
    }

    protected override void SetItemDepth(float depthCurveValue, int depthFactor, float itemCount)
    {
        int newDepth = (int)(depthCurveValue * itemCount);
        this.transform.SetSiblingIndex(newDepth);
    }
    public override void SetSelectState(bool isCenter)
    {
        if (high == null)
            high = transform.Find("checkon_background").gameObject;
        if (IDID == null)
            IDID = transform.Find("ID").GetComponent<Text>();

        high.gameObject.SetActive(false);
        if (isCenter)
        {
            high.gameObject.SetActive(isCenter);
            if (!string.IsNullOrEmpty(IDID.text) && IDID.text != "0")
            {
                ThisID = int.Parse(IDID.text);
                //Module_Forging.instance.ChooseChange(index);
            }
        }
    }
    public override void SetSelectGray(bool isGray)
    {
        if (Images == null)
            Images = transform.Find("Gary").GetComponent<Image>();
        Images.gameObject.SetActive(true);
        Images.gameObject.SetActive(!isGray);
    }
}
