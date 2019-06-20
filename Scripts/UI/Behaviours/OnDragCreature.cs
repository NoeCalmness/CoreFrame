using UnityEngine;
using UnityEngine.EventSystems;

public class OnDragCreature : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    #region 主界面摄像机
    public float leftMax = -5f;
    private float m_leftMax { get { return 360 + leftMax; } }
    public float rightMax = 5f;
    public float upMax = -20f;
    private float m_upMax { get { return 360 + upMax; } }
    public float downMax = 20f;

    public bool inertia = true;
    public float caSensitivity = 1f;
    public float stopTime = 0.2f;

    private Vector2 m_nowPosition;
    private bool m_enableUpdate;
    private Vector2 m_velocity;
    private Vector3 m_lastEuler;
    #endregion

    #region 拍照界面
    public float ctSensitivity = 1;
    #endregion

    private Transform m_target;
    private bool m_isInPhoto;

    private void Awake()
    {
        m_isInPhoto = false;
        m_target = Level.current?.mainCamera?.transform;

        EventManager.AddEventListener(CreatureEvents.PLAYER_ADD_TO_SCENE, OnPlayerAddToScene);
        EventManager.AddEventListener(LevelEvents.PHOTO_MODE_STATE,       OnPhotoModeState);
    }

    private void OnDestroy()
    {
        m_target = null;
        EventManager.RemoveEventListener(this);
    }

    private void OnPlayerAddToScene(Event_ e)
    {
        if (!m_isInPhoto) return;

        var player = e.sender as Creature;
        m_target = player?.transform;
    }

    private void OnPhotoModeState(Event_ e)
    {
        m_isInPhoto = (bool)e.param1;

        if (!m_isInPhoto && m_target) m_target.localEulerAngles = m_lastEuler;

        m_target = m_isInPhoto ? Level.current?.startPos ?? Level.currentMainCamera?.transform : Camera.main?.transform;

        if (m_isInPhoto && m_target) m_lastEuler = m_target.localEulerAngles;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (m_isInPhoto)
        {
            if (m_target == null) return;
            var r = m_target.localEulerAngles;

            r.y += -eventData.delta.x * ctSensitivity * 0.4f;
            m_target.localEulerAngles = r;
        }
        else
            SetTargetPosition(eventData.delta);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_enableUpdate = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.delta == Vector2.zero)
        {
            m_enableUpdate = false;
            return;
        }

        if (!m_isInPhoto)
        {
            m_nowPosition = eventData.delta;
            m_enableUpdate = true;
        }
    }

    private void LateUpdate()
    {
        if (inertia && m_enableUpdate && !m_isInPhoto)
        {
            if (m_nowPosition == Vector2.zero)
            {
                m_enableUpdate = false;
                return;
            }
            float xx = Mathf.SmoothDamp(m_nowPosition.x, 0, ref m_velocity.x, stopTime);
            float yy = Mathf.SmoothDamp(m_nowPosition.y, 0, ref m_velocity.y, stopTime);

            m_nowPosition.x = xx;
            m_nowPosition.y = yy;

            SetTargetPosition(m_nowPosition);
        }
    }

    private void SetTargetPosition(Vector2 delta)
    {
        if (m_target == null) return;
        var r = m_target.localEulerAngles;

        r.y += -delta.x * caSensitivity * Time.deltaTime;
        r.y = r.y > rightMax && r.y < 180 ? rightMax : r.y;
        r.y = r.y < m_leftMax && r.y > 180 ? m_leftMax : r.y;

        r.x += delta.y * caSensitivity * Time.deltaTime;
        r.x = r.x > downMax && r.x < 180 ? downMax : r.x;
        r.x = r.x < m_upMax && r.x > 180 ? m_upMax : r.x;

        m_target.localEulerAngles = r;
    }
}