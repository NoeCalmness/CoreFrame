// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-11      13:19
//  *LastModify：2018-12-11      14:46
//  ***************************************************************************************************/
#region

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#endregion

public class AnimToInteger : UnityEvent<int> { }

public interface ISliderHandle
{
    Transform Root { get; }
    float SliderValue { get; set; }
    AnimToInteger OnAnimToInteger { get; }
}

public interface ISliderAnimControl
{
    bool IsPlaying { get; }
    void Pause();
    void Resume();
    void Stop();
    void StartPlay();
    void Interrupt();
}

public class SliderAnimControlCenter : MonoBehaviour, ISliderAnimControl
{
    [SerializeField]
    private float duration;

    private ISliderHandle handle;

    private Action onAnimEnd;

    [SerializeField]
    private Vector2 sliderRange;

    [SerializeField]
    private float timer;

    private float value;
    public bool IsPlaying { get; private set; }

    public void Pause()
    {
        IsPlaying = false;
        enabled   = false;
    }

    public void Stop()
    {
        AnimComplete();
    }

    public void StartPlay()
    {
        enabled   = true;
        IsPlaying = true;
        timer     = 0;
    }

    public void Interrupt()
    {
        IsPlaying = false;
        enabled = false;
        onAnimEnd = null;
    }

    public void Resume()
    {
        IsPlaying = true;
        enabled   = true;
    }

    private void FixedUpdate()
    {
        if (!IsPlaying)
        {
            enabled = false;
            return;
        }

        timer += Time.fixedDeltaTime;

        var v = Mathf.Lerp(sliderRange.x, sliderRange.y, timer/duration);
        if (Mathf.FloorToInt(v) != Mathf.FloorToInt(value))
        {
            handle.OnAnimToInteger?.Invoke(Mathf.FloorToInt(v));
        }
        value = v;

        handle.SliderValue = value;
        if (timer >= duration)
        {
            AnimComplete();
        }
    }

    private void AnimComplete()
    {
        IsPlaying = false;
        enabled = false;
        onAnimEnd?.Invoke();
        onAnimEnd = null;
    }

    private void SetHandle(ISliderHandle rHandle)
    {
        handle = rHandle;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rHandle"></param>
    /// <param name="rStart"></param>
    /// <param name="rEnd"></param>
    /// <param name="rDuration">从start 到 end 需要花费的时间</param>
    /// <param name="rOnAnimEnd"></param>
    /// <returns></returns>
    public static ISliderAnimControl Play<T>(T rHandle, float rStart, float rEnd, float rDuration, Action rOnAnimEnd = null) where T : ISliderHandle
    {
        if (null == rHandle || !rHandle.Root)
            return default(ISliderAnimControl);
        var sliderAnim = rHandle.Root.gameObject.GetComponentDefault<SliderAnimControlCenter>();
        if (null == sliderAnim)
            return default(ISliderAnimControl);
        sliderAnim.SetHandle(rHandle);
        sliderAnim.duration = rDuration;
        sliderAnim.sliderRange = new Vector2(rStart, rEnd);
        sliderAnim.onAnimEnd = rOnAnimEnd;
        sliderAnim.StartPlay();
        return sliderAnim;
    }
    /// <summary>
    /// 匀速播放
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rHandle"></param>
    /// <param name="rStart"></param>
    /// <param name="rEnd"></param>
    /// <param name="rDuration">从0-1需要花费的时间</param>
    /// <param name="rOnAnimEnd"></param>
    /// <returns></returns>
    public static ISliderAnimControl PlayUniformSpeed<T>(T rHandle, float rStart, float rEnd, float rDuration,
        Action rOnAnimEnd = null) where T : ISliderHandle
    {
        return Play(rHandle, rStart, rEnd, rDuration * Mathf.Abs(rEnd - rStart), rOnAnimEnd);
    }
    /// <summary>
    /// 在（0, 1]之间重复。不同于Math.Repeat(v, 1) [0, 1).只用于slider
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static float Repeat01(float rValue)
    {
        if (rValue <= 1)
            return rValue;
        rValue -= Mathf.FloorToInt(rValue);
        if (Math.Abs(rValue) < float.Epsilon)
            rValue = 1;
        return rValue;
    }
}

#region ISliderHandle some instance

/// <summary>
/// slider value in [0, 1)
/// </summary>
public class ImageSliderAnim : ISliderHandle
{
    protected readonly Image image;

    public ImageSliderAnim(Image rImage)
    {
        image = rImage;
        Root = rImage?.transform;
    }

