// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-07-25      16:49
//  * LastModify：2018-07-25      20:09
//  ***************************************************************************************************/

#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

public class AwakeHandle : SceneObject
{
    public const float DistanceLayer = 12;

    #region Events

    public const string FocusPointChange = "FocusPointChange";

    #endregion

    #region static functions

    public static int awakeLayer = LayerMask.NameToLayer("Awake");
    public static AwakeType CurrentType;
    public static AwakeHandle current;

    public static AwakeHandle Create(AwakeType rType, GameObject rGameObject)
    {
        if (current)
        {
            if (CurrentType == rType)
            {
                current.Refresh();
                return current;
            }
            current.Destroy();
        }
        CurrentType = rType;
        Util.SetLayer(rGameObject, awakeLayer);
        var handle = Create<AwakeHandle>(rType.ToString(), rGameObject);
        current = handle;
        return handle;
    }

    #endregion

    #region Fields

    private Dictionary<int, AwakeLayerController> Points;

    private AwakePageController controller;

    private int focusLayer;

    private int LoadState;


    public int FocusLayer
    {
        get { return focusLayer; }
        set
        {
            if (focusLayer != value)
            {
                if (focusLayer > 0 && Math.Abs(focusLayer - value) > 1)
                    return;

                focusLayer = value;
                Refresh();
            }
        }
    }


    #endregion

    #region functions

    public AwakeController GetAwakeController(int layer, int index)
    {
        if (!Points.ContainsKey(layer))
            return null;
        var layerController = Points[layer];
        return layerController.GetAwakeController(index);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Points != null)
            Points.Clear();
        else
            Points = new Dictionary<int, AwakeLayerController>();

        this.AddEventListener(FocusPointChange, this.OnFocusPointChange);
    }

    private void OnFocusPointChange(Event_ e)
    {
        var id = (int) e.param1;
        var current = ConfigManager.Get<AwakeInfo>(id);

        if (!current) return;


        DelayEvents.Add(() =>
        {
            if (current.nextInfoList.Count != 0)
            {
                if (!controller.isDraging)
                {
                    controller.curve = null;

                    var l = current.layer;
                    while (Module_Awake.instance.CanEnterNextLayer(CurrentType, l))
                    {
                        l += 1;
                    }
                    FocusLayer = l;
                    Watch(true);
                }
                else
                    Refresh();
            }

            var currentController = GetAwakeController(current.layer, current.index);
            if (currentController) currentController.RefreshState();

            var nextList = Module_Awake.NextAwakeInfo(id);
            foreach (var next in nextList)
            {
                var c = GetAwakeController(next.layer, next.index);
                if (c) c.RefreshState();
            }
        }, 0.3f);
    }

    protected override void OnAddedToScene()
    {
        base.OnAddedToScene();
        CreateController();

        focusLayer = moduleAwake.PrecentLayer(CurrentType);

        Refresh(() => Watch(false));
    }

    protected override void OnDestroy()
    {
        if (controller && controller.dragger)
        {
            Util.ClearChildren(controller.dragger);

            var tween = controller.dragger.GetComponent<TweenPosition3D>();
            if (tween) tween.enabled = false;
        }

        Points.Clear();
    }

    private void CreateController()
    {
        controller = gameObject.GetComponentDefault<AwakePageController>();
        controller.Handle = this;
    }

    private void Create(int layer, Action onSuccess = null)
    {
        if (layer < 0 || Points.ContainsKey(layer))
        {
            onSuccess?.Invoke();
            return;
        }

        if (layer > Module_Awake.instance.PrecentLayer(CurrentType))
        {
            onSuccess?.Invoke();
            return;
        }

        var rInfos = moduleAwake.GetInfosOnLayer(CurrentType, layer);
        if (rInfos.Count <= 0)
        {
            onSuccess?.Invoke();
            return;
        }
        var assets = new List<string>();
        assets.Add("awake_point");
        Level.PrepareAssets(assets, (b) =>
        {
            if (!b)
            {
                onSuccess?.Invoke();
                return;
            }

            var list = new List<AwakeController>();
            foreach (var info in rInfos)
            {
                var go = SceneObject.Create<SceneObject>(info.ID.ToString(), "awake_point", Vector3.zero, Vector3.zero);
                Util.SetLayer(go.gameObject, awakeLayer);
                var controller = go.GetComponentDefault<AwakeController>();
                controller.Init(info);
                list.Add(controller);
            }

            var layerController = LayerGameObject(layer);
            layerController.LayoutPoint(list);
            layerController.DrawLine(this);

            //每次重新生成层后需要把上一层连线重绘，因为上一层会有一条线连到这一层
            if (Points.ContainsKey(layer + 1))
                Points[layer + 1].DrawLine(this);

            onSuccess?.Invoke();
        });
    }

    private AwakeLayerController LayerGameObject(int layer)
    {
        if (Points.ContainsKey(layer))
            return Points[layer];

        var go = new GameObject(layer.ToString());
        Util.SetLayer(go, awakeLayer);
        go.transform.SetParent(controller.dragger);
        go.transform.localPosition = new Vector3(0, DistanceLayer*layer, 0);
        var layerController = go.GetComponentDefault<AwakeLayerController>();
        layerController.layer = layer;
        Points.Add(layer, layerController);
        return layerController;
    }

    public void Refresh(Action onSuccess = null)
    {
        var removeList = new List<int>();
        foreach (var kv in Points)
        {
            if (Mathf.Abs(kv.Key - focusLayer) > 2)
            {
                GameObject.DestroyImmediate(kv.Value.gameObject);
                removeList.Add(kv.Key);
            }
        }

        foreach(var layer in removeList)
            Points.Remove(layer);
        LoadState = 0;
        Create(focusLayer - 1, ()=> 
        {
            LoadState |= 1;
            if (LoadState == 7) onSuccess?.Invoke();
        });
        Create(focusLayer, () =>
        {
            LoadState |= 2;
            if (LoadState == 7) onSuccess?.Invoke();
        });
        Create(focusLayer + 1, () =>
        {
            LoadState |= 4;
            if (LoadState == 7) onSuccess?.Invoke();
        });

        controller.range = new Vector2(-moduleAwake.PrecentLayer(CurrentType) * DistanceLayer, 0);
    }

    private void Watch(bool smooth)
    {
        if (!Points.ContainsKey(FocusLayer))
            return;

        var layerController = Points[FocusLayer];
        var y = layerController != null ? layerController.transform.localPosition.y : 0;
        if (smooth)
            TweenPosition3D.Start(controller.dragger, 1, controller.dragger.localPosition, new Vector3(0, -y, 0));
        else
            controller.dragger.localPosition = new Vector3(0, -y, 0);
    }

    #endregion
}