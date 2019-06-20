/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * Description
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.0
 * Created:  2017-
 * 
 ***************************************************************************************************/

using UnityEngine;

public delegate void OnInputChanged(InputKey[] changedKeys, int count);
public delegate void OnInputEnableDisable(bool enabled);

/// <summary>
/// Input key action type definition
/// </summary>
public enum InputKeyType { Down = 0, Up = 1, Click = 2, DoubleClick = 3, Press = 4, Hold = 5 }

public enum TouchID { None = 0, Movement = 1, Touch = 2, TouchMove = 3, Count }

public class InputKey
{
    public int ID { get; private set; }
    /// <summary>
    /// The virtual button name bind to Unity Input Setting
    /// </summary>
    public string key { get; private set; }
    /// <summary>
    /// Bind to left or right
    /// </summary>
    public int bindDirection { get; private set; }
    /// <summary>
    /// Mutex key list
    /// </summary>
    public System.Collections.Generic.List<InputKey> mutexKeys => m_mutexKeys;
    /// <summary>
    /// Input name, used by UI display
    /// </summary>
    public string name { get; private set; }
    /// <summary>
    /// Input description, used by UI display
    /// </summary>
    public string desc { get; private set; }
    public InputKeyType type { get; private set; }
    public TouchID touchID { get; private set; }
    public int touchIndex { get; private set; }
    /// <summary>
    /// The input value used by state machine transition [key] parameter
    /// </summary>
    public int value { get; private set; }
    public int delay
    {
        get { return m_delay; }
        set
        {
            m_delay = value;
            floatDelay = m_delay * 0.001f;
        }
    }
    public float floatDelay { get; private set; }
    /// <summary>
    /// Is this input instant ?
    /// Instant input will reset every logic frame
    /// </summary>
    public bool isInstant { get; private set; }
    /// <summary>
    /// Input will reset every render frame (Update)
    /// </summary>
    public bool oneShot { get; private set; }
    /// <summary>
    /// Input fired in current frame ?
    /// </summary>
    public bool fired { get { return m_customFired || m_fired || m_touchFired; } }
    /// <summary>
    /// Input fired by custom state in current frame ?
    /// </summary>
    public bool customFired { get { return m_customFired; } }
    /// <summary>
    /// Input fired by keyboard in current frame ?
    /// </summary>
    public bool keyFired { get { return m_fired; } }
    /// <summary>
    /// Input fired by touch in current frame ?
    /// </summary>
    public bool touchFired { get { return m_touchFired; } }
    /// <summary>
    /// Current state
    /// 1 = Down, 2 = Up, 3 = Press
    /// </summary>
    public int state
    {
        get { return m_state; }
        set
        {
            m_state = value;

            m_fired = CheckFired(m_state);

            m_lastDownTime = value < 0 || value == 0 && type >= InputKeyType.Press || (type == InputKeyType.Press || type == InputKeyType.DoubleClick) && m_fired ? -1 : value == 1 ? Time.time : m_lastDownTime;
        }
    }

    /// <summary>
    /// Current touch state
    /// 1 = Begin, 2 = End, 3 = Stay
    /// </summary>
    public int touchState
    {
        get { return m_touchState; }
        set
        {
            //var ld = m_lastTouchDownTime;
            //if (ID == 8) Logger.LogError("ns = {0}, cs = {1}, Time.time = {2}, ld = {3}, delay = {4}, diff = {5}, fired = {6}", touchState, value, Time.time, ld, floatDelay, Time.time - ld, value == 1 && Time.time - ld <= floatDelay);

            m_touchState = value;

            m_touchFired = CheckFired(m_touchState, true);

            m_lastTouchDownTime = value < 0 || value == 0 && type >= InputKeyType.Press || (type == InputKeyType.Press || type == InputKeyType.DoubleClick) && m_touchFired ? -1 : value == 1 ? Time.time : m_lastTouchDownTime;
        }
    }

