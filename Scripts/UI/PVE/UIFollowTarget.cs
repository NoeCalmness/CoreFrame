// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2019-03-11      17:57
//  *LastModify：2019-03-11      17:58
//  ***************************************************************************************************/


using UnityEngine;

public class UIFollowTarget : MonoBehaviour
{
    public delegate void OnVisibilityChange(bool isVisible);

    /// <summary>
    /// Callback triggered every time the object becomes visible or invisible.
    /// </summary>

    public OnVisibilityChange onChange;

    /// <summary>
    /// 3D target that this object will be positioned above.
    /// </summary>

    public Transform target;
    
    /// <summary>
    /// follow target with ui offset
    /// </summary>
    public Vector3 offset;

    /// <summary>
    /// Game camera to use.
    /// </summary>

    public Camera gameCamera;

    /// <summary>
    /// UI camera to use.
    /// </summary>

    public Camera uiCamera;

    /// <summary>
    /// Whether the children will be disabled when this object is no longer visible.
    /// </summary>

    public bool disableIfInvisible = true;

    /// <summary>
    /// Destroy the game object when target disappears.
    /// </summary>

    public bool destroyWithTarget = true;

    Transform mTrans;
    int mIsVisible = -1;

    /// <summary>
    /// Whether the target is currently visible or not.
    /// </summary>

    public bool isVisible { get { return mIsVisible == 1; } }

    public Transform transformCache
    {
        get { return mTrans ?? (mTrans = transform); }
    }

    /// <summary>
    /// Cache the transform;
    /// </summary>

    void Awake() { mTrans = transform; }

    public void UpdateFrame()
    {
        LateUpdate();
    }

    /// <summary>
    /// Update the position of the HUD object every frame such that is position correctly over top of its real world object.
    /// </summary>

    private void LateUpdate()
    {
        if (target == null || transformCache == null)
        {
            return;
        }

        if (gameCamera == null)
            gameCamera = Level.current?.mainCamera;

        if (uiCamera == null || gameCamera == null)
            return;

        Vector3 pos = gameCamera.WorldToViewportPoint(target.position);
        // Determine the visibility and the target alpha
        int isVisible = (gameCamera.orthographic || pos.z > 0f) && 
                        (pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f) ? 1 : 0;
        bool vis = (isVisible == 1);

        // If visible, update the position
        if (vis)
        {
            var screenPos = RectTransformUtility.WorldToScreenPoint(gameCamera, target.position);

            RectTransformUtility.ScreenPointToWorldPointInRectangle(this.rectTransform(), screenPos, uiCamera, out pos);
            if(transformCache.parent)
                pos = transformCache.parent.InverseTransformPoint(pos);
            pos.z = 0;
            transformCache.localPosition = pos + offset;
        }

        // Update the visibility flag
        if (mIsVisible != isVisible)
        {
            mIsVisible = isVisible;

            if (disableIfInvisible)
            {
                for (var i = 0; i < transformCache.childCount; i++)
                    transformCache.GetChild(i).SafeSetActive(vis);
            }

            // Inform the listener
            onChange?.Invoke(vis);
        }
    }

    public static UIFollowTarget Start(Transform rUIObject, Transform rTarget)
    {
        if (rUIObject == null || rTarget == null)
            return null;

        var script = rUIObject.gameObject.GetComponentDefault<UIFollowTarget>();
        script.target = rTarget;
        script.gameCamera = Level.current.mainCamera;
        script.uiCamera = UIManager.worldCamera;
        script.destroyWithTarget = true;
        script.offset = new Vector3(0, 100, 0);
        return script;
    }
}