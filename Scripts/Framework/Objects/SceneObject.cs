/****************************************************************************************************
 * Copyright (C) 2017-2019 FengYunChuanShuo
 * 
 * The SceneObject class is the base class for all objects that can be added to game scene.
 * 
 * Author:   Y.Moon <chglove@live.cn>
 * Version:  0.1
 * Created:  2017-03-03
 * 
 ***************************************************************************************************/

using System;
using UnityEngine;
using AssetBundles;
using Object = UnityEngine.Object;

public interface IRenderObject
{
    /// <summary>
    /// MonoBehaviour.Update
    /// </summary>
    void OnRenderUpdate();
    /// <summary>
    /// MonoBehaviour.LateUpdate
    /// </summary>
    void OnPostRenderUpdate();
}

public class SceneObject : LogicObject
{
    #region Static functions

    public static T Create<T>(string name, string assetName, bool visible = true) where T : SceneObject
    {
        T obj = null;
        var go = Level.GetPreloadObjectFromPool(assetName);

        if (go)
        {
            obj = Create<T>(name, go, visible);
            return obj;
        }

        obj = Create<T>(name, assetName, Vector3.zero, Vector3.zero, visible);
        obj.SetName(name);

        return obj;
    }

    public static T Create<T>(string name, string assetName, Vector3 pos, Vector3 rot, bool visible = true) where T : SceneObject
    {
        T obj = null;
        var go = Level.GetPreloadObjectFromPool(assetName);
        if (!go) go = Level.GetPreloadObject(string.IsNullOrEmpty(assetName) ? "__null" : assetName);
        if (go)
        {
            go.transform.position    = pos;
            go.transform.eulerAngles = rot;

            obj = Create<T>(name, go, visible);
            return obj;
        }

        obj = _Create<T>(name);
        obj.m_visible = visible;
        obj.CreateGameObject(assetName, pos, rot);

        return obj;
    }

    public static T Create<T>(string name, GameObject gameObject, bool visible = true) where T : SceneObject
    {
        if (!gameObject) return null;
        var behaviour = gameObject.GetComponent<SceneObjectBehaviour>();
        if (behaviour && behaviour.sceneObject)
        {
            Logger.LogError("GameObject {0} already linked to a SceneObject {1}.", gameObject.name, behaviour.sceneObject.name);
            return null;
        }

        var obj = _Create<T>(name);
        obj.m_visible = visible;
        obj.CreateGameObject(gameObject);

        return obj;
    }

    public static SceneObject Create(string name, Type type, string assetName, bool visible = true)
    {
        return Create(name, type, assetName, Vector3.zero, Vector3.zero, visible);
    }

    public static SceneObject Create(string name, Type type, string assetName, Vector3 pos, Vector3 rot, bool visible = true)
    {
        if (type != typeof(SceneObject) && !type.IsSubclassOf(typeof(SceneObject)))
        {
            Logger.LogError("Could not create SceneObject [{0}] due to class type mismatch. Target type: [{1}]", name, type.Name);
            return null;
        }

        var obj = _Create(name, type) as SceneObject;
        obj.m_visible = visible;

        var go = Level.GetPreloadObject(assetName);
        if (go)
        {
            go.transform.position = pos;
            go.transform.eulerAngles = rot;

            obj.CreateGameObject(go);

            return obj;
        }

        obj.CreateGameObject(assetName, pos, rot);

        return obj;
    }

    public static SceneObject Create(string name, Type type, GameObject gameObject, bool visible = true)
    {
        if (!gameObject) return null;

        if (type != typeof(SceneObject) && !type.IsSubclassOf(typeof(SceneObject)))
        {
            Logger.LogError("Could not create SceneObject [{0}] due to class type mismatch. Target type: [{1}]", name, type.Name);
            return null;
        }

        var behaviour = gameObject.GetComponent<SceneObjectBehaviour>();
        if (behaviour && behaviour.sceneObject)
        {
            Logger.LogError("GameObject {0} already linked to a SceneObject {1}.", gameObject.name, behaviour.sceneObject.name);
            return null;
        }

        var obj = _Create(name, type) as SceneObject;
        obj.m_visible = visible;
        obj.CreateGameObject(gameObject);

        return obj;
    }

    #endregion

    public string assetName { get; private set; }
    public GameObject gameObject { get { return m_gameObject; } }
    public Transform transform { get { return m_transform; } }

    public Vector3 position { get { return transform.position; } set { transform.position = value; } }
    public Vector3 localPosition { get { return transform.localPosition; } set { transform.localPosition = value; } }
    public Vector3 eulerAngles { get { return transform.eulerAngles; } set { transform.eulerAngles = value; } }
    public Vector3 localEulerAngles { get { return transform.localEulerAngles; } set { transform.localEulerAngles = value; } }
    public Vector3 scale { get { return transform.lossyScale; } }
    public Vector3 localScale { get { return transform.localScale; } set { transform.localScale = value; } }
    public Vector3 lossyScale { get { return transform.lossyScale; } }
    public Quaternion rotation { get { return transform.rotation; } set { transform.rotation = value; } }
    public Quaternion localRotation { get { return transform.localRotation; } set { transform.localRotation = value; } }

