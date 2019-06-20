using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

public class BordlandsCreature : PVECreatureBehavior
{
    public enum BordLandCreatureType
    {
        None,

        Self,

        Player,

        Monster,
    }

    [HideInInspector]
    public BordLandCreatureType playerType = BordLandCreatureType.None;
    protected override int moveMentKey { get { return Module_Bordlands.instance.moveMentKey; } }

    //玩家移动相关变量
    private bool m_isInit = false;
    public Vector3 targetPos;
    private bool m_isMove = false;

    //怪物移动相关变量
    private Vector2 randomMoveTime = new Vector2(3,10);
    private Vector2 randomMoveRadius = new Vector2(1,4);
    private float m_lastMonsterMoveTime = 0f;
    private float m_monsterMoveInterval;
    public bool isCheckInterval = true;
    
    //数据相关
    public PNmlPlayer roleInfo;
    public PNmlMonster monsterData { get; set; }

    private bool m_isInBattle;
    public bool isInBattle
    {
        get
        {
            return m_isInBattle;
        }

        set
        {
            m_isInBattle = value;
            if (playerType == BordLandCreatureType.Monster)
            {
                m_lastMonsterMoveTime = m_isInBattle ? -1 : Time.realtimeSinceStartup;
            }
            if(m_headPanel != null)
            {
                m_headPanel.battleImage.gameObject.SetActive(m_isInBattle);
            }
        }
    }

    protected override void OnEnable()
    {
        isInBattle = false;
    }

    // Update is called once per frame
    protected override void Update ()
    {
        base.Update();
        //战斗状态都不移动
        if (isInBattle)
        {
            PlayState(m_standHash);
            return;
        }

        switch (playerType)
        {
            //玩家自己操作
            case BordLandCreatureType.Self:
                CheckMove();
                UpdateSelfMoveMent();
                break;

            //玩家直接插值动画
            case BordLandCreatureType.Player:
                UpdatePlayerMovement();
                break;
            
            //怪物间隔时间到了之后做随机移动
            case BordLandCreatureType.Monster:
                UpdateMonsterMovement();
                break;
        }
    }

    protected override void CreateHeadPanel()
    {
        base.CreateHeadPanel();   
        m_headPanel.NameObj.gameObject.SetActive(playerType != BordLandCreatureType.Monster);

        //todo 也需要检测怪物数据是否为空
        if (roleInfo != null) m_headPanel.nameText.text = playerType != BordLandCreatureType.Monster ? roleInfo.roleName : string.Empty;
        ResetHeadPanelRotation();

        if (playerType == BordLandCreatureType.Monster) ResetMonsterMoveData();
    }
    
    public void LoadMonsterRuntimeAnimator(string animatorName, PNmlMonster data)
    {
        playerType = BordLandCreatureType.Monster;
        monsterData = data;
        useStateMachine = false;

        var ani = Level.GetPreloadObject<Object>(animatorName,false);
        if (animator == null) animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = (RuntimeAnimatorController)ani;
        animator.enabled = true;
        
        m_standHash = Animator.StringToHash(data.standAction);
        m_runHash = Animator.StringToHash(data.moveAction);

        if (!animator.HasState(0, m_standHash)) Logger.LogError("[recv server data ID = {0} stand action = {1}] cannot be finded in animator_borderlandr", data.monster,data.standAction);
        if (!animator.HasState(0, m_runHash)) Logger.LogError("[recv server data ID = {0} move action = {1}] cannot be finded in animator_borderland", data.monster, data.moveAction);
    }

    public void SetMonsterRandomDir()
    {
        direction = Random.Range(0, 10) % 2 == 1 ? CreatureDirection.FORWARD : CreatureDirection.BACK;
        transform.forward = direction == CreatureDirection.FORWARD ? Vector3.right : Vector3.left;
    }
    
    #region player movement
    
    public void SetTargetPos(Vector3 pos)
    {
        targetPos = pos;
        CheckTurnBack(targetPos);
        
        m_isMove = true;
        m_isInit = true;
    }

    public void UpdatePlayerMovement()
    {
        if (targetPos == transform.position || !m_isInit)
        {
            PlayState(m_standHash);
            moveState = 0;
            return;
        }

        PlayState(m_runHash);
        if(m_isMove)
        {
            m_isMove = false;
            transform.DOKill();
            transform.DOMove(targetPos, Vector3.Distance(transform.position, targetPos) / (float)moveSpeed).SetEase(Ease.Linear);
            moveState = targetPos.x > transform.position.x ? 1 : -1;
        }

        creaturePos = transform.position;
    }
    #endregion

    #region monster move

    public void UpdateMonsterMovement()
    {
        if (isCheckInterval)
        {
            //战斗状态就不在执行移动了 
            if(m_lastMonsterMoveTime >= 0 && Time.realtimeSinceStartup - m_lastMonsterMoveTime >= m_monsterMoveInterval)
            {
                //时间到了就直接执行
                GetMonsterTargetPos();
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
            transform.DOMove(targetPos, (Vector3.Distance(transform.position, targetPos) / (float)moveSpeed)).OnComplete(()=> {
                ResetMonsterMoveData();
            });
            moveState = targetPos.x > transform.position.x ? 1 : -1;
        }

        creaturePos = transform.position;
    }

    private void ResetMonsterMoveData()
    {
        isCheckInterval = true;
        m_monsterMoveInterval = Random.Range(randomMoveTime.x, randomMoveTime.y);
        isInBattle = monsterData != null && monsterData.state == (sbyte)Module_Bordlands.EnumBordlandCreatureState.Fighting;
        moveState = 0;
    }

    private void GetMonsterTargetPos()
    {
        float radius = Random.Range(randomMoveRadius.x,randomMoveRadius.y);
        float x = GetTargetPosValue(transform.position.x, radius,true);
        float z = GetTargetPosValue(transform.position.z, radius, false);
        targetPos.Set(x, 0f, z);
    }

    #endregion
}
