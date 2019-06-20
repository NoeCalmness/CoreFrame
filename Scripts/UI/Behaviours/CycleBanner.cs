using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CycleBanner : UIBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// 滚动方向H or V
    /// </summary>
    public enum AxisType
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// 图片轮播方向
    /// </summary>
    public enum LoopDirType
    {
        RightOrUp = -1,
        LeftOrDown = 1,
    }

    /// <summary>
    /// 子物体size
    /// </summary>
    public Vector2 cellSize;

    /// <summary>
    /// 子物体间隔
    /// </summary>
    public Vector2 mSpacing;

    /// <summary>
    /// 页码
    /// </summary>
    public RectTransform mPage;

    /// <summary>
    /// 当前处于正中的元素
    /// </summary>
    public int CurrentIndex
    {
        get { return m_index; }
    }

    /// <summary>
    /// 方向
    /// </summary>
    public AxisType MoveAxisType;

    /// <summary>
    /// 轮播方向-- 1为向左移动，-1为向右移动
    /// </summary>
    public LoopDirType MLoopDirType;

    [Range(1, 500)]
    public int tweenStepNum;

    private float m_loopSpaceTime;

    private bool m_dragging = false;
    private bool m_isNormalizing = false;
    private int m_currentStep = 0;
    private Vector2 m_currentPos;
    private Vector2 m_prePos;
    private int m_index = 0, m_preIndex = 0;
    private RectTransform m_viewRect;
    private RectTransform m_header;
    private bool m_checkCache = true;

    private float m_currTimeDelta = 0;

    private bool m_upate = false;
    List<Toggle> m_pageList = new List<Toggle>();

    private float m_viewRectXMin
    {
        get
        {
            Vector3[] v = new Vector3[4];
            m_viewRect.GetWorldCorners(v);
            return v[0].x;
        }
    }
    private float m_viewRectXMax
    {
        get
        {
            Vector3[] v = new Vector3[4];
            m_viewRect.GetWorldCorners(v);
            return v[3].x;
        }
    }
    private float m_viewRectYMin
    {
        get
        {
            Vector3[] v = new Vector3[4];
            m_viewRect.GetWorldCorners(v);
            return v[0].y;
        }
    }
    private float m_viewRectYMax
    {
        get
        {
            Vector3[] v = new Vector3[4];
            m_viewRect.GetWorldCorners(v);
            return v[2].y;
        }
    }

    public int CellCount
    {
        get { return transform.childCount; }
    }
    public void SetBannerView()
    {
        m_upate = true;
        MLoopDirType = LoopDirType.LeftOrDown;
        m_loopSpaceTime = GeneralConfigInfo.defaultConfig.bannerInterval;

        m_pageList.Clear();
        foreach (Transform item in mPage)
        {
            var tog = item.GetComponentDefault<Toggle>();
            tog.isOn = false;
            m_pageList.Add(tog);
        }
        m_viewRect = GetComponent<RectTransform>();
        m_header = GetChild(m_viewRect, 0);

        SetPageIndex(0);
    }

    public void ResizeChildren()
    {
        Vector2 delta;
        if (MoveAxisType == AxisType.Horizontal) delta = new Vector2(cellSize.x + mSpacing.x, 0);
        else delta = new Vector2(0, cellSize.y + mSpacing.y);

        for (int i = 0; i < CellCount; i++)
        {
            var t = GetChild(m_viewRect, i);
            if (t)
            {
                t.localPosition = delta * i;
                t.sizeDelta = cellSize;
            }
        }
        m_isNormalizing = false;
        m_currentPos = Vector2.zero;
        m_currentStep = 0;
    }
    /// <summary>
    /// 加子物体到当前列表的最后面
    /// </summary>
    /// <param name="t"></param>
    public virtual void AddChild(RectTransform t)
    {
        if (t != null)
        {
            t.SetParent(m_viewRect, false);
            t.SetAsLastSibling();
            Vector2 delta;
            if (MoveAxisType == AxisType.Horizontal) delta = new Vector2(cellSize.x + mSpacing.x, 0);
            else delta = new Vector2(0, cellSize.y + mSpacing.y);

            if (CellCount == 0)
            {
                t.localPosition = Vector3.zero;
                m_header = t;
            }
            else t.localPosition = delta + (Vector2)GetChild(m_viewRect, CellCount - 1).localPosition;

        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        ResizeChildren();
        return;
        if (Application.isPlaying)
        {
            if (ContentIsLongerThanRect())
            {
                int s;
                do
                {
                    s = GetBoundaryState();
                    LoopCell(s);
                } while (s != 0);
            }
        }
    }

    protected virtual void Update()
    {
        if (!m_upate) return;

        if (ContentIsLongerThanRect())
        {
            //实现在必要时loop子元素
            if (Application.isPlaying)
            {
                int s = GetBoundaryState();
                LoopCell(s);
            }
            //缓动回指定位置
            if (m_isNormalizing && EnsureListCanAdjust())
            {
                if (m_currentStep == tweenStepNum)
                {
                    m_isNormalizing = false;
                    m_currentStep = 0;
                    m_currentPos = Vector2.zero;
                    return;
                }
                Vector2 delta = m_currentPos / tweenStepNum;
                m_currentStep++;
                TweenToCorrect(-delta);
            }
            //自动loop
            if (!m_isNormalizing && EnsureListCanAdjust())
            {
                m_currTimeDelta += Time.deltaTime;
                if (m_currTimeDelta > m_loopSpaceTime)
                {
                    m_currTimeDelta = 0;
                    MoveToIndex(m_index + (int)MLoopDirType);
                    Module_Home.instance.GetOpenBanner();
                }
            }
            //检测index是否变化
            if (MoveAxisType == AxisType.Horizontal) m_index = (int)(m_header.localPosition.x / (cellSize.x + mSpacing.x - 1));
            else m_index = (int)(m_header.localPosition.y / (cellSize.y + mSpacing.y - 1));

            if (m_index <= 0) m_index = Mathf.Abs(m_index);
            else m_index = CellCount - m_index;

            if (m_preIndex != m_index) SetPageIndex(m_index);
            m_preIndex = m_index;
        }
    }
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (!m_checkCache) return;

        Vector2 vector;
        if (((eventData.button == PointerEventData.InputButton.Left) && this.IsActive()) && RectTransformUtility.ScreenPointToLocalPointInRectangle(this.m_viewRect, eventData.position, eventData.pressEventCamera, out vector))
        {
            this.m_dragging = true;
            m_prePos = vector;
        }
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!m_checkCache) return;

        Vector2 vector;
        if (((eventData.button == PointerEventData.InputButton.Left) && this.IsActive()) && RectTransformUtility.ScreenPointToLocalPointInRectangle(this.m_viewRect, eventData.position, eventData.pressEventCamera, out vector))
        {
            m_isNormalizing = false;
            m_currentPos = Vector2.zero;
            m_currentStep = 0;
            Vector2 vector2 = vector - this.m_prePos;
            Vector2 vec = CalculateOffset(vector2);
            this.SetContentPosition(vec);
            m_prePos = vector;
        }
        m_currTimeDelta = 0;
    }
    /// <summary>
    /// 移动到指定索引
    /// </summary>
    /// <param name="index"></param>
    public virtual void MoveToIndex(int index)
    {
        if (m_isNormalizing) return;
        if (index == m_index) return;
        this.m_isNormalizing = true;
        Vector2 offset;
        if (MoveAxisType == AxisType.Horizontal) offset = new Vector2(cellSize.x + mSpacing.x, 0);
        else offset = new Vector2(0, cellSize.y + mSpacing.y);

        var delta = CalcCorrectDeltaPos();
        int vindex = m_index;
        m_currentPos = delta + offset * (index - vindex);
        m_currentStep = 0;

    }
    private Vector2 CalculateOffset(Vector2 delta)
    {
        if (MoveAxisType == AxisType.Horizontal) delta.y = 0;
        else delta.x = 0;

        return delta;
    }
    private void SetContentPosition(Vector2 position)
    {
        foreach (RectTransform i in m_viewRect)
        {
            i.localPosition += (Vector3)position;
        }
        return;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (!m_checkCache) return;

        this.m_dragging = false;
        this.m_isNormalizing = true;
        m_currentPos = CalcCorrectDeltaPos();
        m_currentStep = 0;
    }

    public virtual void Rebuild(CanvasUpdate executing)
    {
        return;
    }
    /// <summary>
    /// List是否处于可自由调整状态
    /// </summary>
    /// <returns></returns>
    public virtual bool EnsureListCanAdjust()
    {
        return !m_dragging && ContentIsLongerThanRect();
    }
    /// <summary>
    /// 内容是否比显示范围大
    /// </summary>
    /// <returns></returns>
    public virtual bool ContentIsLongerThanRect()
    {
        float contentLen;
        float rectLen;
        if (MoveAxisType == AxisType.Horizontal)
        {
            contentLen = CellCount * (cellSize.x + mSpacing.x) - mSpacing.x;
            rectLen = m_viewRect.rect.xMax - m_viewRect.rect.xMin;
        }
        else
        {
            contentLen = CellCount * (cellSize.y + mSpacing.y) - mSpacing.y;
            rectLen = m_viewRect.rect.yMax - m_viewRect.rect.yMin;
        }
        m_checkCache = contentLen > rectLen;
        return m_checkCache;
    }
    /// <summary>
    /// 检测边界情况，分为0未触界，-1左(下)触界，1右(上)触界
    /// </summary>
    /// <returns></returns>
    public virtual int GetBoundaryState()
    {
        RectTransform left;
        RectTransform right;
        left = GetChild(m_viewRect, 0);
        right = GetChild(m_viewRect, CellCount - 1);
        Vector3[] ver = new Vector3[4];
        left.GetWorldCorners(ver);
        Vector3[] r = new Vector3[4];
        right.GetWorldCorners(r);
        if (MoveAxisType == AxisType.Horizontal)
        {
            if (ver[0].x >= m_viewRectXMin) return -1;
            else if (r[3].x < m_viewRectXMax) return 1;
        }
        else
        {
            if (ver[0].y >= m_viewRectYMin) return -1;
            else if (r[1].y < m_viewRectYMax) return 1;
        }
        return 0;
    }
    /// <summary>
    /// Loop列表，分为-1把最右(上)边一个移到最左(下)边，1把最左(下)边一个移到最右(上)边
    /// </summary>
    /// <param name="dir"></param>
    protected virtual void LoopCell(int dir)
    {
        if (dir == 0) return;

        RectTransform MoveCell;
        RectTransform Tarborder;
        Vector2 TarPos;
        if (dir == 1)
        {
            MoveCell = GetChild(m_viewRect, 0);
            Tarborder = GetChild(m_viewRect, CellCount - 1);
            MoveCell.SetSiblingIndex(CellCount - 1);
        }
        else
        {
            Tarborder = GetChild(m_viewRect, 0);
            MoveCell = GetChild(m_viewRect, CellCount - 1);
            MoveCell.SetSiblingIndex(0);
        }
        if (MoveAxisType == AxisType.Horizontal)
        {
            TarPos = Tarborder.localPosition + new Vector3((cellSize.x + mSpacing.x) * dir, 0, 0);
        }
        else TarPos = (Vector2)Tarborder.localPosition + new Vector2(0, (cellSize.y + mSpacing.y) * dir);

        MoveCell.localPosition = TarPos;
    }
    /// <summary>
    /// 计算一个最近的正确位置
    /// </summary>
    /// <returns></returns>
    public virtual Vector2 CalcCorrectDeltaPos()
    {
        Vector2 delta = Vector2.zero;
        float distance = float.MaxValue;
        foreach (RectTransform i in m_viewRect)
        {
            var td = Mathf.Abs(i.localPosition.x) + Mathf.Abs(i.localPosition.y);
            if (td <= distance)
            {
                distance = td;
                delta = i.localPosition;
            }
            else break;

        }
        return delta;
    }
    /// <summary>
    /// 移动指定增量
    /// </summary>
    protected virtual void TweenToCorrect(Vector2 delta)
    {
        foreach (RectTransform i in m_viewRect)
        {
            i.localPosition += (Vector3)delta;
        }
    }

    private void SetPageIndex(int index)
    {
        for (int i = 0; i < m_pageList.Count; i++)
        {
            if (m_pageList[i] == null) continue;
            m_pageList[i].isOn = false;
            if (i == index) m_pageList[i].isOn = true;
        }
    }

    private static RectTransform GetChild(RectTransform parent, int index)
    {
        if (parent == null || index >= parent.childCount) return null;
        return parent.GetChild(index) as RectTransform;
    }

}