// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-20      10:29
//  *LastModify：2018-12-20      15:38
//  ***************************************************************************************************/

#region

using System;
using UnityEngine;
using UnityEngine.Events;

#endregion

public class PreviewSlider
{
    private readonly ISliderHandle currentHandle;
    private readonly ISliderHandle previewHandle;

    private float currentValue;
    private float previewValue;

    private ISliderAnimControl currentAnim;
    private ISliderAnimControl previewAnim;

    public PreviewSlider(ISliderHandle rCurrent, ISliderHandle rPreview)
    {
        currentHandle = rCurrent;
        previewHandle = rPreview;
    }

    public void SetCurrent(float rValue)
    {
        currentValue = rValue;
        currentHandle.SliderValue = Mathf.Repeat(rValue, 1);
    }

    public void SetCurrentUniformAnim(float rValue, float rUniformSpeed01 = 1, Action rOnAnimEnd = null)
    {
        currentAnim = SliderAnimControlCenter.PlayUniformSpeed(currentHandle, currentValue, rValue, rUniformSpeed01, rOnAnimEnd);
        currentValue = rValue;
    }

    public void SetCurrentAnim(float rValue, float rDuration = 1, Action rOnAnimEnd = null)
    {
        currentAnim = SliderAnimControlCenter.Play(currentHandle, currentValue, rValue, rDuration, rOnAnimEnd);
        currentValue = rValue;
    }

    public void SetPreview(float rValue)
    {
        previewHandle.SliderValue = SliderAnimControlCenter.Repeat01(rValue - currentValue);
        previewValue = rValue;
    }

    public void SetPreviewUniformAnim(float rValue, float rUniformSpeed01 = 1, Action rOnAnimEnd = null)
    {
        previewAnim = SliderAnimControlCenter.PlayUniformSpeed(previewHandle, 
            previewHandle.SliderValue,
            Mathf.Clamp01(rValue - Mathf.FloorToInt(currentValue)), rUniformSpeed01, rOnAnimEnd);
        previewValue = rValue;
    }

    public void SetPreviewAnim(float rValue, float rDuration = 1, Action rOnAnimEnd = null)
    {
        previewAnim = SliderAnimControlCenter.Play(previewHandle,
            previewHandle.SliderValue,
            Mathf.Clamp01(rValue - Mathf.FloorToInt(currentValue)), rDuration, rOnAnimEnd);
        previewValue = rValue;
    }

    public void PlayUniformAnimToPreview(float rValue, float rUniformSpeed01 = 1, Action rOnAnimEnd = null)
    {
        previewValue = rValue;
        UnityAction<int> action = v =>
        {
            if (Mathf.FloorToInt(previewValue) == Mathf.FloorToInt(v))
            {
                previewHandle.SliderValue = previewValue - v;
            }
        };
        currentHandle.OnAnimToInteger.AddListener(action);
        SetCurrentUniformAnim(previewValue, rUniformSpeed01, () =>
        {
            currentHandle.OnAnimToInteger.RemoveListener(action);
            rOnAnimEnd?.Invoke();
        });
    }

    public void PlayAnimToPreview(float rValue, float rDuration = 1, Action rOnAnimEnd = null)
    {
        previewValue = rValue;
        UnityAction<int> action = v =>
        {
            if (Mathf.FloorToInt(previewValue) == Mathf.FloorToInt(v))
            {
                previewHandle.SliderValue = previewValue - v;
            }
        };
        currentHandle.OnAnimToInteger.AddListener(action);
        SetCurrentAnim(previewValue, rDuration, () =>
        {
            currentHandle.OnAnimToInteger.RemoveListener(action);
            rOnAnimEnd?.Invoke();
        });
    }

    public void Interrupt()
    {
        currentAnim?.Interrupt();
        previewAnim?.Interrupt();
    }

    public void Pause()
    {
        currentAnim?.Pause();
        previewAnim?.Pause();
    }

    public void Resume()
    {
        currentAnim?.Resume();
        previewAnim?.Resume();
    }
}
