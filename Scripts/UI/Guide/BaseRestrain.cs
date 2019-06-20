using UnityEngine;
using System;

/// <summary>
/// base class of guide's restrain 
/// </summary>
public class BaseRestrain : MonoBehaviour
{
    #region event
    public Action<BaseRestrain> onCheckGuideDisable;
    #endregion

    #region static functions

    public static void SetRestrainData(GameObject o, PropItemInfo itemInfo, int level = 0, int star = 0)
    {
        if (!o)
        {
            Logger.LogError("set restrain data is wrong,gameObject is null");
            return;
        }

        if (!itemInfo)
        {
            Logger.LogError("set restrain data is wrong,PropItemInfo is null");
            return;
        }
        var component = o.GetComponentDefault<BaseRestrain>();
        SetRestrainData(component, itemInfo.ID,level,star);
    }

    public static void SetRestrainData(GameObject o, int id, int level = 0, int star = 0)
    {
        if (!o)
        {
            Logger.LogError("set restrain data is wrong,gameObject is null");
            return;
        }

        var component = o.GetComponentDefault<BaseRestrain>();
        SetRestrainData(component, id, level, star);
    }

    public static void SetRestrainData(BaseRestrain component, int id, int level = 0, int star = 0)
    {
        component.level = level;
        component.star = star;
        component.restrainId = id;
    }

    #endregion

    #region restrain property
    [SerializeField]
    private int m_restrainId;

    public int restrainId
    {
        get { return m_restrainId; }

        set
        {
            if (value == m_restrainId) return;

            m_restrainId = value;
            //we need know the restrainId is changed
            onCheckGuideDisable?.Invoke(this);
            if (Module_Guide.instance) Module_Guide.instance.DispatchCheckGuideEnable(this);
        }
    }
    
    public int level { get; set; }
    private int star { get; set; }
    
    private RectTransform m_rectTransform = null;
    public RectTransform rectTransform
    {
        get
        {
            if (!m_rectTransform) m_rectTransform = transform as RectTransform;
            return m_rectTransform;
        }
    }
    #endregion

    #region virtual functions
    protected virtual void OnDestory()
    {
        restrainId = 0;
    }
    #endregion

    #region public functions

    public virtual bool SameData(int id)
    {
        return m_restrainId == id;
    }

    public virtual bool SameData(int[] runeData)
    {
        if (runeData == null || runeData.Length == 0) return false;
        return m_restrainId == runeData.GetValue<int>(0) && level == runeData.GetValue<int>(1) && star == runeData.GetValue<int>(2);
    }

    #endregion
}
