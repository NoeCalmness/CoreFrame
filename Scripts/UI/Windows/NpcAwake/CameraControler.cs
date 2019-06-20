// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-02-18      13:27
//  *LastModify：2019-03-22      17:19
//  ***************************************************************************************************/

#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

public enum ControlType
{
    None, //不可控制
    Global, //全局控制
    Focus, //聚焦
    Part //局部控制
}

[RequireComponent(typeof (EventTriggerListener))]
public class CameraControler : MonoBehaviour
{
    private const float             powerScale = 100;
    private readonly List<Vector2>  _controlRemember = new List<Vector2>();
    public  Camera                  _camera;
    private NpcNode                 _currentFocusNode;
    private bool                    _isDraging;
    private NpcNode                 _lastFocusNode;
    private float                   _moveTimer;
    private CameraData              _originData;
    [SerializeField]
    private float                   _power;

    public float                    angleThrold = 10f;
    public float                    powerThrold = 0.5f;
    public  AnimationCurve[]        attenuation = new AnimationCurve[2];

    [Tooltip("是否开启x轴旋转")] public bool AxisX;

    public Vector2                  axisXRange;

    public float                    focusAngleOffset;
    public int                      rememberFrames;

    [Range(0.1f, 10)] public float[] speed = new float[2];

    public float                    stopThrold = 0.3f;

    private Vector2                 _pointDataDelta;

    public ControlType controlType { get; private set; }

    private NpcNode currentFocusNode
    {
        get { return _currentFocusNode; }
        set
        {
            _currentFocusNode = value;
            if (value != null)
                _lastFocusNode = value;
        }
    }

    public float RuntimeSpeed
    {
        get { return controlType == ControlType.Global ? speed[0] : speed[1]; }
    }

    public AnimationCurve RuntimeAttenuation
    {
        get { return controlType == ControlType.Global ? attenuation[0] : attenuation[1]; }
    }

    private CameraData currentData
    {
        get { return currentFocusNode?.cameraData ?? default(CameraData); }
    }

    private void Awake()
    {
        if (_camera == null)
            _camera = (Level.current as Level_Home)?.npcAwakeCamera;

        var listener = GetComponent<EventTriggerListener>();
        listener.onPressBegin += OnBeginDrag;
        listener.onPressMove += OnDrag;
        listener.onPressEnd += OnDragEnd;

        _originData.position = _camera?.transform?.position ?? Vector3.zero;
        _originData.fov = _camera?.fieldOfView ?? 30;
        controlType = ControlType.Global;

        //数据有效性矫正
        axisXRange.x = Mathf.Clamp(axisXRange.x, -80, 80);
        axisXRange.y = Mathf.Clamp(axisXRange.y, -80, 80);
    }

    private void OnEnable()
    {
        if (Module_Awake.instance.isInitedAccompany)
            return;

        Module_Awake.instance.isInitedAccompany = true;
        currentFocusNode = null;
        FocusNpcNode(GetComponentInChildren<NpcNode>());
    }

    public void CloseTo(CameraData rData)
    {
        if (!enabled)
            return;
        enabled = false;
        if (ControlType.Focus == controlType && currentFocusNode == rData.node)
        {
            var npc = Module_Npc.instance.GetTargetNpc(currentFocusNode.npcId);
            if (!npc.isUnlockEngagement)
            {
                Module_Global.instance.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NpcAwakeUI, 8));
                enabled = true;
                return;
            }

            if (npc.fetterStage < npc.maxFetterStage)
            {
                Module_Global.instance.ShowMessage(ConfigText.GetDefalutString(TextForMatType.NpcAwakeUI, 10));
                enabled = true;
                return;
            }

            var autoFocus = gameObject.GetComponentDefault<CameraAutoFocus>();
            autoFocus.onFocusFinish.RemoveAllListeners();
            autoFocus.onFocusFinish.AddListener(() =>
            {
                enabled = true;
                controlType = ControlType.Part;
                currentFocusNode.OnCameraCloseTo();
            });
            autoFocus.StartFocus(_camera, rData);
            currentFocusNode.OnCameraStartCloseTo();

