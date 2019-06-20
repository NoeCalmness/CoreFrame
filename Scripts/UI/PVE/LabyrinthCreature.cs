using DG.Tweening;
using UnityEngine;

public class LabyrinthCreature : PVECreatureBehavior
{
    public enum LabyrinthCreatureType
    {
        None,

        Self,

        /// <summary>
        /// 扎营
        /// </summary>
        OutLabyrinth,

        /// <summary>
        /// 迷宫中
        /// </summary>
        InLabyrinth,
        
        /// <summary>
        /// 在PVE关卡中
        /// </summary>
        InPveBattle,
    }

    protected override int moveMentKey { get { return Module_Labyrinth.instance.moveMentKey; } }

    //其余玩家移动相关变量
    private Vector2 randomMoveTime = new Vector2(3, 10);
    private Vector2 randomMoveRadius = new Vector2(1, 4);
    private float m_lastPlayerMoveTime = 0f;
    private float m_playerMoveInterval;
    public bool isCheckInterval = true;
    public Vector3 targetPos;
    private bool m_isMove = false;
    private bool m_isInit = false;

    private bool m_hasDispatchTrigger = false;
    private float m_triggerTime = 0f;
    
    [SerializeField]
    private LabyrinthCreatureType m_playerType;
    public LabyrinthCreatureType playerType
    {
        get { return m_playerType; }
        set
        {
            if (m_playerType == value) return;
            m_playerType = value;
            bool active = false;
            switch (m_playerType)
            {
                case LabyrinthCreatureType.Self: active = true; break;

                case LabyrinthCreatureType.OutLabyrinth:
                    active = Module_Labyrinth.instance.playerVisible;
                    ResetPlayerMoveData();
                    break;

                case LabyrinthCreatureType.InLabyrinth:
                    active = Module_Labyrinth.instance.playerVisible;
                    SetTargetPos(Vector3.zero);
                    break;
            }
            gameObject.SetActive(active);
        }
    }

    private PMazePlayer m_roleInfo;
    public PMazePlayer roleInfo
    {
        get { return m_roleInfo; }
        set
        {
            m_roleInfo = value;
            playerType = Module_Labyrinth.instance.GetLabyrinthCreatureType(m_roleInfo.roleId, m_roleInfo.mazeState);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!creature || playerType == LabyrinthCreatureType.InPveBattle || playerType == LabyrinthCreatureType.None) return;

        switch (playerType)
        {
            //玩家自己操作
            case LabyrinthCreatureType.Self:
                CheckMove();
                UpdateSelfMoveMent();
                break;

            //玩家直接插值动画
            case LabyrinthCreatureType.InLabyrinth:
                UpdateInLabyrinthPlayerMovement();
                break;

            //怪物间隔时间到了之后做随机移动
            case LabyrinthCreatureType.OutLabyrinth:
                UpdateOutlabyrinthPlayerMovement();
                break;
        }
    }

    #region other functions

    protected override void CreateHeadPanel()
    {
        base.CreateHeadPanel();
        m_headPanel.NameObj.gameObject.SetActive(true);
        
        if (roleInfo != null) m_headPanel.nameText.text = roleInfo.roleName;
        ResetHeadPanelRotation();
    }

    public void InitCreatureData(Creature creature,PMazePlayer info)
    {
        this.creature = creature;
        roleInfo = info;
        PropItemInfo weaponInfo = ConfigManager.Get<PropItemInfo>(info.fashion.weapon);
        if (weaponInfo) LoadPlayerRuntimeAnimator(Creature.GetAnimatorName(weaponInfo.subType, info.gender), weaponInfo.subType, info.gender);
        CreatePlayerCollider();
        InitPVECreatureBehaviour();
        switch (playerType)
        {
            //随机坐标
            case LabyrinthCreatureType.OutLabyrinth:
                float x = Random.Range((float)Level.current.edge.x, (float)Level.current.edge.y);
                float z = Random.Range((float)Level.current.zEdge.x, (float)Level.current.zEdge.y);
                creaturePos = new Vector3(x, 0, z);
                break;

            //服务器返回的坐标
            case LabyrinthCreatureType.InLabyrinth:
                creaturePos = info.pos == null ? Vector3.zero : info.pos.ToVector3();
                break;
        }
    }