    private int  m_delay;

    private bool m_customFired;

    private bool m_fired;
    private int  m_state;
    /// <summary>
    /// Last down event time, used to check up, click, press and hold event
    /// </summary>
    private float m_lastDownTime = 0;

    private bool m_touchFired;
    private int  m_touchState;
    /// <summary>
    /// Last down event time, used to check up, click, press and hold event
    /// </summary>
    private float m_lastTouchDownTime = 0;

    private string m_mutexKeyIDs = string.Empty;

    private System.Collections.Generic.List<InputKey> m_mutexKeys = new System.Collections.Generic.List<InputKey>();

    public InputKey(int _ID = -1, string _key = "", InputKeyType _type = InputKeyType.Down, TouchID _touchID = 0, int _touchIndex = 0, int _delay = 0, int _value = 0, string _name = "", string _desc = "") { Set(_ID, _key, _type, _touchID, _touchIndex, _delay, _value, _name, _desc); }

    public InputKey(InputKeyInfo config)
    {
        if (!config) Set();
        else
        {
            Set(config.ID, config.virtualName, config.type, config.touchID, config.touchIndex, config.delay, config.value, config.name, config.desc);
            bindDirection = config.bindDirection;
            m_mutexKeyIDs = config.mutexKeys;
        }
    }

    public void Set(InputKeyInfo config)
    {
        if (!config) Set();
        else
        {
            Set(config.ID, config.virtualName, config.type, config.touchID, config.touchIndex, config.delay, config.value, config.name, config.desc);
            bindDirection = config.bindDirection;
            m_mutexKeyIDs = config.mutexKeys;
        }
    }

    public void Set(int _ID = -1, string _key = "", InputKeyType _type = InputKeyType.Down, TouchID _touchID = 0, int _touchIndex = 0, int _delay = 0, int _value = 0, string _name = "", string _desc = "")
    {
        ID            = _ID;
        key           = _key;
        type          = _type;
        touchID       = _touchID;
        touchIndex    = _touchIndex;
        delay         = _delay;
        value         = _value;
        name          = _name;
        desc          = _desc;
        bindDirection = 0;
        oneShot       = type < InputKeyType.Press;
        isInstant     = type != InputKeyType.Hold;

        m_lastDownTime      = 0;
        m_lastTouchDownTime = 0;

        m_customFired = false;
        m_fired       = false;
        m_touchFired  = false;

        m_mutexKeyIDs   = null;
        m_mutexKeys.Clear();
    }

    public void UpdateMutexList()
    {
        m_mutexKeys.Clear();

        var mks = Util.ParseString<int>(m_mutexKeyIDs);
        for (var i = 0; i < mks.Length; ++i)
        {
            if (mks[i] == ID) continue;
            var k = InputManager.GetKey(mks[i]);
            if (k != null) m_mutexKeys.Add(k);
        }
    }

    /// <summary>
    /// Reset current key to default initialize state
    /// </summary>
    public void Reset()
    {
        m_state             = 0;
        m_touchState        = 0;
        m_fired             = false;
        m_touchFired        = false;
        m_customFired       = false;
        m_lastDownTime      = -1;
        m_lastTouchDownTime = -1;
    }

    /// <summary>
    /// Force fired state
    /// </summary>
    public bool SetFired(bool _fired)
    {
        var _customFired = m_customFired;
        m_customFired = _fired;

        if (m_customFired) UpdateMutexTime();

        return _customFired ^ m_customFired;
    }

    /// <summary>
    /// Update current Touch and Key fired state
    /// <para>
    ///  See <see cref="UpdateState(int)"/>, <see cref="UpdateTouchState(int)"/>
    /// </para>
    /// </summary>
    /// <param name="_state">-1 = Use current state see <see cref="state"/></param>
    /// <returns></returns>
    public bool UpdateFiredState(int _state = -1)
    {
        return UpdateState(_state < 0 ? state : _state) | UpdateTouchState(_state < 0 ? touchState : _state);
    }

