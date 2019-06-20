using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnhanceScrollView : MonoBehaviour
{

    public AnimationCurve scaleCurve;//大小scale改变
    public AnimationCurve positionCurve;//位置position改变
    public AnimationCurve depthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

    public int startCenterIndex = 0;//开始的index=0
    public float cellWidth = 10f;//宽
    private float totalHorizontalWidth = 500.0f;//水平位置
    public float yFixedPositionValue = 46.0f;//垂直偏移

    //lerp Duration 移动动画持续
    public float lerpDuration = 0.2f;
    private float mCurrentDuration = 0.0f;
    private int mCenterIndex = 0;
    public bool enableLerpTween = true;

    private EnhanceItem curCenterItem;
    private EnhanceItem preCenterItem;

    private bool canChangeItem = true;
    private float dFactor = 0.2f;

    // 水平垂直差值
    private float originHorizontalValue = 0.1f;
    public float curHorizontalValue = 0.5f;

    // "depth" factor (2d widget depth or 3d Z value)
    private int depthFactor = 5;

    public Camera sourceCamera;

    //private int times = 0;
    public void EnableDrag(bool isEnabled)
    {
        //    if (times == 0)
        //    {
        //        Module_Forging.instance.WeaponOPen();
        //    }
        //    else
        //    {
        //        times = 0;
        //    }
    }

    // ScrollView中物体
    public List<EnhanceItem> listEnhanceItems;
    // 获得index
    private List<EnhanceItem> listSortedItems = new List<EnhanceItem>();

    private static EnhanceScrollView instance;

    public float SetCount = 5;
    public float SetScle = 0.5f;

    public static EnhanceScrollView GetInstance
    {
        get { return instance; }
    }

    void Awake()
    {
        instance = this;
    }

    //public void OPen()
    //{
    //    times = 1;
    //    Start();
    //}
    private void createcure()
    {
        float ss = (12 - SetCount) / 12;
        float Start = SetScle * ss;
        scaleCurve = AnimationCurve.Linear(0, SetScle + Start, 0.5f, 1f);
        scaleCurve.postWrapMode = WrapMode.PingPong;
        scaleCurve.preWrapMode = WrapMode.PingPong;

    }

    public void allstart()
    {
        createcure();
        canChangeItem = true;
        int count = listEnhanceItems.Count;
        dFactor = (Mathf.RoundToInt((1f / count) * 10000f)) * 0.0001f;
        mCenterIndex = count / 2;
        if (count % 2 == 0)
            mCenterIndex = count / 2 - 1;
        int index = 0;
        for (int i = count - 1; i >= 0; i--)
        {
            listEnhanceItems[i].CurveOffSetIndex = i;
            listEnhanceItems[i].CenterOffSet = dFactor * (mCenterIndex - index);
            listEnhanceItems[i].SetSelectState(false);
            GameObject obj = listEnhanceItems[i].gameObject;


            UDragEnhanceView script = obj.GetComponent<UDragEnhanceView>();
            if (script != null)
            {
                script.SetScrollView(this);

            }

            index++;
        }

        // set the center item with startCenterIndex
        if (startCenterIndex < 0 || startCenterIndex >= count)
        {
            Logger.LogError("## startCenterIndex < 0 || startCenterIndex >= listEnhanceItems.Count  out of index ##");
            startCenterIndex = mCenterIndex;
        }

        // sorted items
        listSortedItems = new List<EnhanceItem>(listEnhanceItems.ToArray());
        totalHorizontalWidth = cellWidth * count;
        curCenterItem = listEnhanceItems[startCenterIndex];
        curHorizontalValue = 0.5f - curCenterItem.CenterOffSet;
        LerpTweenToTarget(0f, curHorizontalValue, false);

        // 
        // enable the drag actions
        // 
        EnableDrag(true);
    }
    void Start()
    {
        // allstart();
    }

    private void LerpTweenToTarget(float originValue, float targetValue, bool needTween = false)
    {
        if (!needTween)
        {
            SortEnhanceItem();
            originHorizontalValue = targetValue;
            UpdateEnhanceScrollView(targetValue);
            this.OnTweenOver();
        }
        else
        {
            originHorizontalValue = originValue;
            curHorizontalValue = targetValue;
            mCurrentDuration = 0.0f;
        }
        enableLerpTween = needTween;
    }

    public void DisableLerpTween()
    {
        this.enableLerpTween = false;
    }

    /// 
    /// Update EnhanceItem state with curve fTime value
    /// 
    public void UpdateEnhanceScrollView(float fValue)
    {
        for (int i = 0; i < listEnhanceItems.Count; i++)
        {
            EnhanceItem itemScript = listEnhanceItems[i];
            float xValue = GetXPosValue(fValue, itemScript.CenterOffSet);
            float scaleValue = GetScaleValue(fValue, itemScript.CenterOffSet);
            if (scaleValue > 0.9581f)
            {
                itemScript.SetSelectGray(true);
            }
            else
            {
                itemScript.SetSelectGray(false);
            }
            float depthCurveValue = depthCurve.Evaluate(fValue + itemScript.CenterOffSet);
            itemScript.UpdateScrollViewItems(xValue, depthCurveValue, depthFactor, listEnhanceItems.Count, yFixedPositionValue, scaleValue);
        }
    }

    void Update()
    {
        if (enableLerpTween)
            TweenViewToTarget();
    }

    private void TweenViewToTarget()
    {
        mCurrentDuration += Time.deltaTime;
        if (mCurrentDuration > lerpDuration)
            mCurrentDuration = lerpDuration;

        float percent = mCurrentDuration / lerpDuration;
        float value = Mathf.Lerp(originHorizontalValue, curHorizontalValue, percent);
        UpdateEnhanceScrollView(value);
        if (mCurrentDuration >= lerpDuration)
        {
            canChangeItem = true;
            enableLerpTween = false;
            OnTweenOver();
        }
    }

    private void OnTweenOver()
    {
        if (preCenterItem != null)
            preCenterItem.SetSelectState(false);
        if (curCenterItem != null)
            curCenterItem.SetSelectState(true);

    }

    // Get the evaluate value to set item's scale
    private float GetScaleValue(float sliderValue, float added)
    {
        float scaleValue = scaleCurve.Evaluate(sliderValue + added);
        return scaleValue;
    }

    // Get the X value set the Item's position
    private float GetXPosValue(float sliderValue, float added)
    {
        float evaluateValue = positionCurve.Evaluate(sliderValue + added) * totalHorizontalWidth;
        return evaluateValue;
    }

    private int GetMoveCurveFactorCount(EnhanceItem preCenterItem, EnhanceItem newCenterItem)
    {
        SortEnhanceItem();
        int factorCount = Mathf.Abs(newCenterItem.RealIndex) - Mathf.Abs(preCenterItem.RealIndex);
        return Mathf.Abs(factorCount);
    }

    // sort item with X so we can know how much distance we need to move the timeLine(curve time line)
    static public int SortPosition(EnhanceItem a, EnhanceItem b) { return a.transform.localPosition.x.CompareTo(b.transform.localPosition.x); }

    private void SortEnhanceItem()
    {
        listSortedItems.Sort(SortPosition);
        for (int i = listSortedItems.Count - 1; i >= 0; i--)
            listSortedItems[i].RealIndex = i;
    }

    public void SetHorizontalTargetItemIndex(EnhanceItem selectItem, bool needTween = true)
    {
        if (!canChangeItem)
            return;

        if (curCenterItem == selectItem)
            return;

        canChangeItem = false;
        preCenterItem = curCenterItem;
        curCenterItem = selectItem;

        // calculate the direction of moving
        float centerXValue = positionCurve.Evaluate(0.5f) * totalHorizontalWidth;
        bool isRight = false;
        if (selectItem.transform.localPosition.x > centerXValue)
            isRight = true;

        // calculate the offset * dFactor
        int moveIndexCount = GetMoveCurveFactorCount(preCenterItem, selectItem);
        float dvalue = 0.0f;
        if (isRight)
        {
            dvalue = -dFactor * moveIndexCount;
        }
        else
        {
            dvalue = dFactor * moveIndexCount;
        }
        float originValue = curHorizontalValue;
        LerpTweenToTarget(originValue, curHorizontalValue + dvalue, needTween);

    }

    // Click the right button to select the next item.
    public void OnBtnRightClick()
    {
        if (!canChangeItem)
            return;
        int targetIndex = curCenterItem.CurveOffSetIndex + 1;
        if (targetIndex > listEnhanceItems.Count - 1)
            targetIndex = 0;
        SetHorizontalTargetItemIndex(listEnhanceItems[targetIndex]);
    }

    // Click the left button the select next next item.
    public void OnBtnLeftClick()
    {
        if (!canChangeItem)
            return;
        int targetIndex = curCenterItem.CurveOffSetIndex - 1;
        if (targetIndex < 0)
            targetIndex = listEnhanceItems.Count - 1;
        SetHorizontalTargetItemIndex(listEnhanceItems[targetIndex]);
    }

    public float factor = 0.001f;

    public void OnDragEnhanceViewMove(Vector2 delta)
    {
        // In developing
        if (Mathf.Abs(delta.x) > 0.0f)
        {
            //Module_Forging.instance.IsMove = true;
            curHorizontalValue += delta.x * factor;
            LerpTweenToTarget(0.0f, curHorizontalValue, false);
        }
    }
    public void OnDragEnhanceViewEnd()
    {
        //find closed item to be centered
        int closestIndex = 0;
        float value = (curHorizontalValue - (int)curHorizontalValue);
        float min = float.MaxValue;
        float tmp = 0.5f * (curHorizontalValue < 0 ? -1 : 1);
        for (int i = 0; i < listEnhanceItems.Count; i++)
        {
            float dis = Mathf.Abs(Mathf.Abs(value) - Mathf.Abs((tmp - listEnhanceItems[i].CenterOffSet)));
            if (dis < min)
            {
                closestIndex = i;
                min = dis;
            }
        }
        originHorizontalValue = curHorizontalValue;
        float target = ((int)curHorizontalValue + (tmp - listEnhanceItems[closestIndex].CenterOffSet));
        preCenterItem = curCenterItem;
        curCenterItem = listEnhanceItems[closestIndex];
        LerpTweenToTarget(originHorizontalValue, target, true);
        canChangeItem = false;
       // Module_Forging.instance.IsMove = false;
    }
}
