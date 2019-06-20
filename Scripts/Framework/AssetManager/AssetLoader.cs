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

using System.Collections;
using UnityEngine;

namespace AssetBundles
{
    public class AssetLoader : MonoBehaviour
    {
        public static AssetLoader Create(Transform parent, string name, string assetName, System.Action<AssetLoader> onLoad = null)
        {
            var obj = new GameObject(name);

            if (parent) obj.transform.SetParent(parent);

            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localEulerAngles = Vector3.zero;

            var loader = obj.GetComponentDefault<AssetLoader>();
            loader.m_onLoad = onLoad;
            loader.Load(assetName, onLoad);

            return loader;
        }

        public GameObject loadedObject { get { return m_loadedObject; } }
        public bool loading { get { return !m_loadedObject; } }

        private System.Action<AssetLoader> m_onLoad;

        private AssetLoadOperation m_operation;

        [SerializeField]
        private string m_assetName;

        private GameObject m_loadedObject;

        public void Load(string assetName, System.Action<AssetLoader> onLoad = null)
        {
            StopAllCoroutines();

            m_onLoad = onLoad;

            m_assetName = assetName;
            m_operation = AssetManager.LoadAssetAsync(m_assetName, m_assetName, typeof(GameObject));

            Update();
        }

        private void Update()
        {
            if (m_operation == null || !m_operation.isDone) return;

            m_loadedObject = m_operation.GetAsset<GameObject>();
            m_operation = null;

            if (!m_loadedObject)
            {
                Logger.LogWarning("AssetManager: [{1}] Failed to load {0}.", m_assetName, name);
                m_loadedObject = new GameObject(m_assetName);
            }
            else
            {
                m_loadedObject = GameObject.Instantiate(m_loadedObject);
                m_loadedObject.name = m_assetName;
            }

            Util.AddChild(transform, m_loadedObject.transform);

            if (m_onLoad != null) m_onLoad(this);
            m_onLoad = null;

            AssetManager.UnloadAssetBundle(m_assetName);
        }

        private void OnDestroy()
        {
            AssetManager.UnloadAssetBundle(m_assetName);
        }
    }
}
