// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-24      10:29
//  *LastModify：2018-12-24      10:29
//  ***************************************************************************************************/

using UnityEngine;

public class PreviewSliderMono : MonoBehaviour
{
    public PreviewSlider Handle;

    public void Init(ISliderHandle rCurrent, ISliderHandle rPreview)
    {
        Handle = new PreviewSlider(rCurrent, rPreview);
    }

    private void OnDisable()
    {
        Handle.Interrupt();
    }
}
