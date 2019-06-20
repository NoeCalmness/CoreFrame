// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-13      10:42
//  *LastModify：2018-12-13      14:17
//  ***************************************************************************************************/

#region

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#endregion

public class NpcGroup : AssertOnceBehaviour
{
    private Image                   _icon;
    private Transform               _lineEffect;
    private Text                    _relationships;
    private Button                  _button;

    public  NpcTypeID               npcId;
    private LineRenderer            _lineRenderer;

    [HideInInspector]
    public  UnityAction<NpcGroup>   onIconClick;

    protected override void Init()
    {
        _icon           = transform.GetComponent<Image>    ("mask/head_icon");
        _button           = transform.GetComponent<Button> ();
        _relationships  = transform.GetComponent<Text>     ("npcState");
        _lineEffect     = transform.GetComponent<Transform>("line");
        _lineRenderer   = _lineEffect?.GetComponentInChildren<LineRenderer>();
        _button?.onClick.AddListener(() =>
        {
            onIconClick?.Invoke(this);
        });
    }

    public void SetNpcInfo(INpcMessage rInfo, Transform targetPos)
    {
        if (rInfo == null)
            return;
        AssertInit();
        AtlasHelper.SetShared(_icon, rInfo.icon);
        SetRelationShip(rInfo.fetterStage);

        DrawLine(rInfo, targetPos);
    }

    private void DrawLine(INpcMessage rInfo, Transform targetPos)
    {
        if (!targetPos || !_lineRenderer || !(_lineEffect?.gameObject.activeInHierarchy ?? false))
            return;

        var ps = GeneralConfigInfo.defaultConfig.lineParams;
        if (ps.Length < 1)
        {
            Logger.LogError("NpcGroup::DrawLine: GeneralConfigInfo.lineParams is null.");
            return;
        }

        var idx = rInfo.fetterStage < 0 ? 0 : rInfo.fetterStage >= ps.Length ? ps.Length - 1 : rInfo.fetterStage;
        var p   = ps[idx];

        _lineRenderer.startWidth = p.startWidth;
        _lineRenderer.endWidth   = p.endWidth;
        _lineRenderer.startColor = p.startColor;
        _lineRenderer.endColor   = p.endColor;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.SetPositions(new[] {targetPos.position, _lineEffect.transform.position});
    }

    /// <summary>
    /// 设置亲密度
    /// </summary>
    /// <param name="rRelationShip">Npc关系等级</param>
    private void SetRelationShip(int rRelationShip)
    {
        var isGray = rRelationShip <= 0;
        transform.SetGray(isGray);
        _button.SetInteractable(!isGray);
		
        _lineEffect.SafeSetActive(!isGray);
        
        Util.SetText(_relationships, ConfigText.GetDefalutString(169, rRelationShip) );
    }

    /// <summary>
    /// 设置是否是命星
    /// </summary>
    /// <param name="rFate"></param>
    public void SetIsFate(bool rFate)
    {
        AssertInit();
    }
}
