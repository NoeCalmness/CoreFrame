using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EvolveMaterial = EvolveEquipInfo.EvolveMaterial;

public class PreviewMatItem : MonoBehaviour
{
    public enum EnumSelectType
    {
        Disselectable, //don't select
        Selectable,
    }

    public PItem item { get; private set; }
    private PropItemInfo m_propInfo;
    public EnumSelectType selectType { get; private set; }

    private Button m_selectBtn;
    private Button m_delBtn;
    private GameObject m_itemObj;
    private Image m_icon;

    private Action<PreviewMatItem> onSelectClick { get; set; }
    private Action<PreviewMatItem> onDeleteClick { get; set; }

    private Text m_countText;
    
    private bool m_init = false;

    public void Awake()
    {
        InitComponent();
    }

    private void InitComponent()
    {
        if (m_init) return;
        m_init = true;

        Transform t = transform.Find("btn");
        selectType = t == null ? EnumSelectType.Disselectable : EnumSelectType.Selectable;
        if(selectType == EnumSelectType.Selectable)
        {
            m_itemObj = transform.Find("item").gameObject;
            m_selectBtn = t.GetComponent<Button>();
            if (m_selectBtn)
            {
                m_selectBtn.onClick.RemoveAllListeners();
                m_selectBtn.onClick.AddListener(OnSelectClick);
            }
        }
        else
        {
            m_itemObj = gameObject;
        }

        m_delBtn = m_itemObj.transform.GetComponent<Button>("cancel_btn");
        if (m_delBtn)
        {
            m_delBtn.onClick.RemoveAllListeners();
            m_delBtn.onClick.AddListener(OnDelClick);
        }
        m_countText = m_itemObj.transform.GetComponent<Text>("numberdi/count");
        m_icon = m_itemObj.transform.GetComponent<Image>("icon");
    }

    public void InitSelectableExpItem()
    {
        InitComponent();
        gameObject.SetActive(true);
        m_selectBtn?.gameObject.SetActive(true);
        m_itemObj?.SetActive(false);
        item = null;
    }

    public void RefreshSelectableExpItem(PItem data, int number)
    {
        InitComponent();
        item = data;
        if (item == null)
        {
            InitSelectableExpItem();
            return;
        }

        m_propInfo = item?.GetPropItem();
        Util.SetItemInfo(m_itemObj, m_propInfo, 0, 0, false);
        Util.SetText(m_countText, Util.Format(ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI,17), number));
        m_delBtn?.gameObject.SetActive(number > 0);
        m_selectBtn?.gameObject.SetActive(number <= 0);
        m_itemObj?.gameObject.SetActive(number > 0);
    }

    public void RegisterCallback(Action<PreviewMatItem> select, Action<PreviewMatItem> delete)
    {
        onSelectClick = select;
        onDeleteClick = delete;
    }

    private void OnSelectClick()
    {
        onSelectClick?.Invoke(this);
    }

    private void OnDelClick()
    {
        if (m_selectBtn) m_selectBtn.gameObject.SetActive(true);
        m_itemObj.SetActive(false);
        onDeleteClick?.Invoke(this);
    }

    public void RefreshDisselectableExpItem(EvolveMaterial item,uint totalCount)
    {
        InitComponent();
        m_propInfo = ConfigManager.Get<PropItemInfo>(item.propId);
        if (m_propInfo == null) return;

        Util.SetItemInfo(m_itemObj, m_propInfo, 0,0,false);
        bool enough = totalCount >= item.num;
        string totalStr = enough ? totalCount.ToString() : GeneralConfigInfo.GetNoEnoughColorString(totalCount.ToString());

        Util.SetText(m_countText, ConfigText.GetDefalutString(TextForMatType.EquipIntentyUI, 16), totalStr, item.num);
        if (m_icon) EventTriggerListener.Get(m_icon.gameObject).onClick = (obj) => { if (m_propInfo) Module_Global.instance.SetTargetMatrial(m_propInfo.ID, (int)item.num); };
    }
}