    public ImageSliderAnim(Image rImage, UnityAction<int> rOnAnimToInteger) : this(rImage)
    {
        OnAnimToInteger = new AnimToInteger();
        OnAnimToInteger.AddListener(rOnAnimToInteger);
    }

    public Transform Root { get; }

    public virtual float SliderValue
    {
        get { return image?.fillAmount ?? 0; }
        set { if(image) image.fillAmount = Mathf.Repeat(value, 1); }
    }

    public AnimToInteger OnAnimToInteger { get; }
}

/// <summary>
/// slider value in (0, 1]
/// </summary>
public class ImageSliderAnim2 : ImageSliderAnim
{
    public ImageSliderAnim2(Image rImage) : base(rImage)
    {
    }

    public ImageSliderAnim2(Image rImage, UnityAction<int> rOnAnimToInteger) : base(rImage, rOnAnimToInteger)
    {
    }
    public override float SliderValue
    {
        get { return image?.fillAmount ?? 0; }
        set { if (image) image.fillAmount = SliderAnimControlCenter.Repeat01(value); }
    }
}

/// <summary>
/// slider value in [0, 1)
/// </summary>
public class SliderAnim : ISliderHandle
{
    protected readonly Slider slider;

    public SliderAnim(Slider rSlider)
    {
        slider = rSlider;
        Root = rSlider?.transform;
    }

    public SliderAnim(Slider rSlider, UnityAction<int> rOnAnimToInteger) : this(rSlider)
    {
        OnAnimToInteger = new AnimToInteger();
        OnAnimToInteger.AddListener(rOnAnimToInteger);
    }

    public Transform Root { get; }

    public virtual float SliderValue
    {
        get { return slider?.value ?? 0; }
        set { if (slider) slider.value = Mathf.Repeat(value, 1) ;}
    }

    public AnimToInteger OnAnimToInteger { get; }

    public static implicit operator SliderAnim(Slider rSlider)
    {
        return new SliderAnim(rSlider);
    }
}
/// <summary>
/// SliderValue in (0, 1]
/// </summary>
public class SliderAnim2 : SliderAnim
{
    public SliderAnim2(Slider rSlider) : base(rSlider)
    {
    }

    public SliderAnim2(Slider rSlider, UnityAction<int> rOnAnimToInteger) : base(rSlider, rOnAnimToInteger)
    {
    }

    public override float SliderValue
    {
        get { return slider?.value ?? 0; }
        set { if (slider) slider.value = SliderAnimControlCenter.Repeat01(value); }
    }

    public static implicit operator SliderAnim2(Slider rSlider)
    {
        return new SliderAnim2(rSlider);
    }
}


/// <summary>
/// slider value in [0, 1)
/// </summary>
public class SliderAnim3 : SliderAnim
{
    public SliderAnim3(Slider rSlider) : base(rSlider)
    {
    }

    public SliderAnim3(Slider rSlider, UnityAction<int> rOnAnimToInteger) : base(rSlider, rOnAnimToInteger)
    {
    }

    public override float SliderValue
    {
        get { return slider?.value ?? 0; }
        set { if (slider) slider.value = value; }
    }

    public static implicit operator SliderAnim3(Slider rSlider)
    {
        return new SliderAnim3(rSlider);
    }
}

public class RuneSlider : ISliderHandle
{
    public AnimToInteger OnAnimToInteger { get; }
    public Transform Root { get; }
    public RectTransform parent { get; }

    protected float width;

    public RuneSlider(Transform root, RectTransform _parent, UnityAction<int> levelAction)
    {
        Root = root;
        parent = _parent;
        width = parent.rect.width;
        if (levelAction != null)
        {
            OnAnimToInteger = new AnimToInteger();
            OnAnimToInteger.AddListener(levelAction);
        }
    }

    public virtual float SliderValue
    {
        get { return (float)Root.rectTransform().sizeDelta.x / parent.sizeDelta.x; }
        set { Root.rectTransform().sizeDelta = new Vector2(width * Mathf.Repeat(value, 1), parent.rect.height); }
    }
}

public class RuneSlider2 : RuneSlider
{
    public RuneSlider2(Transform root, RectTransform _parent) : base(root, _parent, null)
    {

    }

    public override float SliderValue
    {
        get { return (float)Root.rectTransform().sizeDelta.x / parent.sizeDelta.x; }
        set { Root.rectTransform().sizeDelta = new Vector2(width * SliderAnimControlCenter.Repeat01(value), parent.rect.height); }
    }
}

#endregion