    /// <summary>
    /// Set current input state.
    /// return true if fired has changed, else return false
    /// </summary>
    /// <param name="_state">fired state changed ?</param>
    /// <returns></returns>
    public bool UpdateState(int _state)
    {
        var _fired = m_fired;
        state = _state;

        if (state == 1) UpdateMutexTime();

        return m_fired ^ _fired;
    }

    /// <summary>
    /// Set current input state.
    /// return true if fired has changed, else return false
    /// </summary>
    /// <param name="_state">fired state changed ?</param>
    /// <returns></returns>
    public bool UpdateTouchState(int _touchState)
    {
        var _touchFired = m_touchFired;
        touchState = _touchState;

        if (touchState == 1) UpdateMutexTime();

        return m_touchFired ^ _touchFired;
    }

    /// <summary>
    /// Reset down time
    /// </summary>
    /// <param name="type">0 = all  1 = touch  2 = key</param>
    public void ResetLastDownTime(int type = 0)
    {
        if (type == 0 || type == 1) m_lastTouchDownTime = -1;
        if (type == 0 || type == 2) m_lastDownTime = -1;
    }

    public bool CheckFired(int _state, bool touch = false)
    {
        var ld = touch ? m_lastTouchDownTime : m_lastDownTime;
        switch (type)
        {
            case InputKeyType.Down:        return _state == 1; 
            case InputKeyType.Up:          return _state == 2; 
            case InputKeyType.Click:       return _state == 2 && Time.time - ld <= floatDelay;
            case InputKeyType.DoubleClick: return _state == 1 && Time.time - ld <= floatDelay;
            case InputKeyType.Press:
            case InputKeyType.Hold:        return _state == 1 && floatDelay < 0 || _state == 3 && ld > 0 && (floatDelay < 0 || Time.time - ld >= floatDelay);
            default: break;
        }
        return false;
    }

    private void UpdateMutexTime()
    {
        foreach (var k in m_mutexKeys)
            k.ResetLastDownTime();
    }
}

[AddComponentMenu("HYLR/Utilities/Input Manager")]
public sealed class InputManager : SingletonBehaviour<InputManager>
{
    #region Static functions

    public static OnInputChanged onInputChanged;
    public static OnInputEnableDisable onInputEnableDisable;

    public static int inputKeyCount { get; private set; }

    public static bool initialized { get; private set; }

    /// <summary>
    /// Locked key
    /// </summary>
    public static int lockedKeyValue { get; set; }

    private static InputKey[] m_inputKeys    = new InputKey[] { };
    private static InputKey[] m_changedKeys  = new InputKey[] { };

    public static void Initialize()
    {
        if (initialized) return;
        initialized = true;

        LoadInputKeys();
    }

    private static void LoadInputKeys()
    {
        var keys = ConfigManager.GetAll<InputKeyInfo>();
        inputKeyCount = keys.Count;

        m_inputKeys    = new InputKey[inputKeyCount];
        m_changedKeys  = new InputKey[inputKeyCount];

        if (inputKeyCount < 1)
        {
            Logger.LogError("InputManager::Initialize: Could not load input keys from config [config_inputkeyinfos], table is empty!");
            return;
        }

        int realCount = 0;
        for (var i = 0; i < inputKeyCount; ++i)
        {
            var key = keys[i];
            if (string.IsNullOrEmpty(key.virtualName))
            {
                if (key.ID > 0)
                {
                    Logger.LogError("InputManager::Initialize: Invalid input key config:");
                    Logger.LogError("{0}", key.ToXml("item"));
                }
                continue;
            }

            m_inputKeys[realCount++] = new InputKey(key);
        }

        inputKeyCount = realCount;

        for (var i = 0; i < inputKeyCount; ++i)
            m_inputKeys[i].UpdateMutexList();

        if (inputKeyCount < 1)
            Logger.LogError("InputManager::Initialize: Could not load any input key from config [config_inputkeyinfos], all datas are invalid!");
    }

