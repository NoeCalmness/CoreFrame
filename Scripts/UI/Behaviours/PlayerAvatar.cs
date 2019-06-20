/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Used for player avatar
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2018-03-20
 * 
 ***************************************************************************************************/

using UnityEngine;

public class PlayerAvatar : MonoBehaviour
{
    public bool useNativeSize
    {
        get { return m_useNativeSize; }
        set
        {
            if (m_useNativeSize == value) return;
            m_useNativeSize = value;

            var di = GetComponent<UIDynamicImage>();
            if (di) di.useNativeSize = m_useNativeSize;
        }
    }
    [SerializeField, Set("useNativeSize")]
    private bool m_useNativeSize = false;

    private void Start()
    {
        UpdateAvatar();

        EventManager.AddEventListener(Module_Player.EventAvatarChanged, UpdateAvatar);
    }

    private void OnDestroy()
    {
        EventManager.RemoveEventListener(this);
    }

    public void UpdateAvatar()
    {
        Module_Avatar.SetPlayerAvatar(gameObject, m_useNativeSize);
    }
}
