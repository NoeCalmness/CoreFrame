/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Base web Request and Reply definitions
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.3
 * Created:  2019-03-13
 * 
 ***************************************************************************************************/

using UnityEngine;

public delegate void OnRequestComplete(WebReply reply);
public delegate void OnRequestComplete<T>(WebReply<T> reply);

public class WebReply
{
    /// <summary>
    /// 0 = OK, other = Failed Code
    /// </summary>
    public int code = 0;
}

public class WebReply<T> : WebReply
{
    public T data;
}

public class WebRequest : CustomYieldInstruction
{
    public WebRequest() { }

    public override bool keepWaiting { get { return !m_isDone; } }
    public bool isDone
    {
        get { return m_isDone; }
        set
        {
            if (m_isDone == value) return;
            m_isDone = value;

            if (m_isDone) onComplete?.Invoke(m_reply);
        }
    }

    public WebReply reply { get { return m_reply; } set { m_reply = value; } }
    public OnRequestComplete onComplete = null;

    private bool m_isDone = false;

    private WebReply m_reply = null;
}

public class WebRequest<T> : CustomYieldInstruction
{
    public WebRequest() { }

    public override bool keepWaiting { get { return !m_isDone; } }
    public bool isDone
    {
        get { return m_isDone; }
        set
        {
            if (m_isDone == value) return;
            m_isDone = value;

            if (m_isDone) onComplete?.Invoke(m_reply);
        }
    }

    public WebReply<T> reply { get { return m_reply; } set { m_reply = value; } }
    public OnRequestComplete<T> onComplete = null;

    private bool m_isDone = false;

    private WebReply<T> m_reply = null;
}