    /// <summary>
    /// Enable or disable input
    /// </summary>
    /// <param name="_enable"></param>
    public static void Enable(bool _enable = true)
    {
        if (!instance) return;
        instance.enabled = _enable;
    }

    public static InputKey[] GetKeys(ref int count)
    {
        count = inputKeyCount;
        return m_inputKeys;
    }

    public static InputKey GetKey(int ID)
    {
        var idx = m_inputKeys.FindIndex(0, inputKeyCount, k => k.ID == ID);
        return idx < 0 ? null : m_inputKeys[idx];
    }

    public static InputKey GetKey(string name)
    {
        var idx = m_inputKeys.FindIndex(0, inputKeyCount, k => k.name == name);
        return idx < 0 ? null : m_inputKeys[idx];
    }

    public static int GetKeyIndex(string name)
    {
        return m_inputKeys.FindIndex(0, inputKeyCount, k => k.name == name);
    }

    /// <summary>
    /// Set current touch id state
    /// state: 0 = none, 1 = begin, 2 = end, 3 = stay
    /// </summary>
    /// <param name="id"></param>
    /// <param name="state"></param>
    /// <param name="indexMask">Touch indexs</param>
    /// <param name="oneShot"></param>
    public static void SetTouchState(TouchID id, int state, int indexMask = 1, bool oneShot = true)
    {
        instance._SetTouchState(id, state, indexMask, oneShot);
    }

    /// <summary>
    /// Set an input button state
    /// </summary>
    /// <param name="fired">Button fired ?</param>
    /// <param name="oneShot">reset after current frame</param>
    public static void SetCustomButtonState(string button, bool fired, bool oneShot = true)
    {
        var idx = GetKeyIndex(button);
        if (idx < 0) return;

        instance.UpdateCustomInput(idx, fired, oneShot);
    }

    #endregion

    public bool randomMovement = false;
    public bool randomKey      = false;

    public double keyChance  = 0.04;
    public double moveChance = 0.03;
    public double stopChance = 0.01;
    public double turnChance = 0.02;

    /// <summary>
    /// Set current touch id state
    /// </summary>
    /// <param name="id"></param>
    /// <param name="state">1 = start, 2 = end, 3 = stay</param>
    private void _SetTouchState(TouchID id, int state, int indexMask = 1, bool oneShot = true)
    {
        //Logger.LogInfo("Touch {0} index {1} update to {2}", id, index, state);

        UpdateTouchInput(id, indexMask, state);

        if (oneShot && indexMask >= 0 && state != 0) UpdateTouchInput(id, indexMask, 0);
    }

    /// <summary>
    /// Get button input state at current frame
    /// </summary>
    /// <param name="button">The virtual button name</param>
    /// <returns>0 = identity 1 = press down 2 = release up 3 = pressed</returns>
    private int _GetButtonState(string button)
    {
        if (Input.GetButtonDown(button)) return 1;
        if (Input.GetButtonUp(button))   return 2;
        if (Input.GetButton(button))     return 3;

        return 0;
    }

    private void Update()
    {
        UpdateKeyInput();
    }

    private void OnEnable()
    {
        onInputEnableDisable?.Invoke(true);
    }

    private void OnDisable()
    {
        ResetInputKeys();
        onInputEnableDisable?.Invoke(false);
    }

    public void ResetInputKeys()
    {
        var changed = 0;
        for (var i = 0; i < inputKeyCount; ++i)
        {
            var inputKey = m_inputKeys[i];
            if (!inputKey.fired) continue;

            inputKey.Reset();
            m_changedKeys[changed++] = inputKey;
        }

        if (changed > 0) OnInputChanged(-1, changed);
    }

