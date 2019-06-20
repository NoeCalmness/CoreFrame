using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PVECreatureBehavior : MonoBehaviour
{
    #region custom class
    public class HeadPanel : CustomSecondPanel
    {
        public HeadPanel(Transform trans) : base(trans) { }

        public Text nameText;
        public GameObject NameObj;

        public Image battleImage;

        public override void InitComponent()
        {
            base.InitComponent();
            battleImage = rectTransform.Find("inbattle").GetComponent<Image>();
            nameText = rectTransform.Find("bg/Text").GetComponent<Text>();
            NameObj = rectTransform.Find("bg").gameObject;
            battleImage.gameObject.SetActive(false);
            nameText.gameObject.SetActive(true);
        }
    }
    #endregion

    #region const animator clip hash
    public const string STATE_RUN_NAME = "StateRun";
    public readonly static int STATE_STAND_HASH = Animator.StringToHash(Module_AI.STATE_STAND_NAME);
    public readonly static int STATE_RUN_HASH = Animator.StringToHash(STATE_RUN_NAME);
    #endregion

    #region fields

    protected virtual int moveMentKey { get { return 0; } } 

    public Creature creature { get; set; }
    public Animator animator { get; protected set; }
    public CreatureDirection direction { get; protected set; }
    public double moveSpeed = CombatConfig.sstandardRunSpeed;

    //动画hash值
    protected int m_runHash;
    protected int m_standHash;
    protected int m_currentStateHash;
    protected bool useStateMachine = true;

    public Vector3 creaturePos
    {
        get { return creature ? creature.position : Vector3.zero; }
        set
        {
            if (creature)
            {
                //creature.position_ = value;
                creature.position = value;
                m_scale = Module_Bordlands.GetCreatureScale(value.z);
                m_moveScale.Set(m_scale, m_scale, m_scale);
                creature.localScale = m_moveScale;
            }
        }
    }

    //自己操作相关变量
    protected Dictionary<int, KeyCode> m_valueKeyCodeMap;
    protected Vector3 m_moveMent;
    protected Vector3 m_movePos, m_moveScale;
    protected float m_scale;
    
    public Vector4 m_levelEdge;
    //头像相关
    protected HeadPanel m_headPanel;

    [SerializeField]
    private int m_moveState;
    /// <summary>
    /// 角色的移动状态
    /// -1 = 向左跑
    /// 0 = idle
    /// 1 = 向右跑
    /// </summary>
    public int moveState
    {
        get { return m_moveState; }
        protected set
        {
            if (value == m_moveState) return;

            m_moveState = value;
        }
    } 
    #endregion

    #region defalut functions
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();

        KeyCode[] keys = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
        int[] values = new int[] { 0x01, 0x02, 0x04, 0x08 };
        m_valueKeyCodeMap = new Dictionary<int, KeyCode>();
        for (int i = 0; i < keys.Length; i++)
        {
            m_valueKeyCodeMap.Add(values[i], keys[i]);
        }
    }

    protected virtual void OnEnable() { }

    protected virtual void Start() { }

    protected virtual void Update() { }
    #endregion

    #region functions

    protected virtual void CreateHeadPanel()
    {
        if (m_headPanel == null)
        {
            Transform t = transform.AddNewChild(Level.GetPreloadObject(Level_3DClick.headPanelName, false));
            m_headPanel = new HeadPanel(t);
            m_headPanel.rectTransform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            m_headPanel.rectTransform.anchoredPosition3D = new Vector3(0f, creature ? (float)creature.height : 1.82f, 0f);
        }
    }

    protected void ResetHeadPanelRotation()
    {
        if (m_headPanel == null) return;
        m_headPanel.rectTransform.localEulerAngles = direction == CreatureDirection.FORWARD ? new Vector3(0f, -90f, 0f) : new Vector3(0f, 90f, 0f);
    }

    protected void CheckTurnBack(KeyCode key)
    {
        if ((key == KeyCode.D && direction == CreatureDirection.BACK) ||
            (key == KeyCode.A && direction == CreatureDirection.FORWARD))
            TurnBack();
    }

    protected void CheckTurnBack(Vector3 targetPos)
    {
        if (transform == null) return;

        if ((targetPos.x > transform.position.x && direction == CreatureDirection.BACK) ||
            (targetPos.x < transform.position.x && direction == CreatureDirection.FORWARD))
            TurnBack();
    }

    protected void TurnBack()
    {
        transform.forward = -transform.forward;
        direction = direction == CreatureDirection.BACK ? CreatureDirection.FORWARD : CreatureDirection.BACK;

        ResetHeadPanelRotation();
    }

    protected void PlayState(int hash)
    {
        if (m_currentStateHash != hash)
        {
            animator.Play(hash);
            if (useStateMachine)
            {
                if (creature.stateMachine) creature.stateMachine.TranslateTo(hash == m_runHash ? STATE_RUN_NAME : Module_AI.STATE_STAND_NAME);
            }
            m_currentStateHash = hash;
        }
    }
    
    protected float GetTargetPosValue(float original, float radius, bool isPosX = true)
    {
        float minPos = original - radius;
        minPos = Mathf.Clamp(minPos, isPosX ? m_levelEdge.x : m_levelEdge.z, minPos);

        float maxPos = original + radius;
        maxPos = Mathf.Clamp(maxPos, maxPos, isPosX ? m_levelEdge.y : m_levelEdge.w);

        return Random.Range(minPos, maxPos);
    }
    
    public void LoadPlayerRuntimeAnimator(string animatorName, int weaponId, byte gender)
    {
        var ani = Level.GetPreloadObject<Object>(animatorName, false);
        if (animator == null) animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = (RuntimeAnimatorController)ani;
        animator.enabled = true;

        m_runHash = STATE_RUN_HASH;
        m_standHash = STATE_STAND_HASH;
        direction = CreatureDirection.FORWARD;
    }

    public void InitPVECreatureBehaviour()
    {
        m_levelEdge = Module_Bordlands.bordlandsEdge;
        if (m_levelEdge.z == m_levelEdge.w) m_levelEdge = new Vector4(m_levelEdge.x, m_levelEdge.y, -1.4f, 2f);
        CreateHeadPanel();
        if (animator)
        {
            m_currentStateHash = -1;
            PlayState(m_standHash);
        }
    }

    public void ResetToOriginal()
    {
        direction = CreatureDirection.FORWARD;
        creaturePos = Vector3.zero;
        transform.forward = direction == CreatureDirection.FORWARD ? Vector3.right : Vector3.left;
    }

    public void CreateCollider(bool isTrigger = false)
    {
        if (!creature || !creature.behaviour || !creature.behaviour.collider_) return;

        Transform mcTrans = creature.behaviour.collider_.transform;
        if (mcTrans != null)
        {
            mcTrans.gameObject.layer = Layers.MODEL;
            BoxCollider collider = mcTrans.GetComponentDefault<BoxCollider>();
            collider.size = new Vector3(1, 2, 1);
            collider.center = new Vector3(0, 1, 0);
            collider.isTrigger = isTrigger;
        }
    }
    #endregion

    #region self movement

    public void CheckMove()
    {
        m_moveMent.Set(0f, 0f, 0f);

        if (moveMentKey <= 0)
        {
            PlayState(m_standHash);
        }
        else
        {
            foreach (var item in m_valueKeyCodeMap)
            {
                int state = moveMentKey & item.Key;
                if (state > 0)
                {
                    //判断转向
                    PlayState(m_runHash);

                    //判断转向
                    CheckTurnBack(item.Value);

                    //操作移动变量
                    switch (item.Value)
                    {
                        case KeyCode.A:
                        case KeyCode.D:
                            m_moveMent += transform.forward;
                            break;
                        case KeyCode.S:
                            m_moveMent -= Vector3.forward;
                            break;
                        case KeyCode.W:
                            m_moveMent -= Vector3.back;
                            break;

                    }
                }
            }
        }
    }

    protected void UpdateSelfMoveMent()
    {
        moveState = m_moveMent.x == 0 ? 0 : m_moveMent.x < 0 ? -1 : 1;
        if (m_moveMent.Equals(Vector3.zero))
            return;

        //move
        m_movePos = transform.position;
        var moveDst = m_moveMent * (float)moveSpeed * Time.deltaTime;
        m_movePos += moveDst;

        // edge check
        if (m_levelEdge.x != 0 || m_levelEdge.x != m_levelEdge.y)
        {
            m_movePos.x = Mathf.Clamp(m_movePos.x, m_levelEdge.x, m_levelEdge.y);
        }
        if (m_levelEdge.z != 0 || m_levelEdge.z != m_levelEdge.w)
        {
            m_movePos.z = Mathf.Clamp(m_movePos.z, m_levelEdge.z, m_levelEdge.w);
        }
        m_movePos.y = 0;

        creaturePos = m_movePos;
    }
    #endregion
    
}
