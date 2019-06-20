// /****************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  * Author:     Noe  <chglove@live.cn>
//  * Version:    0.1
//  * Created:    2018-06-27      9:48
//  * LastModify：2018-07-12      16:08
//  ***************************************************************************************************/
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class ControllerEffect_ShowText {
    public static Pool<ControllerEffect_ShowText> pool = new Pool<ControllerEffect_ShowText>();
    public GameObject   NameObj;
    public Text         nameText;
    private Transform   root;
    private int         tweenCount;

    public static ControllerEffect_ShowText Create(CombatConfig.ShowText rInfo, Transform rParent)
    {
        var controller = pool.Pop();
        DelayEvents.Add(() =>
        {
            controller.Initialize(rInfo, rParent);
        }, rInfo.delay);
        return controller;
    }

    private void Initialize(CombatConfig.ShowText rInfo, Transform rParent)
    {
        if (rParent == null) return;

        Level.PrepareAsset<GameObject>(rInfo.assets, go =>
        {
            var temp = Level.GetPreloadObject(rInfo.assets, false);
            if (temp == null)
            {
                Logger.LogError("显示文本失败，找不到资源：{0}", rInfo.assets);
                return;
            }

            root = rParent.AddNewChild(temp);
            if (!root)
                return;

            nameText = root.Find("bg/Text").GetComponent<Text>();
            NameObj = root.Find("bg").gameObject;
            var board = NameObj.GetComponentDefault<Board>();
            board.target = Level.current.mainCamera;

            root.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            root.transform.localPosition = rInfo.offset;
            Util.SetText(nameText, rInfo.showText);
            nameText.gameObject.SetActive(true);

            var tweens = root.GetComponents<TweenBase>();
            tweenCount = tweens.Length;
            for (var i = 0; i < tweens.Length; i++)
            {
                var tween = tweens[i];
                tween.onComplete.AddListener(b =>
                {
                    if (--tweenCount <= 0)
                    {
                        Uninitialize();
                        pool.Back(this);
                    }
                });
                tween.PlayForward();
            }


            EventManager.AddEventListener(Events.CAMERA_SHOT_UI_STATE, OnCameraShotUIState);
        });
    }

    private void OnCameraShotUIState(Event_ e)
    {
        if (!root) return;
        var hide = (bool)e.param1;
        root.SafeSetActive(!hide);
        if (!hide)
        {
            var tweens = root.GetComponents<TweenBase>();
            if (tweens != null)
            {
                foreach (var tween in tweens)
                {
                    tween.Resum();
                }
            }
        }
    }

    private void Uninitialize()
    {
        Object.Destroy(root.gameObject);

        EventManager.RemoveEventListener(Events.CAMERA_SHOT_UI_STATE);
    }
}