            RecordBackCameraData();
        }
        else if (ControlType.Part == controlType)
        {
            var autoFocusBack = gameObject.GetComponentDefault<CameraAutoFocus>();
            autoFocusBack.onFocusFinish.RemoveAllListeners();
            autoFocusBack.onFocusFinish.AddListener(() =>
            {
                enabled = true;
                controlType = ControlType.Focus;
                currentFocusNode.OnCameraFarAway();
            });
            autoFocusBack.StartFocus(_camera, _originData);
            currentFocusNode.OnCameraStartFarAway();
        }
        //如果没有聚焦，先自动聚焦
        else
        {
            FocusNpcNode(rData.node);
        }
    }


    public void FocusNpcNode(NpcNode rNode, float rTime = 1)
    {
        if (null == rNode || currentFocusNode == rNode)
        {
            enabled = true;
            return;
        }

        //自动聚焦后需要把惯性去掉
        _controlRemember.Clear();

        currentFocusNode?.OnLoseFocus();
        currentFocusNode = rNode;
        RecordBackCameraData();

        var autoFocus = gameObject.GetComponentDefault<CameraAutoFocus>();
        autoFocus.onFocusFinish.RemoveAllListeners();
        autoFocus.onFocusFinish.AddListener(() =>
        {
            enabled = true;
            controlType = ControlType.Focus;
            currentFocusNode.OnGetFocus();
        });
        autoFocus.StartFocus(_camera, _originData, rTime);
    }

    private void RecordBackCameraData()
    {
        var dir = currentFocusNode.transform.position - _originData.position;
        var q = Quaternion.Euler(new Vector3(0, -focusAngleOffset, 0));
        var r = Quaternion.LookRotation(q * dir);
        _originData.euler = r.eulerAngles;
    }

    public void OnBeginDrag(PointerEventData data)
    {
        if (!enabled) return;
        if (controlType == ControlType.Part) return;
        _isDraging = true;
    }

    public void OnDrag(PointerEventData data)
    {
        if (!enabled || !_isDraging) return;

        _moveTimer = stopThrold;
        if (_controlRemember.Count >= rememberFrames)
            _controlRemember.RemoveAt(0);
        //分辨率自适应速度
        data.delta *= 1280f/Screen.width;
        _pointDataDelta = data.delta;
        var delta = -data.delta;
        _controlRemember.Add(delta);
        CalcPower();
        _camera.transform.Rotate(Vector3.up, delta.x*RuntimeSpeed*Time.deltaTime, Space.World);
        if (AxisX && axisXRange.x < axisXRange.y)
        {
            _camera.transform.Rotate(Vector3.left, delta.y*RuntimeSpeed*Time.deltaTime);
            var e = _camera.transform.eulerAngles;
            e.x = Mathf.Repeat(e.x + 180, 360) - 180;
            _camera.transform.eulerAngles = new Vector3(Mathf.Clamp(e.x, axisXRange.x, axisXRange.y), e.y, 0);
        }
        PartControlAssertInBound();
    }

    private void PartControlAssertInBound()
    {
        if (controlType != ControlType.Part)
            return;

        var a = _camera.transform.eulerAngles;
        var x = Mathf.Repeat(a.x, 360);
        var y = Mathf.Repeat(a.y, 360);

        if ((currentData.leftBound < currentData.rightBound && y > currentData.leftBound && y < currentData.rightBound)
            ||
            (currentData.leftBound > currentData.rightBound && (y > currentData.leftBound || y < currentData.rightBound)))
        {
            y = Quaternion.Angle(Quaternion.Euler(0, currentData.leftBound, 0), _camera.transform.rotation) <
                Quaternion.Angle(Quaternion.Euler(0, currentData.rightBound, 0), _camera.transform.rotation)
                ? currentData.leftBound
                : currentData.rightBound;
        }

        if ((currentData.upBound < currentData.downBound && x > currentData.upBound && x < currentData.downBound)
            || (currentData.upBound > currentData.downBound && (x > currentData.upBound || x < currentData.downBound)))
        {
            x = Quaternion.Angle(Quaternion.Euler(currentData.upBound, 0, 0), _camera.transform.rotation) <
                Quaternion.Angle(Quaternion.Euler(currentData.downBound, 0, 0), _camera.transform.rotation)
                ? currentData.upBound
                : currentData.downBound;
        }

        _camera.transform.eulerAngles = new Vector3(x, y, 0);
    }

    public void OnDragEnd(PointerEventData go)
    {
        if (!enabled || !_isDraging) return;

        _isDraging = false;

        if (_lastFocusNode != null)
        {
            if(Mathf.Abs(_originData .euler.y - _camera.transform.eulerAngles.y) > angleThrold || _power > powerThrold)
                FocusNpcNode(GetNearesstNpcNode());
            else
                FocusNpcNode(_lastFocusNode);
        }
    }

    private void CalcPower()
    {
        _power = 0;
        foreach (var delta in _controlRemember)
            _power += delta.magnitude;
        _power /= _controlRemember.Count;

        _power /= powerScale;
    }

    private void Update()
    {
//        AutoFocus();

        if (_isDraging)
        {
            if (_moveTimer > 0)
                _moveTimer -= Time.deltaTime;
            if (_moveTimer <= 0 && _controlRemember.Count > 0)
                _controlRemember.Clear();
            return;
        }

        if (_power <= 0 || _controlRemember.Count <= 0)
            return;
        //惯性
        var delta = _controlRemember[_controlRemember.Count - 1];
        delta = delta.normalized;
        _camera.transform.Rotate(Vector3.up, delta.x*RuntimeSpeed*Time.deltaTime*_power*powerScale, Space.World);
        if (AxisX && axisXRange.x < axisXRange.y)
        {
            _camera.transform.Rotate(Vector3.left, delta.y*RuntimeSpeed*Time.deltaTime*_power*powerScale);

            var e = _camera.transform.eulerAngles;
            e.x = Mathf.Repeat(e.x + 180, 360) - 180;
            _camera.transform.eulerAngles = new Vector3(Mathf.Clamp(e.x, axisXRange.x, axisXRange.y), e.y, 0);
        }
        _power -= Time.deltaTime*RuntimeAttenuation.Evaluate(_power);

        PartControlAssertInBound();
    }

    /// <summary>
    ///     相机自动聚焦
    /// </summary>
    private void AutoFocus()
    {
        if (controlType == ControlType.Part)
            return;
        var ray = new Ray(_camera.transform.position, GetRay(_camera.transform.forward));
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 100, 1 << Layers.NpcAwakeNode))
        {
            var npcNode = hitInfo.transform.GetComponent<NpcNode>();
            if (npcNode == null)
                return;
            controlType = ControlType.Focus;
            if (npcNode == currentFocusNode)
                return;
            currentFocusNode?.OnLoseFocus();
            currentFocusNode = npcNode;
            currentFocusNode.OnGetFocus();

            //保持居中
            FocusNpcNode(npcNode, 0.2f);
            return;
        }
        controlType = ControlType.Global;
        currentFocusNode?.OnLoseFocus();
        currentFocusNode = null;
    }

    private NpcNode GetNearesstNpcNode()
    {
        var angle = 360f;
        NpcNode node = null;
        var o = _power > powerThrold ? _pointDataDelta.x : GetAngleWithCamera(_lastFocusNode);
        foreach (var n in NpcNode.nodeList)
        {
            if (n == _lastFocusNode) continue;
            var a = GetAngleWithCamera(n);
            if (a*o > 0)
                continue;
            a = Mathf.Abs(a);
            if (a < angle)
            {
                angle = a;
                node = n;
            }
        }
        return node;
    }

    /// <summary>
    /// 相机与目标之间的Y轴角度
    /// </summary>
    /// <param name="rNode"></param>
    /// <returns></returns>
    private float GetAngleWithCamera(NpcNode rNode)
    {
        var dir = rNode.transform.position - _camera.transform.position;
        var q = Quaternion.Euler(new Vector3(0, -focusAngleOffset, 0));
        var r = Quaternion.LookRotation(dir) * q;

        var a = r.eulerAngles.y - _camera.transform.eulerAngles.y;
        a = Mathf.Repeat(a + 180,360) - 180;
        return a;
    }

    private Vector3 GetRay(Vector3 v)
    {
        var q = Quaternion.Euler(new Vector3(0, focusAngleOffset, 0));

        return q * v;
    }
}