/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-07-17
 * 
 ***************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Level_3DClick : Level
{
    #region 设置玩家显示数量和默认是否显示玩家
    public static int defaultShowPlayerLimit { get; private set; }

    public static void Init3DDefalutData()
    {
        PresetTypes type = SettingsManager.recommend.type;

        //set original display state
        moduleLabyrinth.playerVisible = type > PresetTypes.Low;
        moduleBordlands.SetPlayerVisible(type > PresetTypes.Low);

        int index = type > PresetTypes.Fantasic ? (int)PresetTypes.Fantasic : (int)type;
        defaultShowPlayerLimit = GeneralConfigInfo.splayerLimitInScene.GetValue<int>(index);
        if (defaultShowPlayerLimit == 0) defaultShowPlayerLimit = 20;
    }
    #endregion

    public const float CHOOSE_MONSTER_DISTANCE = 20f;
    public const string headPanelName = "headpanel";

    protected Creature m_hero = null;
    private bool checkWhenEnded = false;
    public virtual bool checkClick { get { return true; } }

    private List<Transform> m_hitList = new List<Transform>();

    protected override void OnLoadComplete()
    {
        base.OnLoadComplete();

        enableUpdate = true;
        m_hitList.Clear();
    }

    public override void OnUpdate(int diff)
    {
        base.OnUpdate(diff);

        if(checkClick && m_hero) CheckChooseMonster();
    }

    #region choose the monster

    public void CheckChooseMonster()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN||UNITY_STANDALONE_OSX
        MousePick();
#else
        MobilePick();
#endif
    }

    private void MobilePick()
    {
        if (Input.touchCount != 1)
            return;

        TouchPhase phase = Input.GetTouch(0).phase;
        switch (phase)
        {
            case TouchPhase.Began:
                //按下的时候决定抬起手指时是否检测
                checkWhenEnded = CanChooseMonster() && !IsPointerOverUIObject();
                break;
            case TouchPhase.Ended:
                if (checkWhenEnded) CheckRayFormPos(Input.GetTouch(0).position);
                checkWhenEnded = false;
                break;

            case TouchPhase.Moved:
            case TouchPhase.Canceled:
                checkWhenEnded = false;
                break;
        }
    }


    /// <summary>
    /// Cast a ray to test if Input.mousePosition is over any UI object in EventSystem.current. 
    /// This is a replacement for IsPointerOverGameObject() which does not work on Android in 4.6/6.0.1
    /// </summary>
    /// <returns></returns>
    private static List<RaycastResult> RAYCAST_RESULTS = new List<RaycastResult>();
    private static bool IsPointerOverUIObject()
    {
        if (EventSystem.current == null) return false;

        // the ray cast appears to require only eventData.position.
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        RAYCAST_RESULTS.Clear();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, RAYCAST_RESULTS);
        return RAYCAST_RESULTS.Count > 0;
    }

    private void MousePick()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && CanChooseMonster() && EventSystem.current && !EventSystem.current.IsPointerOverGameObject())
        {
            //Logger.LogDetail("{0}按钮按下啦，，，，，，，",Time.realtimeSinceStartup);
            CheckRayFormPos(Input.mousePosition);
        }
    }

    protected virtual bool CanChooseMonster()
    {
        return true;//!m_sanctionPanel.enable && !m_detectPanel.enable && !m_rewardPanel.enable && !m_isAlertShow && !isMove;
    }

    private void CheckRayFormPos(Vector3 touchPos)
    {
        if (!current || !current.mainCamera) return;

        m_hitList.Clear();

        Ray ray = current.mainCamera.ScreenPointToRay(touchPos);
        RaycastHit[] hits = Physics.RaycastAll(ray, CHOOSE_MONSTER_DISTANCE, 1 << Layers.MODEL | 1 << Layers.DEFAULT );
        
        if (hits != null && hits.Length > 0)
        {
            foreach (var item in hits)
            {
                if (m_hitList.Contains(item.transform)) continue;

                m_hitList.Add(item.transform);
            }

            if(!CheckSceneObjFirst())
            {
                CheckPlayers();
            }
        }
        m_hitList.Clear();
    }

    /// <summary>
    /// 优先检查场景碰撞物体
    /// </summary>
    /// <returns></returns>
    private bool CheckSceneObjFirst()
    {
        for (int i = 0, count = m_hitList.Count; i < count; i++)
        {
            Transform trans = m_hitList[i];
            if(trans.gameObject.layer == Layers.DEFAULT)
            {
                moduleBordlands.DispatchClickSceneObjEvent(trans);
                return true;
            }
        }

        return false;
    }

    private bool CheckPlayers()
    {
        for (int i = 0, count = m_hitList.Count; i < count; i++)
        {
            Transform trans = m_hitList[i];
            if (trans?.parent?.name != "root") continue;

            GameObject creatureObj = trans?.parent?.parent?.gameObject;
            if (!creatureObj) continue;

            PVECreatureBehavior bc = creatureObj.GetComponent<PVECreatureBehavior>();
            if (bc != null)
            {
                moduleBordlands.DispatchClickPlayerEvent(bc);
                return true;
            }
        }

        return false;
    }
    #endregion
}