    private  void CreatePlayerCollider()
    {
        CreateCollider(playerType == LabyrinthCreatureType.Self);
        if(playerType == LabyrinthCreatureType.Self && creature)
        {
            GameObject obj = creature.behaviour.collider_.gameObject;
            ColliderTriggerListener.AddEnterListener(obj,OnPlayerTriggerEnter);
            ColliderTriggerListener.AddStayListener(obj, OnPlayerTriggerStay);
            ColliderTriggerListener.AddExitListener(obj, OnPlayerTriggerExit);
        }
    }

    #endregion

    #region 其他玩家的移动操作

    private void UpdateInLabyrinthPlayerMovement()
    {
        if (targetPos == transform.position || !m_isInit)
        {
            PlayState(m_standHash);
            moveState = 0;
            return;
        }

        PlayState(m_runHash);
        if (m_isMove)
        {
            m_isMove = false;
            transform.DOKill();
            transform.DOMove(targetPos, Vector3.Distance(transform.position, targetPos) / (float)moveSpeed).SetEase(Ease.Linear);
            moveState = targetPos.x > transform.position.x ? 1 : -1;
        }

        creaturePos = transform.position;
    }

    private void UpdateOutlabyrinthPlayerMovement()
    {
        if (isCheckInterval)
        {
            //战斗状态就不在执行移动了 
            if (m_lastPlayerMoveTime >= 0 && Time.time - m_lastPlayerMoveTime >= m_playerMoveInterval)
            {
                //时间到了就直接执行
                GetPlayerTargetPos();
                SetTargetPos(targetPos);
                isCheckInterval = false;
            }
            else
            {
                PlayState(m_standHash);
                moveState = 0;
            }
            return;
        }

        PlayState(m_runHash);
        if (m_isMove)
        {
            m_isMove = false;
            transform.DOKill();
            transform.DOMove(targetPos, (Vector3.Distance(transform.position, targetPos) / (float)moveSpeed)).OnComplete(() =>
            {
                ResetPlayerMoveData();
            });
            moveState = targetPos.x > transform.position.x ? 1 : -1;
        }

        creaturePos = transform.position;
    }

    public void SetTargetPos(Vector3 pos)
    {
        targetPos = pos;
        CheckTurnBack(targetPos);
        m_isMove = true;
        m_isInit = true;
    }

    private void ResetPlayerMoveData()
    {
        isCheckInterval = true;
        m_playerMoveInterval = Random.Range(randomMoveTime.x, randomMoveTime.y);
        m_lastPlayerMoveTime = Time.time;
        moveState = 0;
    }

    private void GetPlayerTargetPos()
    {
        float radius = Random.Range(randomMoveRadius.x, randomMoveRadius.y);
        float x = GetTargetPosValue(transform.position.x, radius, true);
        float z = GetTargetPosValue(transform.position.z, radius, false);
        targetPos.Set(x, 0f, z);
    }

    #endregion

    #region 碰撞检测

    private void OnPlayerTriggerEnter(Collider other)
    {
        //Logger.LogInfo("OnTriggerEnter other is {0}",other.name);
        if (playerType != LabyrinthCreatureType.Self) return;

        m_triggerTime = Time.time;
        Logger.LogInfo("OnTriggerEnter m_triggerTime is {0}", m_triggerTime);
    }

    private void OnPlayerTriggerStay(Collider other)
    {
        //Logger.LogInfo("OnTriggerStay other is {0}", other.name);
        if (playerType != LabyrinthCreatureType.Self || m_hasDispatchTrigger) return;

        if(m_currentStateHash == m_runHash && m_triggerTime > 0f && Time.time - m_triggerTime >= GeneralConfigInfo.slabyrinthTriggerTime)
        {
            PlayState(m_standHash);
            OnPlayerTriggerScene();
            return;
        }

        if(m_currentStateHash == m_standHash)
        {
            OnPlayerTriggerScene();
        }
    }

    private void OnPlayerTriggerExit(Collider other)
    {
        Logger.LogInfo("OnTriggerExit other is {0}", other.name);
        if (playerType != LabyrinthCreatureType.Self) return;

        m_hasDispatchTrigger = false;
        m_triggerTime = 0;
    }

    private void OnPlayerTriggerScene()
    {
        Module_Labyrinth.instance?.DispatchClickEvent(this);
        m_hasDispatchTrigger = true;
    }
    #endregion
}