    public bool visible
    {
        get { return m_visible; }
        set
        {
            var v = m_visible;
            m_visible = value;

            UpdateVisibility();

            if (v ^ m_visible) DispatchEvent(Events.ON_VISIBILITY_CHANGED);
        }
    }

    public bool enabledAndVisible { get { return enableUpdate && visible; } set { enableUpdate = value; visible = value; } }

    protected virtual Type behaviourType { get { return typeof(SceneObjectBehaviour); } }

    protected SceneObjectBehaviour m_baseBehaviour;

    private AssetLoader          m_loader;
    private GameObject           m_gameObject;
    private Transform            m_transform;

    private bool                 m_visible;

    public void SetName(string _name)
    {
        if (string.IsNullOrEmpty(_name)) return;

        name = _name;
        m_gameObject.name = name;
    }

    public virtual void UpdateVisibility()
    {
        if (!gameObject) return;
        gameObject.SetActive(m_visible);
    }

    protected void CreateGameObject(string _assetName, Vector3 pos, Vector3 rot)
    {
        assetName     = _assetName;
        m_loader      = AssetLoader.Create(null, name, _assetName, (loader) =>
        {
            m_gameObject = loader.loadedObject;
            m_transform = m_gameObject.transform;

            m_transform.position = pos;
            m_transform.eulerAngles = rot;

            m_gameObject.name = name;
            m_gameObject.SetActive(m_visible);

            AddToScene();
        });

        if (!m_loader.loadedObject)
        {
            m_gameObject = m_loader.gameObject;
            m_transform = m_gameObject.transform;

            m_transform.position = pos;
            m_transform.eulerAngles = rot;
            m_gameObject.name = name;
            m_gameObject.SetActive(m_visible);
        }
    }

    protected void CreateGameObject(GameObject obj)
    {
        assetName     = obj.name;
        m_gameObject  = obj;
        m_transform   = m_gameObject.transform;
        m_gameObject.name = name;

        m_gameObject.SetActive(m_visible);

        AddToScene();
    }

    private void AddToScene()
    {
        m_baseBehaviour = m_gameObject.GetComponentDefault(behaviourType) as SceneObjectBehaviour;
        m_baseBehaviour.sceneObject = this;

        OnAddedToScene();

        DispatchEvent(Events.SCENE_ADD_OBJECT);
    }

    protected virtual void OnAddedToScene() { }

    /// <summary>
    /// Called when gameobject enabled
    /// </summary>
    public virtual void OnEnable() { }

    /// <summary>
    /// Called when gameobject disabled
    /// </summary>
    public virtual void OnDisable() { }

    protected virtual Coroutine StartCoroutine(System.Collections.IEnumerator routin)
    {
        if (!gameObject.activeSelf) { Logger.LogWarning("Could not start coroutine because object [{0}] is inactived.", name); return null; }
        return m_baseBehaviour.StartCoroutine(routin);
    }

    protected virtual void StopCoroutine(Coroutine routine)
    {
        m_baseBehaviour.StopCoroutine(routine);
    }

    protected virtual void StopCoroutine(string methodName)
    {
        m_baseBehaviour.StopCoroutine(methodName);
    }

    protected virtual void StopAllCoroutines()
    {
        m_baseBehaviour.StopAllCoroutines();
    }

    #region Unity component integration

    public T GetComponent<T>() where T : Component
    {
        return gameObject.GetComponent<T>();
    }

    public T GetComponentDefault<T>() where T : Component
    {
        return gameObject.GetComponentDefault<T>();
    }

    public T GetComponent<T>(string path) where T : Component
    {
        var obj = transform.Find(path);
        if (obj) return obj.GetComponent<T>();
        return null;
    }

    public T GetComponentDefault<T>(string path) where T : Component
    {
        var obj = transform.Find(path);
        if (obj) return obj.GetComponentDefault<T>();
        return null;
    }

    public Component GetComponent(Type type)
    {
        return gameObject.GetComponent(type);
    }

    public Component GetComponentDefault(Type type)
    {
        return gameObject.GetComponentDefault(type);
    }

    public Component GetComponent(Type type, string path)
    {
        var obj = transform.Find(path);
        if (obj) return obj.GetComponent(type);
        return null;
    }

    public Component GetComponentDefault(Type type, string path)
    {
        var obj = transform.Find(path);
        if (obj) return obj.GetComponentDefault(type);
        return null;
    }

    #endregion

    protected override void OnDestroy()
    {
        if (m_loader) Object.Destroy(m_loader.gameObject);
        m_loader = null;

        if (m_baseBehaviour) m_baseBehaviour.sceneObject = null;
        m_baseBehaviour = null;

        if (m_gameObject)
        {
            m_gameObject.SetActive(false);
            if (m_gameObject.name.StartsWith("effect_") || m_gameObject.name.StartsWith("eff_"))
            {
                Level.BackEffect(m_gameObject);
            }
            else
            {
                Object.Destroy(m_gameObject);
            }
        }

        m_gameObject = null;
        m_transform = null;
        m_baseBehaviour = null;
    }
}