    private void UpdateCustomInput(int index, bool fired, bool oneShot)
    {
        if (!enabled) return;

        var changed = 0;
        var inputKey = m_inputKeys[index];
        if (inputKey.SetFired(fired)) AddChangedKey(inputKey, ref changed);
        OnInputChanged(2, changed);

        if (fired && oneShot)
        {
            inputKey.SetFired(false);

            changed = 0;
            AddChangedKey(inputKey, ref changed);
            OnInputChanged(2, changed);
        }
    }

    private void UpdateKeyInput()
    {
        // @TODO: Lock input from script
        #if !UNITY_EDITOR
        return;
        #endif

        if (!enabled) return;

        var changed = 0;
        for (var i = 0; i < inputKeyCount; ++i)
        {
            var inputKey = m_inputKeys[i];
            var s = _GetButtonState(inputKey.key);

            if (s == 0)
            {
                if (randomKey && inputKey.value < 30 && Random.Range(0, 1.0f) < keyChance) s = Random.Range(1, 4);
                if (randomMovement && inputKey.value > 29)
                {
                    if (inputKey.fired && Random.Range(0, 1.0f) < turnChance) s = 0;
                    else if (inputKey.fired && Random.Range(0, 1.0f) < stopChance) s = 0;
                    else if (Random.Range(0, 1.0f) < moveChance) s = 3;
                }
            }

            if (inputKey.UpdateState(s)) AddChangedKey(inputKey, ref changed);
        }

        if (changed > 0) OnInputChanged(0, changed);
    }

    private void UpdateTouchInput(TouchID id, int indexMask, int state)
    {
        if (!enabled || Module_Guide.inCheckConditionTime) return;

        var changed = 0;
        for (var i = 0; i < inputKeyCount; ++i)
        {
            var inputKey = m_inputKeys[i];
            if (inputKey.touchID != id || indexMask > -1 && !indexMask.BitMask(inputKey.touchIndex) || (lockedKeyValue > 0 && inputKey.value != lockedKeyValue)) continue;
            if (inputKey.UpdateTouchState(state)) AddChangedKey(inputKey, ref changed);
        }

        if (changed > 0) OnInputChanged(1, changed);
    }

    private void AddChangedKey(InputKey inputKey, ref int changed)
    {
        if (inputKey.type == InputKeyType.Hold && m_changedKeys.PushDistinct(inputKey, changed)) ++changed;
        else m_changedKeys[changed++] = inputKey;
        //Logger.LogInfo("Key {0} changed to {1} from touch with state {2}!", inputKey.name, inputKey.touchFired, state);
    }

    /// <summary>
    /// 0 = key, 1 = touch, other = reset
    /// </summary>
    /// <param name="type"></param>
    private void OnInputChanged(int type, int changed)
    {
        #if UNITY_EDITOR
        if (Root.logicPaused) return;
        #endif

        int cc = changed, mask0 = 0;
        for (var i = 0; i < cc; ++i)
        {
            var key = m_changedKeys[i];
            if (!key.fired || key.mutexKeys.Count < 1) continue;

            var mks = key.mutexKeys;
            foreach (var k in mks)
            {
                if (!k.fired || mask0.BitMask(k.ID)) continue;
                mask0 = mask0.BitMask(k.ID, true);

                if ((k.UpdateFiredState(0) || k.SetFired(false))) AddChangedKey(k, ref changed);
            }
        }

        onInputChanged?.Invoke(m_changedKeys, changed);
    }

    #region Editor helper

    #if UNITY_EDITOR
    private void Start()
    {
        EventManager.AddEventListener("EditorReloadConfig", OnEditorReloadConfig);
    }

    private void OnEditorReloadConfig(Event_ e)
    {
        var config = (string)e.param1;

        if (config == "config_inputkeyinfos")
            LoadInputKeys();
    }
    #endif

    #endregion
}
